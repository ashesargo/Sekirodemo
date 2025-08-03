using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// TPCamera：第三人稱攝影機控制腳本，支援一般跟隨與鎖定目標模式
public class TPCamera : MonoBehaviour
{
    public Animator playerAnimator;
    // 跟隨點（攝影機會跟隨此點）
    public Transform mFollowPoint;
    // 跟隨點的參考（通常為玩家）
    public Transform mFollowPointRef;
    // 攝影機與跟隨點的距離
    public float mFollowDistance;
    public float mMinFollowDistance;
    public float mMaxFollowDistance;

    // 垂直旋轉角度
    private float mVerticalDegree;
    // 垂直旋轉上限
    public float mVerticalLimitUp;
    // 垂直旋轉下限
    public float mVerticalLimitDown;

    // 水平旋轉向量
    private Vector3 mHorizontalVector;
    // 滑鼠旋轉靈敏度
    public float mMouseRotateSensitivity = 1.0f;
    // 攝影機跟隨速度
    public float followSpeed = 10.0f;
    // 攝影機平滑移動用的速度暫存
    private Vector3 mCurrentVel = Vector3.zero;
    // 檢查碰撞的圖層
    public LayerMask mCheckLayer;
    // 前一幀是否為鎖定狀態
    private bool wasLock;
    // 鎖定過渡時間
    private float lockTransitionTime = 0.0f;
    // 鎖定過渡持續時間
    public float lockTransitionDuration = 0.3f;
    [Header("鎖定設定")]
    // 是否鎖定目標
    public bool isLock;
    // 鎖定的目標
    public Transform lockTarget;
    // 鎖定時攝影機高度
    public float lockCameraHeight = 1.5f;
    // 鎖定時額外的後退距離倍數
    public float lockExtraDistanceMultiplier = 0.5f;
    // 鎖定時的俯視角度（度）
    public float lockTiltAngle = -15f;
    // 鎖定圖示的 Prefab
    public GameObject lockOnIconPrefab; // 指到 LockOnIcon.prefab
    // 當前顯示的鎖定圖示
    private GameObject currentLockOnIcon;
    // 上一次鎖定的目標
    private Transform lastLockTarget = null;
    // UI Canvas（需在 Inspector 指定）
    public Canvas uiCanvas; // 在 Inspector 拖入你的 Canvas
    // 超過此距離自動解除鎖定
    public float lockOffDistance = 100.0f;
    AnimatorStateInfo stateInfo;
    public float lockRange = 10.0f;
    public LayerMask enemyLayer;
    EnemyTest enemyTest;
    // 新增：平滑處理 lockTarget 的位置
    private Vector3 smoothedLockTargetPos;
    // 新增：平滑處理碰撞檢測的距離
    private float smoothedCollisionDistance;
    // 新增：上一次碰撞距離的初始值
    private float lastDistance = 0f;
    // 新增：限制最大 Time.deltaTime
    private const float maxDeltaTime = 0.02f; // 對應 50 FPS

    private Vector3 lastCameraDir;
    public float executionDistanceMultiplier = 0.7f; // 處決模式距離縮減到 70%
    public float executionRightOffset = 0.5f; // 處決模式右偏移 0.5 單位
    public float executionHeightOffset = 1.0f; // 處決模式高度偏移
    // 初始化攝影機位置與方向
    void Awake()
    {
        stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        mFollowPoint.position = mFollowPointRef.position;
        mFollowPoint.rotation = mFollowPointRef.rotation;
        transform.position = mFollowPoint.position - mFollowDistance * mFollowPoint.forward;
        Vector3 vDir = transform.position - mFollowPoint.position;
        mHorizontalVector = vDir;
        mHorizontalVector.y = 0.0f;
        mHorizontalVector.Normalize();
        // 初始化平滑位置
        smoothedLockTargetPos = mFollowPointRef.position; // 初始值設為玩家位置
        smoothedCollisionDistance = mFollowDistance; // 初始碰撞距離
    }

