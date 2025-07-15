using UnityEngine;
using System.Collections.Generic;

// 武器特效
public class WeaponEffect : MonoBehaviour
{
    public Collider weaponCollider; // 武器 collider
    public LayerMask environmentLayer;  // 判定層
    public GameObject sparkPrefab;  // 火花 prefab
    public float weaponRange = 2f;  // 武器檢測範圍

    private float nextSparkTime;  // 下次觸發火花時間
    private Animator animator;  // 動畫控制器

    // 記錄本段攻擊已觸發的碰撞體
    private HashSet<Collider> hitCollidersThisAttack = new HashSet<Collider>();

    private int lastAttackStateHash; // 上一幀攻擊狀態
    private bool wasAttackingLastFrame = false;

    void Start()
    {
        animator = GetComponentInParent<Animator>();
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isAttacking = stateInfo.IsTag("Attack");

        if (isAttacking && Time.time > nextSparkTime)
        {
            DetectCollisionAndSpawnSpark();
        }

        // 檢查是否換了攻擊段數（切換到不同動畫 clip）
        if (isAttacking)
        {
            int currentAttackStateHash = stateInfo.fullPathHash;

            if (!wasAttackingLastFrame || currentAttackStateHash != lastAttackStateHash)
            {
                // 進入新的攻擊段（或剛從非攻擊切入攻擊）
                hitCollidersThisAttack.Clear();
            }

            lastAttackStateHash = currentAttackStateHash;
            wasAttackingLastFrame = true;
        }
        else
        {
            wasAttackingLastFrame = false;
            lastAttackStateHash = 0;
            hitCollidersThisAttack.Clear();
        }
    }

    void DetectCollisionAndSpawnSpark()
    {
        if (weaponCollider == null || sparkPrefab == null) return;

        Vector3 weaponCenter = weaponCollider.transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(weaponCenter, weaponRange, environmentLayer);

        foreach (Collider col in hitColliders)
        {
            if (hitCollidersThisAttack.Contains(col)) continue;

            Vector3 closestPoint = col.ClosestPoint(weaponCenter);

            if (Physics.Raycast(weaponCenter, (closestPoint - weaponCenter).normalized, out RaycastHit hitInfo, weaponRange, environmentLayer))
            {
                SpawnSpark(hitInfo.point, hitInfo.normal);
            }
            else
            {
                Vector3 fallbackNormal = (closestPoint - weaponCenter).normalized;
                SpawnSpark(closestPoint, fallbackNormal);
            }

            hitCollidersThisAttack.Add(col);
        }

        nextSparkTime = Time.time;
    }

    public void SpawnSpark(Vector3 position, Vector3 normal)
    {
        GameObject spark = Instantiate(sparkPrefab, position, Quaternion.LookRotation(normal));
        Destroy(spark, 1f);
    }
}
