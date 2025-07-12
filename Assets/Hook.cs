using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrapple : MonoBehaviour
{
    public float grappleRange = 10f; // 勾鎖檢測範圍
    public float maxGrappleDistance = 15f; // 最大勾鎖距離
    public float grappleSpeed = 10f; // 勾鎖移動速度
    public float sphereRadius = 0.5f; // SphereCast的球體半徑
    public float ropeWidth = 0.03f; // 繩索寬度
    public float ropeExtendDuration = 0.1f; // 繩索射出時長
    public float grapple2Duration = 0.2f; // Grapple2 動畫時長
    public float pullTautDuration = 0.5f; // 繩索拉緊過渡時間
    public float turnDuration = 0.2f; // 轉向目標點的時長
    public float jumpAnimationDuration = 0.5f; // JumpToTarget 動畫時長
    public int ropeSegments = 20; // 繩索細分節點數
    public float sagAmount = 1.0f; // 繩索下垂幅度
    public LayerMask grappleLayer; // 勾鎖點的層
    public Camera mainCamera; // 主攝影機
    public Animator animator; // 玩家動畫控制器
    public CharacterController controller; // 角色控制器
    public LineRenderer grappleRope; // 繩索的LineRenderer
    public Transform ropeStartPoint; // 繩索起點（應為 RightHand 的子物件）

    private List<GrapplePoint> nearbyPoints = new List<GrapplePoint>();
    private bool isGrappling = false;
    private bool isExtendingRope = false; // 控制繩索延伸狀態
    private Vector3 currentTargetPos; // 當前目標點位置
    private Coroutine extendRopeCoroutine; // 儲存 ExtendRope 協程

    void Start()
    {
        if (grappleRope != null)
        {
            grappleRope.enabled = false;
            grappleRope.startWidth = ropeWidth;
            grappleRope.endWidth = ropeWidth * 0.8f;
            grappleRope.positionCount = 0; // 初始無節點
        }
        else
        {
            Debug.LogError("GrappleRope 未分配，請在 Inspector 中分配 LineRenderer");
        }

        if (ropeStartPoint == null)
        {
            Debug.LogError("RopeStartPoint 未分配，請在 Inspector 中分配並設為 RightHand 的子物件");
            ropeStartPoint = transform;
        }
        else if (ropeStartPoint.parent == null)
        {
            Debug.LogError("ropeStartPoint 的父物件未設置，請將其設為 RightHand");
        }
        else if (ropeStartPoint.localScale != Vector3.one)
        {
            Debug.LogWarning($"ropeStartPoint Scale ({ropeStartPoint.localScale}) 不是 (1,1,1)，可能導致位置錯誤");
        }

        if (animator == null)
        {
            Debug.LogError("Animator 未分配，請在 Inspector 中分配");
        }

        if (mainCamera == null)
        {
            Debug.LogError("MainCamera 未分配，請在 Inspector 中分配");
        }

        if (controller == null)
        {
            Debug.LogError("CharacterController 未分配，請在 Inspector 中分配");
        }
    }

    void Update()
    {
        UpdateNearbyPoints();

        if (Input.GetKeyDown(KeyCode.E) && !isGrappling)
        {
            TryGrapple();
        }

        // 調試 Animator 狀態
        if (isGrappling && animator != null)
        {
            AnimatorStateInfo baseState = animator.GetCurrentAnimatorStateInfo(0); // Base Layer
            AnimatorStateInfo upperState = animator.GetCurrentAnimatorStateInfo(1); // UpperBody Layer
            Debug.Log($"Base state: {GetCurrentStateName(baseState)}, normalizedTime: {baseState.normalizedTime}, UpperBody state: {GetCurrentStateName(upperState)}, Time: {Time.time}");
        }
    }

    void LateUpdate()
    {
        // 備用繩索更新，確保 JumpToTarget 階段繩索跟隨
        if (isGrappling && grappleRope.enabled && !isExtendingRope)
        {
            AnimatorStateInfo baseState = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo upperState = animator.GetCurrentAnimatorStateInfo(1);
            if (baseState.IsName("JumpToTarget") || upperState.IsName("Grapple2"))
            {
                Vector3 ropeStartPos = ropeStartPoint.position;
                Vector3 ropeEndPos = currentTargetPos;
                UpdateRopeWithSag(ropeStartPos, ropeEndPos, sagAmount, ropeSegments);
                Debug.Log($"LateUpdate: Base state={GetCurrentStateName(baseState)}, UpperBody state={GetCurrentStateName(upperState)}, ropeStartPoint={ropeStartPoint.position}, Time={Time.time}");
            }
        }
    }

    string GetCurrentStateName(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsName("Grapple")) return "Grapple";
        if (stateInfo.IsName("Grapple2")) return "Grapple2";
        if (stateInfo.IsName("JumpToTarget")) return "JumpToTarget";
        return "Other";
    }

    void UpdateNearbyPoints()
    {
        foreach (var point in nearbyPoints)
        {
            point.Highlight(false);
        }
        nearbyPoints.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, grappleRange, grappleLayer);
        foreach (var collider in colliders)
        {
            GrapplePoint point = collider.GetComponent<GrapplePoint>();
            if (point != null && point.isGrapplable)
            {
                point.Highlight(true);
                nearbyPoints.Add(point);
            }
        }
    }

    void TryGrapple()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.SphereCast(ray, sphereRadius, out hit, maxGrappleDistance, grappleLayer))
        {
            GrapplePoint targetPoint = hit.collider.GetComponent<GrapplePoint>();
            if (targetPoint != null && targetPoint.isGrapplable)
            {
                StartCoroutine(StartGrappleWithTurn(targetPoint));
                return;
            }
        }

        GrapplePoint closestPoint = null;
        float minDistance = float.MaxValue;

        foreach (var point in nearbyPoints)
        {
            float distance = Vector3.Distance(transform.position, point.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = point;
            }
        }

        if (closestPoint != null && closestPoint.isGrapplable)
        {
            StartCoroutine(StartGrappleWithTurn(closestPoint));
        }
    }

    IEnumerator StartGrappleWithTurn(GrapplePoint target)
    {
        Vector3 targetDirection = (target.transform.position - transform.position).normalized;
        targetDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion startRotation = transform.rotation;

        float elapsedTime = 0f;
        while (elapsedTime < turnDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / turnDuration;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            Debug.Log($"Turning: t={t}, rotation={transform.rotation.eulerAngles}, Time={Time.time}");
            yield return null;
        }
        transform.rotation = targetRotation;
        Debug.Log($"Turn completed: rotation={transform.rotation.eulerAngles}, Time={Time.time}");

        StartGrapple(target);
    }

    void StartGrapple(GrapplePoint target)
    {
        isGrappling = true;
        currentTargetPos = target.transform.position;

        if (animator != null)
        {
            animator.SetTrigger("Grapple");
            Debug.Log("Grapple animation triggered at: " + Time.time);
        }
        else
        {
            Debug.LogWarning("Animator is null, cannot trigger Grapple animation");
        }

        if (controller != null)
        {
            controller.enabled = false;
        }
        else
        {
            Debug.LogWarning("CharacterController is null, cannot disable controller");
        }

        if (grappleRope != null)
        {
            grappleRope.enabled = true;
            grappleRope.positionCount = 0;
            StartCoroutine(DelayedUpdateRope(currentTargetPos));
        }
    }

    public void StartRopeExtend()
    {
        Debug.Log("StartRopeExtend called at: " + Time.time);
        if (extendRopeCoroutine != null)
        {
            StopCoroutine(extendRopeCoroutine);
        }
        isExtendingRope = true;
        extendRopeCoroutine = StartCoroutine(ExtendRope(currentTargetPos));
    }

    public void Grapple2()
    {
        Debug.Log("Grapple2 called at: " + Time.time);
        if (animator != null)
        {
            animator.SetTrigger("Grapple2");
            Debug.Log("Grapple2 triggered at: " + Time.time);
            StartCoroutine(DelayedMoveToGrapple());
        }
        else
        {
            Debug.LogWarning("Animator is null, cannot trigger Grapple2");
        }
    }

    public void StartMoveToGrapple()
    {
        Debug.Log($"StartMoveToGrapple called: Base state={GetCurrentStateName(animator.GetCurrentAnimatorStateInfo(0))}, UpperBody state={GetCurrentStateName(animator.GetCurrentAnimatorStateInfo(1))}, Time={Time.time}");
    }

    IEnumerator DelayedMoveToGrapple()
    {
        Debug.Log($"DelayedMoveToGrapple: Waiting for {grapple2Duration}s, Time={Time.time}");
        yield return new WaitForSeconds(grapple2Duration); // 等待 Grapple2 動畫結束
        animator.SetBool("IsJumping", true);
        float distance = Vector3.Distance(transform.position, currentTargetPos);
        float moveTime = distance / grappleSpeed;
        animator.speed = jumpAnimationDuration / Mathf.Max(moveTime, 0.1f); // 確保動畫播放完整
        Debug.Log($"JumpToTarget started: animator.speed={animator.speed}, moveTime={moveTime}, Time={Time.time}");
        StartCoroutine(MoveToGrapplePoint(currentTargetPos));
    }

    IEnumerator MoveToGrapplePoint(Vector3 targetPos)
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float moveTime = distance / grappleSpeed;

        while (elapsedTime < moveTime && Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveTime;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            Debug.Log($"MoveToGrapplePoint: t={t}, position={transform.position}, distance={Vector3.Distance(transform.position, targetPos)}, Time={Time.time}");
            yield return null;
        }

        transform.position = targetPos;
        Debug.Log($"Reached target: position={transform.position}, Time={Time.time}");

        // 確保動畫播放至少 jumpAnimationDuration
        float remainingTime = jumpAnimationDuration - elapsedTime;
        if (remainingTime > 0)
        {
            Debug.Log($"Waiting for remaining animation time: {remainingTime}s, Time={Time.time}");
            yield return new WaitForSeconds(remainingTime);
        }



        if (animator != null)
        {
            animator.SetBool("IsJumping", false);
            animator.ResetTrigger("Grapple");
            animator.ResetTrigger("Grapple2");
            animator.speed = 1f;
            Debug.Log("JumpToTarget ended, IsJumping reset, Time: " + Time.time);
        }

        if (controller != null)
        {
            controller.enabled = true;
            Debug.Log("CharacterController enabled, Time: " + Time.time);
        }

        isGrappling = false;
        Debug.Log("Grapple completed, isGrappling: false, Time: " + Time.time);
    }

    IEnumerator DelayedUpdateRope(Vector3 targetPos)
    {
        yield return new WaitForSeconds(ropeExtendDuration);
        isExtendingRope = false;
        yield return StartCoroutine(UpdateRope(targetPos));
    }

    IEnumerator UpdateRope(Vector3 targetPos)
    {
        float elapsedTime = 0f;
        while (isGrappling)
        {
            if (!isExtendingRope)
            {
                float t = Mathf.Clamp01(elapsedTime / pullTautDuration);
                float currentSag = Mathf.Lerp(sagAmount, 0f, t);
                UpdateRopeWithSag(ropeStartPoint.position, targetPos, currentSag, ropeSegments);
                Debug.Log($"UpdateRope: t={t}, sag={currentSag}, ropeStartPoint={ropeStartPoint.position}, Time={Time.time}");
                elapsedTime += Time.deltaTime;
            }
            yield return null;
        }
        grappleRope.enabled = false;
        grappleRope.positionCount = 0;
        Debug.Log("UpdateRope ended, rope disabled, Time: " + Time.time);
    }

    IEnumerator ExtendRope(Vector3 targetPos)
    {
        grappleRope.startWidth = ropeWidth;
        grappleRope.endWidth = ropeWidth * 0.8f;

        float elapsedTime = 0f;
        while (elapsedTime < ropeExtendDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / ropeExtendDuration;
            int currentSegments = Mathf.FloorToInt(Mathf.Lerp(2, ropeSegments, t));
            Vector3 ropeStartPos = ropeStartPoint.position;
            Vector3 ropeEndPos = Vector3.Lerp(ropeStartPoint.position, targetPos, t);
            UpdateRopeWithSag(ropeStartPos, ropeEndPos, sagAmount, currentSegments);
            Debug.Log($"ExtendRope: t={t}, segments={currentSegments}, ropeStartPoint={ropeStartPoint.position}, Time={Time.time}");
            yield return null;
        }

        isExtendingRope = false;
    }

    void UpdateRopeWithSag(Vector3 startPos, Vector3 endPos, float currentSag, int segments)
    {
        grappleRope.positionCount = segments;
        float distance = Vector3.Distance(startPos, endPos);
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 point = Vector3.Lerp(startPos, endPos, t);
            if (i > 0 && i < segments - 1)
            {
                float sag = currentSag * Mathf.Sin(t * Mathf.PI);
                point.y -= sag;
            }
            grappleRope.SetPosition(i, point);
        }

        // 可視化繩索
        for (int i = 0; i < segments - 1; i++)
        {
            Debug.DrawLine(grappleRope.GetPosition(i), grappleRope.GetPosition(i + 1), Color.red, 1f);
        }
        Debug.DrawRay(ropeStartPoint.position, Vector3.up * 0.5f, Color.green, 1f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, grappleRange);
    }

    public bool IsGrappling()
    {
        return isGrappling;
    }
    public void DisableGrapple()
    {
        grappleRope.enabled = false;
        grappleRope.positionCount = 0;
    }
}