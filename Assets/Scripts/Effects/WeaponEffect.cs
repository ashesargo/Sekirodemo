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
    public List<LayerEffectMapping> layerEffectMappings = new List<LayerEffectMapping>(); // 層級特效映射

    [Header("Effect Settings")]
    public GameObject defaultSparkPrefab; // 預設火花特效

    private Weapon weaponComponent; // Weapon 組件引用
    private bool isDetecting = false; // 是否正在檢測
    private bool isAttackActive = false; // 是否正在進行攻擊
    private HashSet<Collider> hitCollidersThisAttack = new HashSet<Collider>(); // 記錄本段攻擊已觸發的碰撞體
    private HashSet<Collider> validAttackTargets = new HashSet<Collider>(); // 本次攻擊的有效目標
    private List<GameObject> activeEffects = new List<GameObject>(); // 記錄當前活躍的特效

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
        isDetecting = true;
        hitCollidersThisAttack.Clear(); // 清空上一段攻擊的碰撞體記錄
        
        // 預先計算本次攻擊的有效目標
        PreCalculateValidTargets();
    }

    // 動畫用觸發事件
    public void StopDetection()
    {
        isDetecting = false;
        hitCollidersThisAttack.Clear(); // 清空碰撞體記錄
        isAttackActive = false; // 標記攻擊結束
        validAttackTargets.Clear(); // 清空有效目標列表        
        // 開始監控特效完成
        StartCoroutine(MonitorEffectsCompletion());
    }

    // 在指定位置生成火花特效
    public void SpawnSpark(Vector3 position, Vector3 normal, int hitLayer)
    {
        // 根據碰撞層級選擇對應的特效預製體
        GameObject effectPrefab = GetEffectPrefabForLayer(hitLayer);
        
        if (effectPrefab != null)
        {
            // 在指定位置生成特效，並根據法向量調整旋轉
            GameObject effect = Instantiate(effectPrefab, position, Quaternion.LookRotation(normal));
            activeEffects.Add(effect); // 將特效添加到活躍特效列表以便後續管理
            Destroy(effect, 1f); // 1秒後自動銷毀特效
        }
    }

    // 檢測武器 Collider 範圍內的所有 Collider 並記錄
    private void DetectCollisionAndSpawnSpark()
    {
        if (weaponCollider == null) return; // 如果沒有武器 Collider 則直接返回

        // 合併環境層和敵人層進行統一檢測
        LayerMask combinedLayer = environmentLayer | enemyLayer;
        
        // 使用武器 Collider 邊界進行碰撞檢測
        Collider[] hitColliders = Physics.OverlapBox
        (
            weaponCollider.bounds.center, // Collider 中心點
            weaponCollider.bounds.extents, // Collider 半徑
            weaponCollider.transform.rotation, // Collider 旋轉
            combinedLayer // 檢測的層級遮罩
        );

        foreach (Collider col in hitColliders)
        {
            if (hitCollidersThisAttack.Contains(col)) continue; // 跳過本段攻擊中已經處理過的 Collider

            // 如果是敵人 Collider，檢查是否在 Weapon 組件的攻擊範圍內
            if (IsEnemyCollider(col))
            {
                if (!IsInWeaponAttackRange(col)) continue; // 如果不在攻擊範圍內則跳過
            }

            // 處理碰撞並生成特效
            ProcessCollision(col, combinedLayer);
            hitCollidersThisAttack.Add(col); // 記錄已處理 Collider，避免重複觸發
        }
    }

    // 處理 Collider 的碰撞並生成火花特效
    private void ProcessCollision(Collider col, LayerMask combinedLayer)
    {
        // 計算 Collider 到武器 Collider 中心的最遠點
        Vector3 closestPoint = col.ClosestPoint(weaponCollider.bounds.center);

        // 從武器 Collider 中心向最遠點發射射線，獲取精確的碰撞點和法向量
        if (Physics.Raycast(weaponCollider.bounds.center, (closestPoint - weaponCollider.bounds.center).normalized, out RaycastHit hitInfo, weaponRange, combinedLayer))
        {
            // 射線成功擊中，使用射線的碰撞點和法向量生成火花特效
            SpawnSpark(hitInfo.point, hitInfo.normal, col.gameObject.layer);
        }
        else
        {
            // 射線檢測失敗，使用最遠點和計算的法向量作為備用方案生成火花特效
            Vector3 fallbackNormal = (closestPoint - weaponCollider.bounds.center).normalized;
            SpawnSpark(closestPoint, fallbackNormal, col.gameObject.layer);
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

    // 檢查 Collider 是否為敵人
    private bool IsEnemyCollider(Collider col)
    {
        return ((1 << col.gameObject.layer) & enemyLayer) != 0;
    }

    // 獲取對應層級的特效 Prefab
    private GameObject GetEffectPrefabForLayer(int layer)
    {
        foreach (LayerEffectMapping mapping in layerEffectMappings)
        {
            if (((1 << layer) & mapping.layerMask) != 0)
            {
                return mapping.effectPrefab;
            }
        }
        
        // 如果沒有找到對應的特效，返回預設特效
        return defaultSparkPrefab;
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
