using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 武器碰撞特效
/// </summary>
public class WeaponEffect : MonoBehaviour
{
    // 層級特效映射
    [System.Serializable]
    public class LayerEffectMapping
    {
        public LayerMask layerMask;
        public GameObject effectPrefab;
    }

    [Header("Weapon Settings")]
    public Collider weaponCollider; // 武器碰撞器
    public float weaponRange = 1f; // 武器檢測範圍

    [Header("Layer Settings")]
    public LayerMask environmentLayer; // 環境判定層
    public LayerMask enemyLayer; // 敵人判定層
    public LayerMask playerLayer; // 新增：玩家判定層
    public List<LayerEffectMapping> layerEffectMappings = new List<LayerEffectMapping>(); // 層級特效映射

    [Header("Effect Settings")]
    public GameObject defaultSparkPrefab; // 預設火花特效

    private Weapon weaponComponent; // Weapon 組件引用
    private bool isDetecting = false; // 是否正在檢測
    private bool isAttackActive = false; // 是否正在進行攻擊
    private HashSet<Collider> hitCollidersThisAttack = new HashSet<Collider>(); // 記錄本段攻擊已觸發的碰撞體
    private HashSet<Collider> validAttackTargets = new HashSet<Collider>(); // 本次攻擊的有效目標
    private List<GameObject> activeEffects = new List<GameObject>(); // 記錄當前活躍的特效
    private bool hasTriggeredEffectThisAttack = false; // 新增：防止本次攻擊重複觸發特效
    private float lastStopDetectionTime = 0f; // 新增：記錄上次StopDetection的時間
    private const float STOP_DETECTION_COOLDOWN = 0.1f; // 新增：StopDetection冷卻時間
    private int effectCountThisAttack = 0; // 新增：本次攻擊的特效計數
    private const int MAX_EFFECTS_PER_ATTACK = 5; // 新增：每次攻擊最多觸發的特效數量

    public System.Action OnAllEffectsComplete; // 當所有特效完成時觸發事件

    void Start()
    {
        InitializeWeaponComponent();
    }

    void Update()
    {
        if (isDetecting)
        {
            DetectCollisionAndSpawnSpark();
        }
    }

    // 初始化 Weapon 組件
    private void InitializeWeaponComponent()
    {
        weaponComponent = GetComponent<Weapon>();
    }

    // 動畫用觸發事件
    public void StartDetection()
    {
        // 防止重複調用
        if (isDetecting) 
        {
            return;
        }
        
        isDetecting = true;
        hasTriggeredEffectThisAttack = false; // 重置特效觸發標記
        effectCountThisAttack = 0; // 重置特效計數器
        hitCollidersThisAttack.Clear(); // 清空上一段攻擊的碰撞體記錄
        
        // 預先計算本次攻擊的有效目標
        PreCalculateValidTargets();
        
        Debug.Log("[WeaponEffect] 開始檢測，特效計數器重置為: " + effectCountThisAttack);
    }

    // 動畫用觸發事件
    public void StopDetection()
    {
        // 防止重複調用 - 添加時間間隔檢查
        if (!isDetecting || Time.time - lastStopDetectionTime < STOP_DETECTION_COOLDOWN) 
        {
            return;
        }
        
        lastStopDetectionTime = Time.time;
        isDetecting = false;
        hitCollidersThisAttack.Clear(); // 清空碰撞體記錄
        isAttackActive = false; // 標記攻擊結束
        validAttackTargets.Clear(); // 清空有效目標列表
        hasTriggeredEffectThisAttack = false; // 重置特效觸發標記
        effectCountThisAttack = 0; // 重置特效計數器
        
        Debug.Log("[WeaponEffect] 停止檢測，本次攻擊生成特效數量: " + effectCountThisAttack);
        
        // 開始監控特效完成
        StartCoroutine(MonitorEffectsCompletion());
    }

