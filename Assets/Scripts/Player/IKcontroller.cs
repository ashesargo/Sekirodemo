using UnityEngine;

public class IKController : MonoBehaviour
{
    private bool enableIK = true; // 是否啟用 IK
    private Animator animator;
    private Vector3 leftFootIK, rightFootIK; // 射線檢測需要的 IK 位置
    private Vector3 leftFootPosition, rightFootPosition; // 腳 IK 的位置
    private Quaternion leftFootRotation, rightFootRotation; // 腳 IK 的旋轉
    private float leftFootWeight, rightFootWeight; // 儲存計算出的 IK 權重

    [Header("階梯對齊")]
    [SerializeField] private float stepHeightUnit = 0.2f; // 每階高度

    [Header("射線檢測")]
    [SerializeField] private LayerMask iKLayer; // 射線檢測需要的層
    [SerializeField] private float rayCastDistance; // 射線檢測距離
    [SerializeField] [Range(0, 1f)] private float rayHitOffset; // 射線檢測位置與 IK 偏移量
    [SerializeField] private float forwardRayOffset = 0.1f; // 射線起點往前偏移距離

    [Header("IK 權重")]
    [SerializeField] [Range(0.1f, 1f)] private float ikFalloffDistance = 0.5f; // IK 權重開始衰減的距離

    [Header("Gizmos")]
    [SerializeField] private float iKSphereRadius = 0.2f; // 射線檢測球體半徑
    [SerializeField] private float PositionSphereRadius = 0.2f; //射線檢測距離


    void Awake()
    {
        animator = this.gameObject.GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        // 獲取 IK 位置
        leftFootIK = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        rightFootIK = animator.GetIKPosition(AvatarIKGoal.RightFoot);

        if (!enableIK) return;

        // 設置 IK 權重
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootWeight);

        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootWeight);

        // 設置 IK 位置及旋轉
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPosition);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);

        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPosition);
        animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRotation);
    }

    void FixedUpdate()
    {
        Vector3 forwardOffset = transform.forward * forwardRayOffset;

        Vector3 leftRayOrigin = leftFootIK + Vector3.up * 0.5f + forwardOffset;
        Vector3 rightRayOrigin = rightFootIK + Vector3.up * 0.5f - forwardOffset;

        Debug.DrawLine(leftRayOrigin, leftRayOrigin + Vector3.down * rayCastDistance, Color.blue, Time.deltaTime);
        Debug.DrawLine(rightRayOrigin, rightRayOrigin + Vector3.down * rayCastDistance, Color.blue, Time.deltaTime);

        if (Physics.Raycast(leftRayOrigin, Vector3.down, out RaycastHit hit, rayCastDistance, iKLayer))
        {
            Debug.DrawRay(hit.point, hit.normal, Color.red, Time.deltaTime);
            // 對齊階梯高度
            Vector3 hitPos = hit.point + Vector3.up * rayHitOffset;
            hitPos.y = Mathf.Round(hitPos.y / stepHeightUnit) * stepHeightUnit;
            leftFootPosition = Vector3.Lerp(leftFootPosition, hitPos, Time.deltaTime * 100f);
            leftFootRotation = Quaternion.Slerp(leftFootRotation, Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation, Time.deltaTime * 100f);


            // 根據距離計算權重
            float distance = hit.distance - 0.5f; // 減去射線起點的向上偏移 (0.5f)
            leftFootWeight = 1f - Mathf.Clamp01(distance / ikFalloffDistance);
        }
        else
        {
            // 如果射線沒打到，權重為 0
            leftFootWeight = 0f;
        }

        if (Physics.Raycast(rightRayOrigin, Vector3.down, out RaycastHit hit_01, rayCastDistance, iKLayer))
        {
            Debug.DrawRay(hit_01.point, hit_01.normal, Color.red, Time.deltaTime);
            // 對齊階梯高度
            Vector3 hitPos = hit_01.point + Vector3.up * rayHitOffset;
            hitPos.y = Mathf.Round(hitPos.y / stepHeightUnit) * stepHeightUnit;
            rightFootPosition = Vector3.Lerp(rightFootPosition, hitPos, Time.deltaTime * 100f);
            rightFootRotation = Quaternion.Slerp(rightFootRotation, Quaternion.FromToRotation(Vector3.up, hit_01.normal) * transform.rotation, Time.deltaTime * 100f);


            // 根據距離計算權重
            float distance = hit_01.distance - 0.5f; // 減去射線起點的向上偏移 (0.5f)
            rightFootWeight = 1f - Mathf.Clamp01(distance / ikFalloffDistance);
        }
        else
        {
            // 如果射線沒打到，權重為 0
            rightFootWeight = 0f;
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(leftFootIK, iKSphereRadius);
        Gizmos.DrawWireSphere(rightFootIK, iKSphereRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(leftFootPosition, PositionSphereRadius);
        Gizmos.DrawWireSphere(rightFootPosition, PositionSphereRadius);
    }


 
}