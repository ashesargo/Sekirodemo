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
    public float ropeExtendDuration = 0.1f; // 繩索射出時長（射出階段）
    public int ropeSegments = 20; // 繩索細分節點數
    public float sagAmount = 1.0f; // 繩索下垂幅度
    public float pullTautDuration = 0.5f; // 繩索拉緊過渡時間
    public LayerMask grappleLayer; // 勾鎖點的層
    public Camera mainCamera; // 主攝影機
    public Animator animator; // 玩家動畫控制器
    public CharacterController controller; // 角色控制器
    public LineRenderer grappleRope; // 繩索的LineRenderer
    public Transform ropeStartPoint; // 繩索起點（手部位置）

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
        if (ropeStartPoint == null)
        {
            Debug.LogWarning("RopeStartPoint 未分配，將使用玩家位置作為繩索起點");
            ropeStartPoint = transform;
        }
    }

    void Update()
    {
        UpdateNearbyPoints();

        if (Input.GetKeyDown(KeyCode.E) && !isGrappling)
        {
            TryGrapple();
        }
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
                StartGrapple(targetPoint);
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
            StartGrapple(closestPoint);
        }
    }

    void StartGrapple(GrapplePoint target)
    {
        isGrappling = true;
        currentTargetPos = target.transform.position;

        if (animator != null)
        {
            animator.SetTrigger("Grapple"); // 觸發射出動畫
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

        if (grappleRope != null)
        {
            grappleRope.enabled = true;
            grappleRope.positionCount = 0; // 重置繩索
            StartCoroutine(DelayedUpdateRope(currentTargetPos));
        }
    }

    // 由動畫事件調用，開始繩索射出
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

    // 由動畫事件調用，開始移動和 JumpToTarget
    public void StartMoveToGrapple()
    {
        Debug.Log("StartMoveToGrapple called at: " + Time.time);
        if (animator != null)
        {
            animator.SetBool("IsJumping", true);
        }
        StartCoroutine(MoveToGrapplePoint(currentTargetPos));
    }

    IEnumerator MoveToGrapplePoint(Vector3 targetPos)
    {
        float elapsedTime = 0f;
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            Vector3 move = direction * grappleSpeed * Time.deltaTime;
            transform.position += move;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (animator != null)
        {
            animator.SetBool("IsJumping", false);
            animator.ResetTrigger("Grapple");
        }

        if (controller != null)
        {
            controller.enabled = true;
        }

        isGrappling = false;
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
                elapsedTime += Time.deltaTime;
            }
            yield return null;
        }
        grappleRope.enabled = false;
        grappleRope.positionCount = 0;
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
            Vector3 currentEndPos = Vector3.Lerp(ropeStartPoint.position, targetPos, t);
            UpdateRopeWithSag(ropeStartPoint.position, currentEndPos, sagAmount, currentSegments);
            Debug.Log($"ExtendRope: t={t}, segments={currentSegments}, endPos={currentEndPos}");
            yield return null;
        }

        UpdateRopeWithSag(ropeStartPoint.position, targetPos, sagAmount, ropeSegments);
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
}