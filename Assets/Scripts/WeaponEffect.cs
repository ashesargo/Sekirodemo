using UnityEngine;

// 武器特效 
public class WeaponEffect : MonoBehaviour
{
    public Collider weaponCollider; // 武器 collider
    public LayerMask environmentLayer; // 判定層
    public GameObject sparkPrefab; // 火花 prefab
    public float weaponRange = 2f; // 武器檢測範圍

    private float nextSparkTime; // 下次火花時間
    private Animator animator; // 動畫控制器

    // 初始化
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
    }

    void DetectCollisionAndSpawnSpark()
    {
        if (weaponCollider == null || sparkPrefab == null) return;

        Vector3 weaponCenter = weaponCollider.transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(weaponCenter, weaponRange, environmentLayer);

        if (hitColliders.Length > 0)
        {
            Collider closestCollider = null;
            Vector3 closestPoint = Vector3.zero;
            float closestDistance = float.MaxValue;

            foreach (Collider col in hitColliders)
            {
                Vector3 point = col.ClosestPoint(weaponCenter);
                float distance = Vector3.Distance(weaponCenter, point);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                    closestCollider = col;
                }
            }

            if (Physics.Raycast(weaponCenter, (closestPoint - weaponCenter).normalized, out RaycastHit hitInfo, weaponRange, environmentLayer))
            {
                SpawnSpark(hitInfo.point, hitInfo.normal);
            }
            else
            {
                Vector3 fallbackNormal = (closestPoint - weaponCenter).normalized;
                SpawnSpark(closestPoint, fallbackNormal);
            }

            nextSparkTime = Time.time + 0.1f;
        }
    }

    void SpawnSpark(Vector3 position, Vector3 normal)
    {
        GameObject spark = Instantiate(sparkPrefab, position, Quaternion.LookRotation(normal));
        Destroy(spark, 1f);
    }
}