    void FindLockTarget()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, lockRange, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;
        foreach (Collider col in targets)
        {
            // 獲取敵人的 Animator 組件
            Animator animator = col.GetComponent<Animator>();
            if (animator != null)
            {
                // 檢查敵人是否處於 "Death" 動畫狀態
                if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Death"))
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = col.transform;
                    }
                }
            }
        }
        // 更新鎖定目標
        if (closestTarget != null)
        {
            lockTarget = closestTarget;
            isLock = true;
            smoothedLockTargetPos = lockTarget.position; // 重置平滑位置
        }
        else
        {
            // 如果沒有可鎖定的目標，解除鎖定
            lockTarget = null;
            isLock = false;
        }
    }

    // 更新攝影機位置與旋轉
    public void UpdateCameraTransform()
    {
        // 限制 Time.deltaTime 以避免幀率不穩定
        //float deltaTime = Mathf.Min(Time.deltaTime, maxDeltaTime);
        float deltaTime = Time.deltaTime;

        if (!isLock && !stateInfo.IsTag("Execution"))
        {
            // 一般模式：根據滑鼠移動旋轉攝影機
            float fMX = Input.GetAxis("Mouse X");
            float fMY = Input.GetAxis("Mouse Y");
            mHorizontalVector = Quaternion.AngleAxis(fMX * mMouseRotateSensitivity, Vector3.up) * mHorizontalVector;
            Vector3 rotationAxis = Vector3.Cross(mHorizontalVector, Vector3.up);
            mVerticalDegree -= fMY * mMouseRotateSensitivity;
            // 限制垂直旋轉角度
            if (mVerticalDegree < -mVerticalLimitUp)
            {
                mVerticalDegree = -mVerticalLimitUp;
            }
            else if (mVerticalDegree > mVerticalLimitDown)
            {
                mVerticalDegree = mVerticalLimitDown;
            }
            Vector3 vFinalDir = Quaternion.AngleAxis(mVerticalDegree, rotationAxis) * mHorizontalVector;
            vFinalDir.Normalize();
            // 跟隨點平滑移動
            mFollowPoint.position = Vector3.Lerp(mFollowPoint.position, mFollowPointRef.position, followSpeed * deltaTime);
            Vector3 vFinalPosition = mFollowPoint.position + vFinalDir * mFollowDistance;
            Vector3 vDir = mFollowPoint.position - vFinalPosition;
            vDir.Normalize();
            // 檢查攝影機與角色間是否有障礙物
            RaycastHit rh;
            Ray r = new Ray(mFollowPoint.position, -vDir);

            if (Physics.SphereCast(r, 0.1f, out rh, mFollowDistance, mCheckLayer))
            {
                smoothedCollisionDistance = Mathf.Lerp(smoothedCollisionDistance, rh.distance - 0.1f, deltaTime * followSpeed);
                vFinalPosition = mFollowPoint.position - vDir * smoothedCollisionDistance;
            }
            else
            {
                smoothedCollisionDistance = Mathf.Lerp(smoothedCollisionDistance, mFollowDistance, deltaTime * followSpeed);
                vFinalPosition = mFollowPoint.position - vDir * smoothedCollisionDistance;
            }
            // 攝影機平滑移動到目標位置
            transform.position = Vector3.Lerp(transform.position, vFinalPosition, 1.0f);
            vDir = mFollowPoint.position - transform.position;
            // 攝影機朝向角色
            transform.forward = vDir;
        }
        else if (stateInfo.IsTag("Execution"))
        {
            // 處決模式：拉近並向右偏移
            mFollowPoint.position = Vector3.Lerp(mFollowPoint.position, mFollowPointRef.position, followSpeed * deltaTime);
            float executionDistance = mFollowDistance * executionDistanceMultiplier;
            Vector3 rightOffset = mFollowPointRef.right * executionRightOffset;
            Vector3 heightOffset = Vector3.up * executionHeightOffset;
            Vector3 vFinalDir = lastCameraDir;
            Vector3 vFinalPosition = mFollowPoint.position + heightOffset - vFinalDir * executionDistance + rightOffset;
            Vector3 vDir = (mFollowPoint.position - vFinalPosition).normalized;

            RaycastHit rh;
            Ray r = new Ray(mFollowPoint.position, -vDir);
            if (Physics.SphereCast(r, 0.1f, out rh, executionDistance, mCheckLayer))
            {
                smoothedCollisionDistance = Mathf.Lerp(smoothedCollisionDistance, rh.distance - 0.1f, deltaTime * followSpeed * 2f);
                vFinalPosition = mFollowPoint.position - vDir * smoothedCollisionDistance + rightOffset + heightOffset;
            }
            else
            {
                smoothedCollisionDistance = Mathf.Lerp(smoothedCollisionDistance, executionDistance, deltaTime * followSpeed * 2f);
                vFinalPosition = mFollowPoint.position - vDir * smoothedCollisionDistance + rightOffset + heightOffset;
            }

            transform.position = Vector3.Lerp(transform.position, vFinalPosition, deltaTime * followSpeed);
            transform.forward = (mFollowPoint.position - transform.position).normalized;
            lastCameraDir = (mFollowPoint.position - transform.position).normalized;
        }
        else
        {
            // 鎖定模式
            mFollowPoint.position = Vector3.Lerp(mFollowPoint.position, mFollowPointRef.position, followSpeed * deltaTime);
            if (lockTarget != null)
            {
                smoothedLockTargetPos = Vector3.Lerp(smoothedLockTargetPos, lockTarget.position, deltaTime * followSpeed);
            }
            Vector3 centerBetween = mFollowPoint.position * (2.0f / 3.0f) + smoothedLockTargetPos * (1.0f / 3.0f);
            centerBetween.y += lockCameraHeight;
            Vector3 lockDirection = centerBetween - mFollowPoint.position;
            lockDirection.y = 0;
            lockDirection.Normalize();
            Vector3 offset = Vector3.up * lockCameraHeight;
            float extraDistance = mFollowDistance * lockExtraDistanceMultiplier;
            Vector3 vFinalPosition = mFollowPoint.position + offset - lockDirection * (mFollowDistance + extraDistance);
            Vector3 vDir = mFollowPoint.position - vFinalPosition;
            vDir.Normalize();

            RaycastHit rh;
            Ray r = new Ray(mFollowPoint.position, -vDir);
            if (Physics.SphereCast(r, 0.1f, out rh, mFollowDistance + extraDistance, mCheckLayer))
            {
                smoothedCollisionDistance = Mathf.Lerp(smoothedCollisionDistance, rh.distance - 0.1f, deltaTime * followSpeed * 2f);
                vFinalPosition = mFollowPoint.position - vDir * smoothedCollisionDistance;
            }
            else
            {
                smoothedCollisionDistance = Mathf.Lerp(smoothedCollisionDistance, mFollowDistance + extraDistance, deltaTime * followSpeed * 2f);
                vFinalPosition = mFollowPoint.position - vDir * smoothedCollisionDistance;
            }

            float transitionProgress = Mathf.Clamp01(lockTransitionTime / lockTransitionDuration);
            float smoothFactor = Mathf.SmoothStep(0, 1, transitionProgress);
            transform.position = Vector3.Lerp(transform.position, vFinalPosition, deltaTime * followSpeed * (1 + smoothFactor));
            Vector3 targetForward = (centerBetween - transform.position).normalized;
            Vector3 horizontalForward = targetForward;
            horizontalForward.y = 0;
            horizontalForward.Normalize();
            Vector3 tiltedForward = Quaternion.AngleAxis(lockTiltAngle, Vector3.Cross(horizontalForward, Vector3.up)) * horizontalForward;
            transform.forward = Vector3.Slerp(transform.forward, tiltedForward, deltaTime * followSpeed * (1 + smoothFactor));
            lastCameraDir = (centerBetween - transform.position).normalized;
        }
    }

    // 更新鎖定圖示（LockOn Icon）
    public void UpdateLockOnIcon()
    {
        if (isLock && lockTarget != null)
        {
            if (lockTarget != lastLockTarget)
            {
                // 移除舊的 ICON
                if (currentLockOnIcon != null)
                    Destroy(currentLockOnIcon);

                // 生成新的 ICON
                if (lockOnIconPrefab == null)
                {
                    Debug.LogError("lockOnIconPrefab 尚未指定！");
                    return;
                }
                if (uiCanvas == null)
                {
                    Debug.LogError("uiCanvas 尚未指定！");
                    return;
                }
                GameObject icon = Instantiate(lockOnIconPrefab);
                icon.transform.SetParent(uiCanvas.transform, false);
                var follow = icon.GetComponent<LockOnIconFollow>();
                if (follow == null)
                {
                    Debug.LogError("LockOnIconFollow 腳本沒掛在 Prefab 上！");
                    return;
                }
                // 嘗試取得敵人身上的 SpawnPoint 作為鎖定點
                Transform iconTarget = lockTarget.Find("SpawnPoint");
                if (iconTarget == null)
                {
                    iconTarget = lockTarget;
                }
                // 設定跟隨目標
                follow.target = iconTarget;
                currentLockOnIcon = icon;
                lastLockTarget = lockTarget;
            }
        }
        else
        {
            // 非鎖定狀態或無目標時，移除 ICON
            if (currentLockOnIcon != null)
            {
                Destroy(currentLockOnIcon);
                currentLockOnIcon = null;
                lastLockTarget = null;
            }
        }
    }

    /// <summary>
    /// 通知鎖定圖示觸發架勢滿效果
    /// </summary>
    public void NotifyPostureFullEffect()
    {
        if (isLock && currentLockOnIcon != null)
        {
            LockOnIconFollow follow = currentLockOnIcon.GetComponent<LockOnIconFollow>();
            if (follow != null)
            {
                follow.TriggerPostureFullEffect();
            }
        }
    }

    // 每幀更新（建議用 LateUpdate 以確保角色移動後再更新攝影機）
    void LateUpdate()
    {

        // 限制 Time.deltaTime 以避免幀率不穩定
        float deltaTime = Mathf.Min(Time.deltaTime, maxDeltaTime);

        stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        playerAnimator.SetBool("Lock", isLock);
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            if (!isLock)
            {
                FindLockTarget();
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("Lock", isLock);
                }
            }
            else
            {
                isLock = false;
                lockTarget = null;
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("Lock", isLock);
                }
            }
        }
        // 若鎖定中且有目標，檢查距離是否超過閾值
        if (isLock && lockTarget != null)
        {
            enemyTest = lockTarget.GetComponent<EnemyTest>();
            float dist = Vector3.Distance(mFollowPointRef.position, lockTarget.position);
            if (dist > lockOffDistance)
            {
                isLock = false;
                lockTarget = null;
            }
        }
        if (enemyTest != null)
        {
            if (enemyTest.isDead)
            {
                isLock = false;
                lockTarget = null;
                enemyTest = null;
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("Lock", isLock);
                }
            }
        }
        // 鎖定狀態切換時重設攝影機
        if (wasLock != isLock)
        {
            // 重置過渡時間
            lockTransitionTime = 0.0f;

            // 只在進入鎖定時重設，解除鎖定時不重設攝影機位置，保留原本指向
            if (isLock)
            {
                // 進入鎖定模式時，保持當前攝影機位置，讓過渡更平滑
                // 不立即重設位置，讓過渡系統處理
            }
            else // 解除鎖定時，將當前攝影機 forward 轉換回 mHorizontalVector 與 mVerticalDegree
            {
                // 計算新的 mHorizontalVector（投影到 XZ 平面並正規化）
                Vector3 camDir = (transform.position - mFollowPoint.position).normalized;
                mHorizontalVector = camDir;
                mHorizontalVector.y = 0.0f;
                mHorizontalVector.Normalize();
                // 計算 mVerticalDegree
                Vector3 rotationAxis = Vector3.Cross(mHorizontalVector, Vector3.up);
                float angle = Vector3.SignedAngle(mHorizontalVector, camDir, rotationAxis);
                mVerticalDegree = angle;
            }
        }

        // 更新過渡時間
        if (isLock && lockTarget != null)
        {
            lockTransitionTime += deltaTime;
        }
        // 更新攝影機位置與旋轉
        UpdateCameraTransform();
        // 記錄本幀鎖定狀態
        wasLock = isLock;
        // 更新鎖定圖示
        UpdateLockOnIcon();
    }
}