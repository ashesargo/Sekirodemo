using UnityEngine;

// 腳部 IK 控制器 - 實現角色腳部與地面的自適應對齊
public class IKController : MonoBehaviour
{
    [Header("階梯對齊設定")]
    [SerializeField] private float stepHeightUnit = 0.2f; // 每階高度單位

    [Header("射線檢測設定")]
    [SerializeField] private LayerMask iKLayer; // 射線檢測層級
    [SerializeField] private float rayCastDistance; // 射線檢測距離
    [SerializeField] [Range(0, 1f)] private float rayHitOffset; // 射線檢測位置與 IK 偏移量
    [SerializeField] private float forwardRayOffset = 0.1f; // 射線起點往前偏移距離

    [Header("IK 權重設定")]
    [SerializeField] [Range(0.1f, 1f)] private float ikFalloffDistance = 0.5f; // IK 權重開始衰減的距離

    [Header("Gizmos 顯示設定")]
    [SerializeField] private float iKSphereRadius = 0.2f; // IK 位置球體半徑
    [SerializeField] private float PositionSphereRadius = 0.2f; // 實際位置球體半徑

    private bool enableIK = true; // 是否啟用 IK
    private Animator animator; // 動畫控制器引用
    
    // IK 位置和旋轉
    private Vector3 leftFootIK, rightFootIK; // 射線檢測需要的 IK 位置
    private Vector3 leftFootPosition, rightFootPosition; // 腳 IK 的實際位置
    private Quaternion leftFootRotation, rightFootRotation; // 腳 IK 的實際旋轉
    private float leftFootWeight, rightFootWeight; // IK 權重值

    void Awake()
    {
        InitializeComponents();
    }

    void FixedUpdate()
    {
        if (!enableIK) return;
        
        ProcessFootIK(AvatarIKGoal.LeftFoot, ref leftFootIK, ref leftFootPosition, ref leftFootRotation, ref leftFootWeight);
        ProcessFootIK(AvatarIKGoal.RightFoot, ref rightFootIK, ref rightFootPosition, ref rightFootRotation, ref rightFootWeight);
    }

    // 在 Animator 中呼叫
    void OnAnimatorIK(int layerIndex)
    {
        if (!enableIK) return;
        
        UpdateIKPositions();
        ApplyIKWeights();
    }

    void OnDrawGizmos()
    {
        DrawIKGizmos();
    }

    // 初始化方法
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
    }

    // 更新 IK 位置
    private void UpdateIKPositions()
    {
        // 獲取 IK 位置
        leftFootIK = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
        rightFootIK = animator.GetIKPosition(AvatarIKGoal.RightFoot);
    }

    // 設定 IK 權重
    private void ApplyIKWeights()
    {
        // 設置左腳 IK 權重
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPosition);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);

        // 設置右腳 IK 權重
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootWeight);
        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPosition);
        animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRotation);
    }

    // 腳部 IK 處理
    private void ProcessFootIK(AvatarIKGoal footGoal, ref Vector3 footIK, ref Vector3 footPosition, ref Quaternion footRotation, ref float footWeight)
    {
        // 計算射線起點
        Vector3 forwardOffset = transform.forward * forwardRayOffset;
        Vector3 rayOrigin = footIK + Vector3.up * 0.5f + (footGoal == AvatarIKGoal.LeftFoot ? forwardOffset : -forwardOffset);

        // 繪製調試射線
        Debug.DrawLine(rayOrigin, rayOrigin + Vector3.down * rayCastDistance, Color.blue, Time.deltaTime);

        // 執行射線檢測
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayCastDistance, iKLayer))
        {
            ProcessSuccessfulRaycast(hit, ref footPosition, ref footRotation, ref footWeight);
        }
        else
        {
            // 射線未擊中，權重設為 0
            footWeight = 0f;
        }
    }

    private void ProcessSuccessfulRaycast(RaycastHit hit, ref Vector3 footPosition, ref Quaternion footRotation, ref float footWeight)
    {
        // 繪製擊中點的法向量
        Debug.DrawRay(hit.point, hit.normal, Color.red, Time.deltaTime);

        // 計算對齊階梯高度的位置
        Vector3 hitPos = hit.point + Vector3.up * rayHitOffset;
        hitPos.y = Mathf.Round(hitPos.y / stepHeightUnit) * stepHeightUnit;

        // 平滑插值到目標位置和旋轉
        footPosition = Vector3.Lerp(footPosition, hitPos, Time.deltaTime * 100f);
        footRotation = Quaternion.Slerp(footRotation, Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation, Time.deltaTime * 100f);

        // 根據距離計算 IK 權重
        float distance = hit.distance - 0.5f; // 減去射線起點的向上偏移
        footWeight = 1f - Mathf.Clamp01(distance / ikFalloffDistance);
    }

    // Gizmos 繪製方法
    private void DrawIKGizmos()
    {
        // 繪製 IK 位置球體
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(leftFootIK, iKSphereRadius);
        Gizmos.DrawWireSphere(rightFootIK, iKSphereRadius);

        // 繪製實際位置球體
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(leftFootPosition, PositionSphereRadius);
        Gizmos.DrawWireSphere(rightFootPosition, PositionSphereRadius);
    }

    // 設定 IK 啟用狀態
    public void SetIKEnabled(bool enabled)
    {
        enableIK = enabled;
    }

    // 檢查 IK 啟用狀態
    public bool IsIKEnabled()
    {
        return enableIK;
    }
}