    // 在指定位置生成火花特效
    public void SpawnSpark(Vector3 position, Vector3 normal, int hitLayer)
    {
        // 檢查特效數量限制
        if (effectCountThisAttack >= MAX_EFFECTS_PER_ATTACK)
        {
            Debug.Log("[WeaponEffect] 特效數量已達上限: " + effectCountThisAttack + "/" + MAX_EFFECTS_PER_ATTACK);
            return;
        }
        
        // 根據碰撞層級選擇對應的特效預製體
        GameObject effectPrefab = GetEffectPrefabForLayer(hitLayer);
        
        if (effectPrefab != null)
        {
            // 在指定位置生成特效，並根據法向量調整旋轉
            GameObject effect = Instantiate(effectPrefab, position, Quaternion.LookRotation(normal));
            activeEffects.Add(effect); // 將特效添加到活躍特效列表以便後續管理
            Destroy(effect, 1f); // 1秒後自動銷毀特效
            
            effectCountThisAttack++; // 增加特效計數
            hasTriggeredEffectThisAttack = true; // 標記已觸發特效
            
            Debug.Log("[WeaponEffect] 生成特效 #" + effectCountThisAttack + "，層級: " + LayerMask.LayerToName(hitLayer) + "，位置: " + position);
        }
        else
        {
            Debug.LogWarning("[WeaponEffect] 未找到層級 " + LayerMask.LayerToName(hitLayer) + " 的特效預製體");
        }
    }

    // 檢測武器 Collider 範圍內的所有 Collider 並記錄
    private void DetectCollisionAndSpawnSpark()
    {
        if (weaponCollider == null) return; // 如果沒有武器 Collider 則直接返回

        // 合併所有檢測層級
        LayerMask combinedLayer = environmentLayer | enemyLayer | playerLayer;
        
        // 使用武器 Collider 邊界進行碰撞檢測
        Collider[] hitColliders = Physics.OverlapBox
        (
            weaponCollider.bounds.center, // Collider 中心點
            weaponCollider.bounds.extents, // Collider 半徑
            weaponCollider.transform.rotation, // Collider 旋轉
            combinedLayer // 檢測的層級遮罩
        );

        bool hasPlayerHit = false; // 記錄是否有玩家碰撞
        bool hasGeneratedEffect = false; // 記錄是否已生成特效
        HashSet<GameObject> processedObjects = new HashSet<GameObject>(); // 記錄已處理的遊戲物件

        Debug.Log("[WeaponEffect] 檢測到 " + hitColliders.Length + " 個碰撞體");

        foreach (Collider col in hitColliders)
        {
            if (hitCollidersThisAttack.Contains(col)) continue; // 跳過本段攻擊中已經處理過的 Collider

            // 檢查碰撞體的層級
            int colliderLayer = col.gameObject.layer;
            bool isEnemy = ((1 << colliderLayer) & enemyLayer) != 0;
            bool isPlayer = ((1 << colliderLayer) & playerLayer) != 0;

            Debug.Log("[WeaponEffect] 處理碰撞體: " + col.name + "，層級: " + LayerMask.LayerToName(colliderLayer) + 
                     "，是否敵人: " + isEnemy + "，是否玩家: " + isPlayer);

            // 記錄是否有玩家碰撞
            if (isPlayer)
            {
                hasPlayerHit = true;
                Debug.Log("[WeaponEffect] 檢測到玩家碰撞: " + col.name);
            }

            // 如果是敵人 Collider，檢查是否在 Weapon 組件的攻擊範圍內
            if (isEnemy)
            {
                if (!IsInWeaponAttackRange(col)) 
                {
                    Debug.Log("[WeaponEffect] 敵人碰撞體 " + col.name + " 不在攻擊範圍內，跳過");
                    continue; // 如果不在攻擊範圍內則跳過
                }
            }

            // 檢查是否已經處理過這個遊戲物件的特效（避免同一敵人觸發多個特效）
            if (processedObjects.Contains(col.gameObject))
            {
                Debug.Log("[WeaponEffect] 已處理過遊戲物件 " + col.gameObject.name + "，跳過");
                hitCollidersThisAttack.Add(col); // 仍然記錄已處理的 Collider
                continue;
            }

            // 處理碰撞並生成特效
            if (ProcessCollision(col, combinedLayer))
            {
                hasGeneratedEffect = true;
                processedObjects.Add(col.gameObject); // 記錄已處理的遊戲物件
                Debug.Log("[WeaponEffect] 成功為 " + col.name + " 生成特效");
            }
            else
            {
                Debug.Log("[WeaponEffect] 為 " + col.name + " 生成特效失敗");
            }
            hitCollidersThisAttack.Add(col); // 記錄已處理 Collider，避免重複觸發
        }

        // 如果有玩家碰撞但沒有生成特效，強制在主角前方生成 playerLayer 特效
        if (hasPlayerHit && !hasGeneratedEffect)
        {
            Debug.Log("[WeaponEffect] 檢測到玩家碰撞但未生成特效，強制生成 playerLayer 特效");
            ForceSpawnPlayerLayerEffect();
        }
        else if (hasPlayerHit && hasGeneratedEffect)
        {
            Debug.Log("[WeaponEffect] 檢測到玩家碰撞且已生成特效");
        }
        else if (!hasPlayerHit)
        {
            Debug.Log("[WeaponEffect] 未檢測到玩家碰撞");
        }
    }

