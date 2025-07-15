using UnityEngine;
using System.Collections.Generic;

// 武器特效
public class WeaponEffect : MonoBehaviour
{
    public Collider weaponCollider; // 武器 collider
    public LayerMask environmentLayer;  // 判定層
    public GameObject sparkPrefab;  // 火花 prefab
    public float weaponRange = 1f;  // 武器檢測範圍

    private bool isDetecting = false;  // 是否正在檢測
    private HashSet<Collider> hitCollidersThisAttack = new HashSet<Collider>();  // 記錄本段攻擊已觸發的碰撞體

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
    }

    void DetectCollisionAndSpawnSpark()
    {
        if (weaponCollider == null || sparkPrefab == null) return;

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
                SpawnSpark(hitInfo.point, hitInfo.normal);
            }
            else
            {
                Vector3 fallbackNormal = (closestPoint - weaponCollider.bounds.center).normalized;
                SpawnSpark(closestPoint, fallbackNormal);
            }

            hitCollidersThisAttack.Add(col);  // 記錄已觸發的碰撞體
        }
    }

    public void SpawnSpark(Vector3 position, Vector3 normal)
    {
        GameObject spark = Instantiate(sparkPrefab, position, Quaternion.LookRotation(normal));
        Destroy(spark, 1f);
    }
}
