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
    public LayerMask environmentLayer;  // 判定層
    public GameObject defaultSparkPrefab;  // 預設火花 prefab
    public List<LayerEffectMapping> layerEffectMappings = new List<LayerEffectMapping>();  // 層級特效映射
    public float weaponRange = 1f;  // 武器檢測範圍

    private bool isDetecting = false;  // 是否正在檢測
    private HashSet<Collider> hitCollidersThisAttack = new HashSet<Collider>();  // 記錄本段攻擊已觸發的碰撞體
    private List<GameObject> activeEffects = new List<GameObject>();  // 記錄當前活躍的特效

    // 事件：當所有特效完成時觸發
    public System.Action OnAllEffectsComplete;

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
    }

    // 動畫事件：停止檢測
    public void StopDetection()
    {
        isDetecting = false;
        hitCollidersThisAttack.Clear();  // 清空碰撞體記錄
        
        // 開始監控特效完成
        StartCoroutine(MonitorEffectsCompletion());
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

    void DetectCollisionAndSpawnSpark()
    {
        if (weaponCollider == null) return;

        // 使用 weaponCollider 的實際範圍進行碰撞檢測
        Collider[] hitColliders = Physics.OverlapBox(
            weaponCollider.bounds.center,
            weaponCollider.bounds.extents,
            weaponCollider.transform.rotation,
            environmentLayer
        );

        foreach (Collider col in hitColliders)
        {
            if (hitCollidersThisAttack.Contains(col)) continue;  // 跳過已觸發的碰撞體

            Vector3 closestPoint = col.ClosestPoint(weaponCollider.bounds.center);

            if (Physics.Raycast(weaponCollider.bounds.center, (closestPoint - weaponCollider.bounds.center).normalized, out RaycastHit hitInfo, weaponRange, environmentLayer))
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
