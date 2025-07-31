using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGrapple : MonoBehaviour
{
    public float grappleRange = 10f; // 勾鎖檢測範圍
    public float maxGrappleDistance = 15f; // 最大勾鎖距離
    public float grappleSpeed = 10f; // 勾鎖移動速度
    public float sphereRadius = 10f; // SphereCast的球體半徑
    public float ropeWidth = 0.05f; // 繩索寬度
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
    public Material ropeMaterial; // 繩索材質

    // 新增的 UI 圖示相關欄位
    [SerializeField] private Canvas indicatorCanvas; // 獨立的 UI Canvas
    [SerializeField] private GameObject indicatorPrefab; // UI 圖示預製體
    [SerializeField] private Sprite grayCircleSprite; // 灰色圓圈圖片
    [SerializeField] private Sprite greenCircleSprite; // 綠色圓圈圖片
    [SerializeField] private float edgeOffset = 10f; // 螢幕邊緣偏移量

    private List<GrapplePoint> nearbyPoints = new List<GrapplePoint>();
    private bool isGrappling = false;
    private bool isExtendingRope = false;
    private Vector3 currentTargetPos;
    private Coroutine extendRopeCoroutine;
    private GrapplePoint selectedPoint;
    AudioSource _audioSource;
    public AudioClip ropeSound;
    // 新增的目標和圖示管理
    [System.Serializable]
    private class TargetInfo
    {
        public Transform target; // 目標物件
        public Image indicatorImage; // 對應的 UI 圖示
        public RectTransform indicatorRect; // 圖示的 RectTransform
        public bool isInRange; // 是否在偵測範圍內
        public bool isHitByRay; // 是否被射線擊中
    }

    private List<TargetInfo> targetInfos = new List<TargetInfo>(); // 儲存目標和對應圖示的資訊
    private CanvasScaler canvasScaler; // Canvas 的縮放器

    void Start()
    {
        // 初始化 LineRenderer
        if (grappleRope != null)
        {
            grappleRope.enabled = false;
            grappleRope.startWidth = ropeWidth;
            grappleRope.endWidth = ropeWidth * 0.8f;
            grappleRope.positionCount = 0;
        }
        // 檢查 grappleLayer 是否有效
        string layerName = LayerMask.LayerToName(Mathf.FloorToInt(Mathf.Log(grappleLayer.value, 2)));

        // 初始化 UI 圖示管理
        if (indicatorCanvas != null)
        {
            canvasScaler = indicatorCanvas.GetComponent<CanvasScaler>();
        }
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        UpdateNearbyPoints();
        UpdateIndicators(); // 新增的 UI 圖示更新

        if (Input.GetKeyDown(KeyCode.F) && !isGrappling)
        {
            TryGrapple();
        }
    }

    void LateUpdate()
    {
        if (isGrappling && grappleRope != null && grappleRope.enabled)
        {
            Vector3 ropeStartPos = ropeStartPoint.position;
            Vector3 ropeEndPos = currentTargetPos;
            UpdateRopeWithSag(ropeStartPos, ropeEndPos, sagAmount, ropeSegments);
        }
    }

    void UpdateNearbyPoints()
    {
        nearbyPoints.Clear();
        Collider[] colliders = Physics.OverlapSphere(transform.position, grappleRange, grappleLayer);
        string layerName = grappleLayer.value == 0 ? "未設置" : LayerMask.LayerToName(Mathf.FloorToInt(Mathf.Log(grappleLayer.value, 2)));

        // 取得攝影機前方向量（忽略 y 軸以專注於水平朝向）
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0; // 忽略垂直方向
        cameraForward.Normalize();

        // 使用滑鼠游標的射線來選擇最佳點
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.SphereCastAll(ray, sphereRadius, maxGrappleDistance, grappleLayer);
        GrapplePoint bestPoint = null;
        float minAngle = float.MaxValue;

        foreach (var collider in colliders)
        {
            GrapplePoint point = collider.GetComponent<GrapplePoint>();
            if (point != null && point.isGrapplable)
            {
                // 計算從玩家到可抓取點的方向向量
                Vector3 directionToPoint = (point.transform.position - transform.position).normalized;
                directionToPoint.y = 0; // 忽略垂直方向
                directionToPoint.Normalize();

                // 檢查點積是否為正（即點是否在攝影機前方）
                float dotProduct = Vector3.Dot(cameraForward, directionToPoint);
                if (dotProduct > 0) // 僅當點在攝影機前方
                {
                    // 計算與滑鼠射線的夾角
                    Vector3 pointWorldPos = point.transform.position;
                    Vector3 screenPointPos = mainCamera.WorldToScreenPoint(pointWorldPos);
                    Vector3 mousePos = Input.mousePosition;
                    float angle = Vector2.Angle(new Vector2(screenPointPos.x, screenPointPos.y), new Vector2(mousePos.x, mousePos.y));

                    // 選擇與滑鼠游標夾角最小的點
                    if (angle < minAngle)
                    {
                        minAngle = angle;
                        bestPoint = point;
                    }
                }
            }
        }

        // 僅將最佳點添加到 nearbyPoints
        if (bestPoint != null)
        {
            nearbyPoints.Add(bestPoint);
            // 為最佳點創建圖示（如果尚未創建）
            if (!targetInfos.Exists(info => info.target == bestPoint.transform))
            {
                CreateIndicatorForTarget(bestPoint.transform);
            }
        }

        // 移除非最佳點的目標資訊
        targetInfos.RemoveAll(info =>
        {
            if (info.target == null || (bestPoint != null && info.target != bestPoint.transform))
            {
                if (info.indicatorImage != null)
                {
                    Destroy(info.indicatorImage.gameObject);
                }
                return true;
            }
            return false;
        });

        // 更新射線擊中狀態
        foreach (var info in targetInfos)
        {
            info.isHitByRay = false;
            foreach (var hit in hits)
            {
                GrapplePoint point = hit.collider.GetComponent<GrapplePoint>();
                if (point != null && point.isGrapplable && info.target == point.transform)
                {
                    info.isHitByRay = true;
                    break;
                }
            }
        }
    }

    void TryGrapple()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * maxGrappleDistance, Color.yellow, 2f);
        RaycastHit[] hits = Physics.SphereCastAll(ray, sphereRadius, maxGrappleDistance, grappleLayer);
        GrapplePoint closestPoint = null;
        float minAngle = float.MaxValue;

        // 取得攝影機前方向量
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        foreach (var hit in hits)
        {
            GrapplePoint point = hit.collider.GetComponent<GrapplePoint>();
            if (point != null && point.isGrapplable)
            {
                // 檢查點是否在攝影機前方
                Vector3 directionToPoint = (point.transform.position - transform.position).normalized;
                directionToPoint.y = 0;
                directionToPoint.Normalize();
                float dotProduct = Vector3.Dot(cameraForward, directionToPoint);
                if (dotProduct > 0)
                {
                    // 計算與滑鼠游標的夾角
                    Vector3 pointWorldPos = point.transform.position;
                    Vector3 screenPointPos = mainCamera.WorldToScreenPoint(pointWorldPos);
                    Vector3 mousePos = Input.mousePosition;
                    float angle = Vector2.Angle(new Vector2(screenPointPos.x, screenPointPos.y), new Vector2(mousePos.x, mousePos.y));

                    if (angle < minAngle)
                    {
                        minAngle = angle;
                        closestPoint = point;
                    }
                }
            }
        }

        if (closestPoint != null)
        {
            selectedPoint = closestPoint;
            StartCoroutine(StartGrappleWithTurn(closestPoint));
        }
    }
    public void PlayRopeSound()
    {
        _audioSource.PlayOneShot(ropeSound);
    }
    void UpdateIndicators()
    {
        // 取得攝影機前方向量（忽略 y 軸以專注於水平朝向）
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0; // 忽略垂直方向
        cameraForward.Normalize();

        foreach (TargetInfo info in targetInfos)
        {
            if (info.target == null) // 檢查目標是否已被銷毀
            {
                if (info.indicatorImage != null)
                {
                    Destroy(info.indicatorImage.gameObject);
                }
                continue;
            }

            // 計算從玩家到可抓取點的方向向量
            Vector3 directionToPoint = (info.target.position - transform.position).normalized;
            directionToPoint.y = 0; // 忽略垂直方向
            directionToPoint.Normalize();

            // 檢查點積是否為正（即點是否在攝影機前方）
            float dotProduct = Vector3.Dot(cameraForward, directionToPoint);
            if (dotProduct <= 0) // 如果點不在攝影機前方，隱藏圖示
            {
                info.indicatorImage.enabled = false;
                continue;
            }

            // 計算目標與玩家的距離
            float distanceToTarget = Vector3.Distance(transform.position, info.target.position);
            info.isInRange = distanceToTarget <= grappleRange;

            // 當目標不在範圍內時不顯示
            if (!info.isInRange)
            {
                info.indicatorImage.enabled = false;
                continue;
            }

            // 目標在範圍內且在攝影機前方，啟用圖示
            info.indicatorImage.enabled = true;

            // 根據射線是否擊中選擇圖示
            info.indicatorImage.sprite = info.isHitByRay ? greenCircleSprite : grayCircleSprite;

            // 將目標世界座標轉換為螢幕座標
            Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(info.target.position);

            // 判斷目標是否在攝影機視野內
            bool isTargetVisible = targetScreenPos.z > 0 &&
                                  targetScreenPos.x > 0 && targetScreenPos.x < Screen.width &&
                                  targetScreenPos.y > 0 && targetScreenPos.y < Screen.height;

            if (isTargetVisible)
            {
                // 目標在攝影機視野內，直接顯示在目標位置
                info.indicatorRect.position = targetScreenPos;
            }
            else
            {
                // 目標在偵測範圍內但不在攝影機視野內，顯示在螢幕邊緣
                Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
                Vector3 direction = (targetScreenPos - screenCenter).normalized;

                // 計算螢幕邊緣位置
                Vector2 canvasSize = canvasScaler.referenceResolution;
                float canvasScale = canvasScaler.scaleFactor;
                float edgeX = (Screen.width / canvasScale - edgeOffset) / 2f;
                float edgeY = (Screen.height / canvasScale - edgeOffset) / 2f;

                // 限制圖示在螢幕邊緣
                float maxX = edgeX;
                float maxY = edgeY;
                float ratio = Mathf.Min(maxX / Mathf.Abs(direction.x), maxY / Mathf.Abs(direction.y));
                Vector3 edgePosition = screenCenter + direction * ratio * canvasScale;

                info.indicatorRect.position = edgePosition;
            }
        }

        // 清理無效的目標資訊
        targetInfos.RemoveAll(info => info.target == null || info.indicatorImage == null);
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
            float t = Mathf.Clamp01(elapsedTime / turnDuration);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }
        transform.rotation = targetRotation;

        StartGrapple(target);
    }

    void StartGrapple(GrapplePoint target)
    {
        isGrappling = true;
        currentTargetPos = target.transform.position;

        if (animator != null)
        {
            animator.SetTrigger("Grapple");
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        if (grappleRope != null)
        {
            grappleRope.enabled = true;
            grappleRope.positionCount = 2;
            grappleRope.SetPosition(0, ropeStartPoint.position);
            grappleRope.SetPosition(1, currentTargetPos);
            StartCoroutine(DelayedUpdateRope(currentTargetPos));
        }
    }

    public void StartRopeExtend()
    {
        if (extendRopeCoroutine != null)
        {
            StopCoroutine(extendRopeCoroutine);
        }
        isExtendingRope = true;
        extendRopeCoroutine = StartCoroutine(ExtendRope(currentTargetPos));
    }

    public void Grapple2()
    {
        if (animator != null)
        {
            animator.SetTrigger("Grapple2");
            StartCoroutine(DelayedMoveToGrapple());
        }
    }

    IEnumerator DelayedMoveToGrapple()
    {
        yield return new WaitForSeconds(grapple2Duration);
        animator.SetBool("IsJumping", true);
        float distance = Vector3.Distance(transform.position, currentTargetPos);
        float moveTime = distance / grappleSpeed;
        animator.speed = jumpAnimationDuration / Mathf.Max(moveTime, 0.1f);
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
            yield return null;
        }

        transform.position = targetPos;

        float remainingTime = jumpAnimationDuration - elapsedTime;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        if (animator != null)
        {
            animator.SetBool("IsJumping", false);
            animator.ResetTrigger("Grapple");
            animator.ResetTrigger("Grapple2");
            animator.speed = 1f;
        }

        if (controller != null)
        {
            controller.enabled = true;
        }

        isGrappling = false;
        nearbyPoints.Clear();
        selectedPoint = null;

        if (grappleRope != null)
        {
            grappleRope.enabled = false;
            grappleRope.positionCount = 0;
        }
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
        if (grappleRope != null)
        {
            grappleRope.enabled = false;
            grappleRope.positionCount = 0;
        }
    }

    IEnumerator ExtendRope(Vector3 targetPos)
    {
        if (grappleRope == null) yield break;

        grappleRope.startWidth = ropeWidth;
        grappleRope.endWidth = ropeWidth * 0.8f;
        grappleRope.enabled = true;

        float elapsedTime = 0f;
        while (elapsedTime < ropeExtendDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / ropeExtendDuration;
            int currentSegments = Mathf.FloorToInt(Mathf.Lerp(2, ropeSegments, t));
            Vector3 ropeStartPos = ropeStartPoint.position;
            Vector3 ropeEndPos = Vector3.Lerp(ropeStartPoint.position, targetPos, t);
            UpdateRopeWithSag(ropeStartPos, ropeEndPos, sagAmount, currentSegments);
            yield return null;
        }
        isExtendingRope = false;
    }

    void UpdateRopeWithSag(Vector3 startPos, Vector3 endPos, float currentSag, int segments)
    {
        if (grappleRope == null) return;

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
    }

    public bool IsGrappling()
    {
        return isGrappling;
    }

    public void DisableGrapple()
    {
        if (grappleRope != null)
        {
            grappleRope.enabled = false;
            grappleRope.positionCount = 0;
        }
    }

    void CreateIndicatorForTarget(Transform target)
    {
        if (indicatorPrefab == null || indicatorCanvas == null)
        {
            return;
        }

        // 實例化 UI 圖示預製體
        GameObject indicatorObj = Instantiate(indicatorPrefab, indicatorCanvas.transform);
        Image indicatorImage = indicatorObj.GetComponent<Image>();
        RectTransform indicatorRect = indicatorObj.GetComponent<RectTransform>();

        if (indicatorImage == null || indicatorRect == null)
        {
            Destroy(indicatorObj);
            return;
        }

        // 初始化圖示為隱藏
        indicatorImage.enabled = false;
        indicatorImage.sprite = grayCircleSprite;

        // 添加到目標資訊清單
        TargetInfo info = new TargetInfo
        {
            target = target,
            indicatorImage = indicatorImage,
            indicatorRect = indicatorRect,
            isInRange = true,
            isHitByRay = false
        };
        targetInfos.Add(info);
    }
}