    // 處理 Collider 的碰撞並生成火花特效
    private bool ProcessCollision(Collider col, LayerMask combinedLayer)
    {
        // 檢查特效數量限制
        if (effectCountThisAttack >= MAX_EFFECTS_PER_ATTACK)
        {
            Debug.Log("[WeaponEffect] 特效數量已達上限，跳過處理碰撞體: " + col.name);
            return false;
        }
        
        // 計算 Collider 到武器 Collider 中心的最遠點
        Vector3 closestPoint = col.ClosestPoint(weaponCollider.bounds.center);

        // 從武器 Collider 中心向最遠點發射射線，獲取精確的碰撞點和法向量
        if (Physics.Raycast(weaponCollider.bounds.center, (closestPoint - weaponCollider.bounds.center).normalized, out RaycastHit hitInfo, weaponRange, combinedLayer))
        {
            // 射線成功擊中，使用射線的碰撞點和法向量生成火花特效
            SpawnSpark(hitInfo.point, hitInfo.normal, col.gameObject.layer);
            Debug.Log("[WeaponEffect] 射線擊中，為碰撞體 " + col.name + " 生成特效");
            return true;
        }
        else
        {
            // 射線檢測失敗，在主角前方生成火花特效
            Vector3 playerForward = transform.forward;
            Vector3 spawnPosition = transform.position + playerForward * weaponRange;
            Vector3 fallbackNormal = -playerForward; // 法向量指向主角
            SpawnSpark(spawnPosition, fallbackNormal, col.gameObject.layer);
            Debug.Log("[WeaponEffect] 射線未擊中，為碰撞體 " + col.name + " 在主角前方生成特效");
            return true;
        }
    }

    // 計算攻擊的有效目標
    private void PreCalculateValidTargets()
    {
        if (weaponComponent == null) return;
        
        validAttackTargets.Clear();
        isAttackActive = true;
        
        // 使用與 Weapon.PerformFanAttack 相同的邏輯來計算有效目標
        Collider[] hits = Physics.OverlapSphere(weaponComponent.transform.position, weaponComponent.attackRadius, weaponComponent.targetLayer);     
        foreach (var hit in hits)
        {
            Vector3 dirToTarget = (hit.transform.position - weaponComponent.transform.position).normalized;
            float angle = Vector3.Angle(weaponComponent.transform.forward, dirToTarget);
            
            if (angle <= weaponComponent.attackAngle * 0.5f)
            {
                validAttackTargets.Add(hit);
            }
        }
    }

    // 檢查 Collider 是否在 Weapon 的攻擊範圍內
    private bool IsInWeaponAttackRange(Collider targetCollider)
    {
        if (weaponComponent == null) return true; // 如果沒有 Weapon 組件，回傳 true
        
        // 如果正在進行攻擊，使用預先計算的有效目標列表
        if (isAttackActive && validAttackTargets.Contains(targetCollider))
        {
            return true;
        }
        
        // 即時計算攻擊範圍
        Vector3 weaponPosition = weaponComponent.transform.position;
        Vector3 targetPosition = targetCollider.transform.position;
        
        // 檢查距離
        float distance = Vector3.Distance(weaponPosition, targetPosition);
        if (distance > weaponComponent.attackRadius) return false;
        
        // 檢查角度
        Vector3 dirToTarget = (targetPosition - weaponPosition).normalized;
        float angle = Vector3.Angle(weaponComponent.transform.forward, dirToTarget);
        
        // 回傳是否在攻擊範圍內
        return angle <= weaponComponent.attackAngle * 0.5f;
    }

