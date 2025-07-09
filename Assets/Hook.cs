using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public float grappleRange = 10f; // 勾鎖檢測範圍
    public float maxGrappleDistance = 15f; // 最大勾鎖距離
    public float grappleSpeed = 10f; // 勾鎖移動速度
    public LayerMask grappleLayer; // 勾鎖點的層
    public Camera mainCamera; // 主攝影機
    public Animator animator; // 玩家動畫控制器
    public CharacterController controller; // 角色控制器
    public LineRenderer grappleRope; // 繩索的LineRenderer

    private List<HookPoint> nearbyPoints = new List<HookPoint>();
    private bool isGrappling = false;

    void Start()
    {
        // 初始化繩索效果
        if (grappleRope != null)
        {
            grappleRope.enabled = false;
        }
    }

    void Update()
    {
        // 檢測範圍內的勾鎖點
        UpdateNearbyPoints();

        // 按下E鍵進行勾鎖
        if (Input.GetKeyDown(KeyCode.E) && !isGrappling)
        {
            TryGrapple();
        }
    }

    void UpdateNearbyPoints()
    {
        // 清除之前的高亮
        foreach (var point in nearbyPoints)
        {
            point.Highlight(false);
        }
        nearbyPoints.Clear();

        // 檢測範圍內的勾鎖點
        Collider[] colliders = Physics.OverlapSphere(transform.position, grappleRange, grappleLayer);
        foreach (var collider in colliders)
        {
            HookPoint point = collider.GetComponent<HookPoint>();
            if (point != null && point.isGrapplable)
            {
                point.Highlight(true);
                nearbyPoints.Add(point);
            }
        }
    }

    void TryGrapple()
    {
        // 從攝影機發射射線
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGrappleDistance, grappleLayer))
        {
            HookPoint targetPoint = hit.collider.GetComponent<HookPoint>();
            if (targetPoint != null && targetPoint.isGrapplable)
            {
                StartGrapple(targetPoint);
            }
        }
    }

    void StartGrapple(HookPoint target)
    {
        isGrappling = true;

        // 播放勾鎖動畫
        if (animator != null)
        {
            animator.SetTrigger("Grapple");
        }

        // 禁用CharacterController的默認行為（可選，根據你的需求）
        if (controller != null)
        {
            controller.enabled = false; // 臨時禁用以直接控制移動
        }

        // 啟動繩索效果
        if (grappleRope != null)
        {
            grappleRope.enabled = true;
            StartCoroutine(UpdateRope(target.transform.position));
        }

        // 開始移動到勾鎖點
        StartCoroutine(MoveToHookPoint(target.transform.position));
    }

    IEnumerator MoveToHookPoint(Vector3 targetPos)
    {
        // 平滑移動到目標點
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            Vector3 move = direction * grappleSpeed * Time.deltaTime;

            // 使用CharacterController.Move進行移動
            if (controller != null)
            {
                transform.position += move; // 直接修改位置（因為controller已禁用）
            }

            yield return null;
        }

        // 恢復CharacterController
        if (controller != null)
        {
            controller.enabled = true;
        }

        isGrappling = false;
    }

    IEnumerator UpdateRope(Vector3 targetPos)
    {
        // 更新繩索的起點和終點
        while (isGrappling)
        {
            grappleRope.SetPosition(0, transform.position); // 繩索起點
            grappleRope.SetPosition(1, targetPos); // 繩索終點
            yield return null;
        }

        // 隱藏繩索
        grappleRope.enabled = false;
    }

    // 調試：可視化檢測範圍
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, grappleRange);
    }

    // 提供外部訪問，檢查是否正在勾鎖
    public bool IsGrappling()
    {
        return isGrappling;
    }
}