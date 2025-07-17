using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// 武器特效
public class WeaponEffect : MonoBehaviour
{
    [System.Serializable]
    public class LayerEffectMapping
    {
        public LayerMask layerMask;
        public GameObject effectPrefab;
    }

    public Collider weaponCollider; // 武器 collider
    public LayerMask environmentLayer;  // 環境判定層
    public LayerMask enemyLayer;  // 敵人判定層（新增）
    public GameObject defaultSparkPrefab;  // 預設火花 prefab
    public List<LayerEffectMapping> layerEffectMappings = new List<LayerEffectMapping>();  // 層級特效映射
    public float weaponRange = 1f;  // 武器檢測範圍

    // 新增：引用 Weapon 組件來檢查攻擊範圍
    private Weapon weaponComponent;
    
    private bool isDetecting = false;  // 是否正在檢測
    private HashSet<Collider> hitCollidersThisAttack = new HashSet<Collider>();  // 記錄本段攻擊已觸發的碰撞體
    private List<GameObject> activeEffects = new List<GameObject>();  // 記錄當前活躍的特效
    
    // 新增：攻擊同步相關變數
    private bool isAttackActive = false;  // 是否正在進行攻擊
    private HashSet<Collider> validAttackTargets = new HashSet<Collider>();  // 本次攻擊的有效目標

    // 事件：當所有特效完成時觸發
    public System.Action OnAllEffectsComplete;

    void Start()
    {
        // 獲取 Weapon 組件
        weaponComponent = GetComponent<Weapon>();
        if (weaponComponent == null)
        {
            // 如果當前物件沒有 Weapon 組件，嘗試從父物件獲取
            weaponComponent = GetComponentInParent<Weapon>();
        }
        
        // 如果沒有設定敵人層級，自動設定為 Layer 6（敵人層）
        if (enemyLayer == 0)
        {
            enemyLayer = 1 << 6; // Layer 6
        }
    }

    void Update()
    {
        if (isDetecting)
        {
            DetectCollisionAndSpawnSpark();
        }
    }

    // 動畫事件：開始檢測
    public void StartDetection()
    {
        isDetecting = true;
        hitCollidersThisAttack.Clear();  // 清空上一段攻擊的碰撞體記錄
        
        // 新增：預先計算本次攻擊的有效目標
        PreCalculateValidTargets();
    }

    // 動畫事件：停止檢測
    public void StopDetection()
    {
        isDetecting = false;
        hitCollidersThisAttack.Clear();  // 清空碰撞體記錄
        isAttackActive = false;  // 標記攻擊結束
        validAttackTargets.Clear();  // 清空有效目標列表
        
        // 開始監控特效完成
        StartCoroutine(MonitorEffectsCompletion());
    }

    // 新增：預先計算本次攻擊的有效目標
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

    // 監控特效完成
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
        
        // 通知所有死亡敵人關閉碰撞器
        NotifyDeadEnemiesToDisableColliders();
    }

    // 通知所有死亡敵人關閉碰撞器
    private void NotifyDeadEnemiesToDisableColliders()
    {
        // 尋找所有死亡敵人
        HealthPostureController[] allControllers = FindObjectsOfType<HealthPostureController>();
        
        foreach (HealthPostureController controller in allControllers)
        {
            // 檢查是否為死亡敵人（血量為0）
            if (controller.GetHealthPercentage() <= 0)
            {
                controller.DisableCollider();
            }
        }
    }

    // 新增：檢查是否在 Weapon 的攻擊範圍內
    private bool IsInWeaponAttackRange(Collider targetCollider)
    {
        if (weaponComponent == null) return true; // 如果沒有 Weapon 組件，預設允許播放特效
        
        // 如果正在進行攻擊，使用預先計算的有效目標列表
        if (isAttackActive && validAttackTargets.Contains(targetCollider))
        {
            return true;
        }
        
        // 否則使用即時計算（向後兼容）
        Vector3 weaponPosition = weaponComponent.transform.position;
        Vector3 targetPosition = targetCollider.transform.position;
        
        // 檢查距離
        float distance = Vector3.Distance(weaponPosition, targetPosition);
        if (distance > weaponComponent.attackRadius) return false;
        
        // 檢查角度
        Vector3 dirToTarget = (targetPosition - weaponPosition).normalized;
        float angle = Vector3.Angle(weaponComponent.transform.forward, dirToTarget);
        
        return angle <= weaponComponent.attackAngle * 0.5f;
    }

    // 新增：檢查碰撞體是否為敵人
    private bool IsEnemyCollider(Collider col)
    {
        return ((1 << col.gameObject.layer) & enemyLayer) != 0;
    }

    void DetectCollisionAndSpawnSpark()
    {
        if (weaponCollider == null) return;

        // 合併環境層和敵人層進行檢測
        LayerMask combinedLayer = environmentLayer | enemyLayer;
        
        // 使用 weaponCollider 的實際範圍進行碰撞檢測
        Collider[] hitColliders = Physics.OverlapBox(
            weaponCollider.bounds.center,
            weaponCollider.bounds.extents,
            weaponCollider.transform.rotation,
            combinedLayer
        );

        foreach (Collider col in hitColliders)
        {
            if (hitCollidersThisAttack.Contains(col)) continue;  // 跳過已觸發的碰撞體

            // 如果是敵人，檢查是否在 Weapon 的攻擊範圍內
            if (IsEnemyCollider(col))
            {
                if (!IsInWeaponAttackRange(col)) continue;
            }

            Vector3 closestPoint = col.ClosestPoint(weaponCollider.bounds.center);

            if (Physics.Raycast(weaponCollider.bounds.center, (closestPoint - weaponCollider.bounds.center).normalized, out RaycastHit hitInfo, weaponRange, combinedLayer))
            {
                SpawnSpark(hitInfo.point, hitInfo.normal, col.gameObject.layer);
            }
            else
            {
                Vector3 fallbackNormal = (closestPoint - weaponCollider.bounds.center).normalized;
                SpawnSpark(closestPoint, fallbackNormal, col.gameObject.layer);
            }

            hitCollidersThisAttack.Add(col);  // 記錄已觸發的碰撞體
        }
    }

    public void SpawnSpark(Vector3 position, Vector3 normal, int hitLayer)
    {
        // 根據碰撞層選擇對應的特效
        GameObject effectPrefab = GetEffectPrefabForLayer(hitLayer);
        
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, position, Quaternion.LookRotation(normal));
            activeEffects.Add(effect); // 添加到活躍特效列表
            Destroy(effect, 1f);
        }
    }

    // 根據層級獲取對應的特效預製體
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

    // 為了向後兼容，保留舊的方法
    public void SpawnSpark(Vector3 position, Vector3 normal)
    {
        SpawnSpark(position, normal, 0); // 使用預設層級
    }
}
