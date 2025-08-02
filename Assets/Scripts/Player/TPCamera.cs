using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for UI Canvas
using TMPro; // Required for TextMeshPro

public class TPCamera : MonoBehaviour
{
    public Animator playerAnimator;
    public Transform mFollowPoint;
    public Transform mFollowPointRef;
    public float mFollowDistance;
    public float mMinFollowDistance;
    public float mMaxFollowDistance;
    public float executionDistanceMultiplier = 0.7f; // 處決模式距離縮減到 70%
    public float executionRightOffset = 0.5f; // 處決模式右偏移 0.5 單位
    public float executionHeightOffset = 1.0f; // 處決模式高度偏移

    private float mVerticalDegree;
    public float mVerticalLimitUp;
    public float mVerticalLimitDown;
    private Vector3 mHorizontalVector;
    public float mMouseRotateSensitivity = 1.0f;
    public float followSpeed = 10.0f;
    private Vector3 mCurrentVel = Vector3.zero;
    public LayerMask mCheckLayer;
    private bool wasLock;
    private float lockTransitionTime = 0.0f;
    public float lockTransitionDuration = 0.3f;

    [Header("鎖定設定")]
    public bool isLock;
    public Transform lockTarget;
    public float lockCameraHeight = 1.5f;
    public float lockExtraDistanceMultiplier = 0.5f;
    public float lockTiltAngle = -15f;
    public GameObject lockOnIconPrefab;
    private GameObject currentLockOnIcon;
    private Transform lastLockTarget = null;
    public Canvas uiCanvas;
    public float lockOffDistance = 100.0f;
    public float lockRange = 10.0f;
    public LayerMask enemyLayer;
    private AnimatorStateInfo stateInfo;
    private Animator _animator;
    private EnemyTest enemyTest;
    private Vector3 smoothedLockTargetPos;
    private float smoothedCollisionDistance;
    private const float maxDeltaTime = 0.02f; // 對應 50 FPS
    private Vector3 lastCameraDir;

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
        lastCameraDir = mHorizontalVector;
        _animator = GetComponent<Animator>();
        smoothedLockTargetPos = mFollowPointRef.position;
        smoothedCollisionDistance = mFollowDistance;
    }

    void FindLockTarget()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, lockRange, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;
        foreach (Collider col in targets)
        {
            Animator animator = col.GetComponent<Animator>();
            if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Death"))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = col.transform;
                }
            }
        }
        if (closestTarget != null)
        {
            lockTarget = closestTarget;
            isLock = true;
            smoothedLockTargetPos = lockTarget.position;
        }
        else
        {
            lockTarget = null;
            isLock = false;
        }
    }

    public void UpdateCameraTransform()
    {
        float deltaTime = Mathf.Min(Time.deltaTime, maxDeltaTime);
        stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        if (!isLock && !stateInfo.IsTag("Execution"))
        {
            // 一般模式
            float fMX = Input.GetAxis("Mouse X");
            float fMY = Input.GetAxis("Mouse Y");
            // 修正水平旋轉方向
            mHorizontalVector = Quaternion.AngleAxis(fMX * mMouseRotateSensitivity, Vector3.up) * mHorizontalVector;
            Vector3 rotationAxis = Vector3.Cross(mHorizontalVector, Vector3.up);
            // 垂直旋轉方向（已正確）
            mVerticalDegree += fMY * mMouseRotateSensitivity;
            if (mVerticalDegree < -mVerticalLimitUp)
                mVerticalDegree = -mVerticalLimitUp;
            else if (mVerticalDegree > mVerticalLimitDown)
                mVerticalDegree = mVerticalLimitDown;
            Vector3 vFinalDir = Quaternion.AngleAxis(mVerticalDegree, rotationAxis) * mHorizontalVector;
            vFinalDir.Normalize();
            mFollowPoint.position = Vector3.Lerp(mFollowPoint.position, mFollowPointRef.position, followSpeed * deltaTime);
            Vector3 vFinalPosition = mFollowPoint.position - vFinalDir * mFollowDistance;
            Vector3 vDir = mFollowPoint.position - vFinalPosition;
            vDir.Normalize();

            RaycastHit rh;
            Ray r = new Ray(mFollowPoint.position, -vDir);
            if (Physics.SphereCast(r, 0.1f, out rh, mFollowDistance, mCheckLayer))
            {
                smoothedCollisionDistance = Mathf.Lerp(smoothedCollisionDistance, rh.distance - 0.1f, deltaTime * followSpeed * 2f);
                vFinalPosition = mFollowPoint.position - vDir * smoothedCollisionDistance;
            }
            else
            {
                smoothedCollisionDistance = Mathf.Lerp(smoothedCollisionDistance, mFollowDistance, deltaTime * followSpeed * 2f);
                vFinalPosition = mFollowPoint.position - vDir * smoothedCollisionDistance;
            }

            transform.position = Vector3.Lerp(transform.position, vFinalPosition, deltaTime * followSpeed);
            transform.forward = (mFollowPoint.position - transform.position).normalized;
            lastCameraDir = vFinalDir;
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

    public void UpdateLockOnIcon()
    {
        if (isLock && lockTarget != null)
        {
            if (lockTarget != lastLockTarget)
            {
                if (currentLockOnIcon != null)
                    Destroy(currentLockOnIcon);
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
                Transform iconTarget = lockTarget.Find("SpawnPoint");
                if (iconTarget == null)
                {
                    iconTarget = lockTarget;
                }
                follow.target = iconTarget;
                currentLockOnIcon = icon;
                lastLockTarget = lockTarget;
            }
        }
        else
        {
            if (currentLockOnIcon != null)
            {
                Destroy(currentLockOnIcon);
                currentLockOnIcon = null;
                lastLockTarget = null;
            }
        }
    }

    void LateUpdate()
    {
        float deltaTime = Mathf.Min(Time.deltaTime, maxDeltaTime);
        stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            if (!isLock)
            {
                FindLockTarget();
                if (_animator != null)
                {
                    _animator.SetBool("Lock", isLock);
                }
            }
            else
            {
                isLock = false;
                lockTarget = null;
                if (_animator != null)
                {
                    _animator.SetBool("Lock", isLock);
                }
            }
        }

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
        if (enemyTest != null && enemyTest.isDead)
        {
            isLock = false;
            lockTarget = null;
            enemyTest = null;
            if (_animator != null)
            {
                _animator.SetBool("Lock", isLock);
            }
        }

        if (wasLock != isLock)
        {
            lockTransitionTime = 0.0f;
            if (!isLock)
            {
                Vector3 camDir = (transform.position - mFollowPoint.position).normalized;
                mHorizontalVector = camDir;
                mHorizontalVector.y = 0.0f;
                mHorizontalVector.Normalize();
                Vector3 rotationAxis = Vector3.Cross(mHorizontalVector, Vector3.up);
                float angle = Vector3.SignedAngle(mHorizontalVector, camDir, rotationAxis);
                mVerticalDegree = angle;
            }
        }

        if (isLock && lockTarget != null)
        {
            lockTransitionTime += deltaTime;
        }

        UpdateCameraTransform();
        wasLock = isLock;
        UpdateLockOnIcon();
    }
}