    // 獲取對應層級的特效 Prefab
    private GameObject GetEffectPrefabForLayer(int layer)
    {
        string layerName = LayerMask.LayerToName(layer);
        Debug.Log("[WeaponEffect] 查找層級 " + layerName + " (ID: " + layer + ") 的特效預製體");
        
        foreach (LayerEffectMapping mapping in layerEffectMappings)
        {
            if (((1 << layer) & mapping.layerMask) != 0)
            {
                Debug.Log("[WeaponEffect] 找到層級 " + layerName + " 的特效預製體: " + (mapping.effectPrefab != null ? mapping.effectPrefab.name : "null"));
                return mapping.effectPrefab;
            }
        }
        
        Debug.LogWarning("[WeaponEffect] 未找到層級 " + layerName + " 的特效預製體，使用預設特效");
        // 如果沒有找到對應的特效，返回預設特效
        return defaultSparkPrefab;
    }

    // 強制在主角前方生成 playerLayer 特效
    private void ForceSpawnPlayerLayerEffect()
    {
        // 檢查特效數量限制
        if (effectCountThisAttack >= MAX_EFFECTS_PER_ATTACK)
        {
            Debug.Log("[WeaponEffect] 特效數量已達上限，無法強制生成 playerLayer 特效");
            return;
        }
        
        // 在主角前方生成特效
        Vector3 playerForward = transform.forward;
        Vector3 spawnPosition = transform.position + playerForward * weaponRange;
        Vector3 fallbackNormal = -playerForward; // 法向量指向主角
        
        Debug.Log("[WeaponEffect] 嘗試強制生成 playerLayer 特效，位置: " + spawnPosition);
        
        // 使用 playerLayer 的特效
        GameObject playerEffectPrefab = GetEffectPrefabForLayer(LayerMask.NameToLayer("Player"));
        if (playerEffectPrefab != null)
        {
            GameObject effect = Instantiate(playerEffectPrefab, spawnPosition, Quaternion.LookRotation(fallbackNormal));
            activeEffects.Add(effect);
            Destroy(effect, 1f);
            
            effectCountThisAttack++;
            hasTriggeredEffectThisAttack = true;
            
            Debug.Log("[WeaponEffect] 成功強制在主角前方生成 playerLayer 特效");
        }
        else
        {
            Debug.LogWarning("[WeaponEffect] 未找到 playerLayer 特效預製體，嘗試使用預設特效");
            
            // 如果沒有找到 playerLayer 特效，使用預設特效
            if (defaultSparkPrefab != null)
            {
                GameObject effect = Instantiate(defaultSparkPrefab, spawnPosition, Quaternion.LookRotation(fallbackNormal));
                activeEffects.Add(effect);
                Destroy(effect, 1f);
                
                effectCountThisAttack++;
                hasTriggeredEffectThisAttack = true;
                
                Debug.Log("[WeaponEffect] 成功強制在主角前方生成預設特效");
            }
            else
            {
                Debug.LogError("[WeaponEffect] 無法生成任何特效：playerLayer 和預設特效都為 null");
            }
        }
    }

    // 監控特效狀態
    private IEnumerator MonitorEffectsCompletion()
    {
        // 等待所有特效完成
        while (activeEffects.Count > 0)
        {
            // 移除已銷毀的特效
            activeEffects.RemoveAll(effect => effect == null);
            yield return new WaitForSeconds(0.1f);
        }
        
        // 所有特效完成，觸發事件
        OnAllEffectsComplete?.Invoke();
        
        // 通知所有死亡敵人關閉 Collider
        NotifyDeadEnemiesToDisableColliders();
    }

    // 通知所有死亡敵人關閉 Collider
    private void NotifyDeadEnemiesToDisableColliders()
    {
        // 尋找所有死亡敵人
        HealthPostureController[] allControllers = FindObjectsOfType<HealthPostureController>();
        
        foreach (HealthPostureController controller in allControllers)
        {
            // 檢查是否為死亡敵人
            if (controller.GetHealthPercentage() <= 0)
            {
                controller.DisableCollider();
            }
        }
    }
}
