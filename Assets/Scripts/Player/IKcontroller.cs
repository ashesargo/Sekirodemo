using UnityEngine;

public class IKController : MonoBehaviour
{
    private Animator animator;
    private Vector3 leftFootIK, rightFootIK; // 射線檢測需要的 IK 位置
    private Vector3 leftFootPosition, rightFootPosition; // 腳 IK 的位置
    private Quaternion leftFootRotation, rightFootRotation; // 腳 IK 的旋轉

    [SerializeField] private LayerMask iKLayer; // 射線檢測需要的層
    [SerializeField] [Range(0, 0.2f)] private float rayHitOffset; // 射線檢測位置與 IK 偏移量
    [SerializeField] private float rayCastDistance; // 射線檢測距離

    [SerializeField] private bool enableIK = true; // 是否啟用 IK
    [SerializeField] private float iKSphereRadius = 100; // 射線檢測球體半徑
    [SerializeField] private float PositionSphereRadius; //射線檢測距離

    void Awake()
    {
        animator = this.gameObject.GetComponent<Animator>();
        // 獲取 IK 位置
        // leftFootIK = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        // rightFootIK = animator.GetIKPosition(AvatarIKGoal.RightFoot);
    }

    void OnAnimatorIK(int layerIndex)
    {
        leftFootIK = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        rightFootIK = animator.GetIKPosition(AvatarIKGoal.RightFoot);

        if (!enableIK || !IsGroundedAndIdle()) return;

        //設置 IK 權重
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);

        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);

        // 設置 IK 位置及旋轉
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPosition);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);

        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPosition);
        animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRotation);
    }

    void FixedUpdate()
    {
        Debug.DrawLine(leftFootIK + (Vector3.up * 0.5f), leftFootIK + Vector3.down * rayCastDistance, Color.blue, Time.deltaTime);
        Debug.DrawLine(rightFootIK + (Vector3.up * 0.5f), rightFootIK + Vector3.down * rayCastDistance, Color.blue, Time.deltaTime);

        // 獲取旋轉值及位置
        if (Physics.Raycast(leftFootIK + (Vector3.up * 0.5f), Vector3.down, out RaycastHit hit, rayCastDistance, iKLayer))
        {
            Debug.DrawRay(hit.point, hit.normal, Color.red, Time.deltaTime);
            // 避免腳位置等於碰撞點造成穿模穿模
            leftFootPosition = hit.point + Vector3.up * rayHitOffset;
            leftFootRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;
        }

        if (Physics.Raycast(rightFootIK + (Vector3.up * 0.5f), Vector3.down, out RaycastHit hit_01, rayCastDistance, iKLayer))
        {
            Debug.DrawRay(hit_01.point, hit_01.normal, Color.red, Time.deltaTime);
            // 避免腳位置等於碰撞點造成穿模穿模
            rightFootPosition = hit_01.point + Vector3.up * rayHitOffset;
            rightFootRotation = Quaternion.FromToRotation(Vector3.up, hit_01.normal) * transform.rotation;
        }
    }

    bool IsGroundedAndIdle()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle");
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