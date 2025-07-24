using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static Transform CachedPlayer; // 靜態快取
    public Transform player { get { return CachedPlayer; } }
    public Animator animator;
    public Rigidbody rb; // 新增Rigidbody引用

    public float visionRange = 1000f;
    public float attackRange = 2f;

    public float maxSpeed = 3f;
    public float avoidDistance = 1.5f;
    public float avoidStrength = 10f;
    public LayerMask obstacleMask = 1 << 9; // 只檢測 Obstacle 層 (Layer 9)

    [Header("AI 參數")]
    public float retreatTime = 2f;

    [Header("敵人碰撞避免")]
    public float enemyAvoidDistance = 2f;
    public float enemyAvoidStrength = 15f;
    public LayerMask enemyLayerMask = 1 << 6; // 只檢測 Enemy 層 (Layer 6)

    [Header("立方體避障系統")]
    public Vector3 detectionBoxSize = new Vector3(2f, 1f, 1.5f); // 立方體大小
    public float detectionBoxOffset = 1f; // 立方體前方偏移距離
    public float detectionBoxYOffset = 0f; // 立方體Y軸偏移距離
    public bool showDetectionBox = true; // 是否顯示偵測立方體
    
    [Header("撤退避障設定")]
    public Vector3 retreatBoxSize = new Vector3(0.8f, 0.5f, 0.8f); // 撤退專用立方體大小（更小）
    public float retreatBoxOffset = 0.5f; // 撤退立方體偏移距離（更近）
    public float retreatBoxYOffset = 0f; // 撤退立方體Y軸偏移距離
    public LayerMask retreatObstacleMask = (1 << 9) | (1 << 6); // 檢測 Obstacle 層 (9) 和 Enemy 層 (6)

    [Header("調試選項")]
    public bool showDebugInfo = false;
    [Header("轉向速度 (越小越慢)")]
    public float lookAtTurnSpeed = 2f;
    [Header("與玩家最小距離 (避免推擠)")]
    public float minDistanceToPlayer = 1.2f;

    private BaseEnemyState currentState;
    [HideInInspector] public Vector3 velocity;
    public bool canAutoAttack = true;
    private bool isObstacleDetected = false; // 是否檢測到障礙物
    public bool canBeParried = false;

    public BaseEnemyState CurrentState => currentState;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>(); // 取得Rigidbody
        // 只在第一次時尋找並快取
        if (CachedPlayer == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                CachedPlayer = playerObj.transform;
        }
    }

    void Start()
    {
        SwitchState(new IdleState());
        
        // 調試：檢查 obstacleMask 設置
        Debug.Log($"EnemyAI Start - obstacleMask: {obstacleMask.value}, avoidDistance: {avoidDistance}");
        
        // 檢查自己的 Collider
        Collider myCollider = GetComponent<Collider>();
        if (myCollider == null)
        {
            Debug.LogError($"敵人 {gameObject.name} 沒有 Collider 組件！敵人避障需要 Collider 才能工作。");
        }
        else
        {
            Debug.Log($"敵人 {gameObject.name} 有 Collider: {myCollider.GetType().Name}, Layer: {gameObject.layer}");
        }
    }

    void Update()
    {
        if (player == null) return; // 找不到就不執行狀態機
        currentState?.UpdateState(this);
    }

    public void SwitchState(BaseEnemyState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState?.EnterState(this);
    }

    public bool CanSeePlayer()
    {
        return Vector3.Distance(transform.position, player.position) < visionRange;
    }

    public bool IsInAttackRange()
    {
        return Vector3.Distance(transform.position, player.position) < attackRange;
    }

    public Vector3 Seek(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position);
        direction.y = 0f; // 只考慮水平方向
        
        if (direction.magnitude < 0.1f)
            return Vector3.zero;
            
        Vector3 desired = direction.normalized * maxSpeed;
        Vector3 steering = desired - velocity;
        
        return steering;
    }

    // 追擊專用避障方法 - 使用前方立方體檢測
    public Vector3 ObstacleAvoid()
    {
        Vector3 avoid = Vector3.zero;
        isObstacleDetected = false;
        
        // 計算前方偵測立方體的位置和旋轉（用於追擊）
        Vector3 boxCenter = transform.position + transform.forward * detectionBoxOffset + Vector3.up * detectionBoxYOffset;
        Quaternion boxRotation = transform.rotation;
        
        // 使用 OverlapBox 檢測前方立方體內的障礙物
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, detectionBoxSize * 0.5f, boxRotation, obstacleMask);
        
        if (showDebugInfo)
        {
            Debug.Log($"ObstacleAvoid - 前方立方體中心: {boxCenter}, 大小: {detectionBoxSize}, 檢測到 {hitColliders.Length} 個 Collider");
        }
        
        if (hitColliders.Length > 0)
        {
            isObstacleDetected = true;
            
            // 計算避障方向 - 遠離所有檢測到的障礙物
            foreach (var hitCollider in hitColliders)
            {
                // 跳過自己的 Collider
                if (hitCollider.gameObject == gameObject)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"ObstacleAvoid - 跳過自己的 Collider: {hitCollider.name}");
                    }
                    continue;
                }
                
                Vector3 directionToObstacle = hitCollider.transform.position - transform.position;
                directionToObstacle.y = 0f; // 只考慮水平方向
                
                if (directionToObstacle.magnitude > 0.1f)
                {
                    Vector3 awayFromObstacle = -directionToObstacle.normalized;
                    float distance = directionToObstacle.magnitude;
                    
                    // 根據距離調整避障強度，距離越近強度越大
                    float distanceRatio = Mathf.Clamp01(1f - (distance / detectionBoxSize.z));
                    float adjustedStrength = avoidStrength * distanceRatio;
                    
                    avoid += awayFromObstacle * adjustedStrength;
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"ObstacleAvoid - 檢測到前方障礙物: {hitCollider.name}, 距離: {distance:F2}, 避障力: {awayFromObstacle * adjustedStrength}");
                    }
                }
            }
            
            // 限制避障力的最大值
            if (avoid.magnitude > maxSpeed)
            {
                avoid = avoid.normalized * maxSpeed;
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log("ObstacleAvoid - 沒有檢測到前方障礙物");
            }
        }
        
        if (showDebugInfo && avoid.magnitude > 0.1f)
        {
            Debug.Log($"ObstacleAvoid - 前方總避障力: {avoid}");
        }
        
        return avoid;
    }

    public Vector3 EnemyAvoid()
    {
        Vector3 avoid = Vector3.zero;
        
        // 調試：檢查敵人避障設置
        if (showDebugInfo)
        {
            Debug.Log($"EnemyAvoid - enemyAvoidDistance: {enemyAvoidDistance}, enemyAvoidStrength: {enemyAvoidStrength}, enemyLayerMask: {enemyLayerMask.value}");
        }
        
        // 檢測周圍的敵人
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, enemyAvoidDistance, enemyLayerMask);
        
        if (showDebugInfo)
        {
            Debug.Log($"EnemyAvoid - 檢測到 {nearbyEnemies.Length} 個附近的 Collider");
        }
        
        foreach (var enemyCollider in nearbyEnemies)
        {
            // 跳過自己
            if (enemyCollider.gameObject == gameObject)
                continue;
                
            // 檢查是否為敵人
            EnemyAI otherEnemy = enemyCollider.GetComponent<EnemyAI>();
            if (otherEnemy == null)
                continue;
                
            Vector3 directionToEnemy = enemyCollider.transform.position - transform.position;
            directionToEnemy.y = 0f; // 只考慮水平方向
            
            if (directionToEnemy.magnitude > 0.1f)
            {
                Vector3 awayFromEnemy = -directionToEnemy.normalized;
                float distance = directionToEnemy.magnitude;
                
                // 根據距離調整避障強度
                float distanceRatio = Mathf.Clamp01(1f - (distance / enemyAvoidDistance));
                float adjustedStrength = enemyAvoidStrength * distanceRatio;
                
                avoid += awayFromEnemy * adjustedStrength;
                
                if (showDebugInfo)
                {
                    Debug.Log($"EnemyAvoid - 檢測到敵人: {otherEnemy.name}, 距離: {distance:F2}, 避障力: {awayFromEnemy * adjustedStrength}");
                }
            }
        }
        
        // 限制避障力的最大值
        if (avoid.magnitude > maxSpeed)
        {
            avoid = avoid.normalized * maxSpeed;
        }
        
        if (showDebugInfo && avoid.magnitude > 0.1f)
        {
            Debug.Log($"EnemyAvoid - 總避障力: {avoid}");
        }
        
        return avoid;
    }

    // 狀態切換時由各狀態決定是否啟用Root Motion
    public void SetRootMotion(bool useRootMotion)
    {
        if (animator != null)
            animator.applyRootMotion = useRootMotion;
    }

    // Root Motion推動Rigidbody（如有）
    void OnAnimatorMove()
    {
        if (animator.applyRootMotion)
        {
            if (rb != null)
            {
                rb.MovePosition(animator.rootPosition);
                rb.MoveRotation(animator.rootRotation);
            }
            else
            {
                transform.position = animator.rootPosition;
                transform.rotation = animator.rootRotation;
            }
        }
    }

    // 追擊、Idle等狀態下仍可用Move（不啟用Root Motion）
    public void Move(Vector3 force, bool lookAtPlayer = false)
    {
        if (animator != null && animator.applyRootMotion)
            return; // 啟用Root Motion時不由程式移動
        force.y = 0f;
        if (player != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer < minDistanceToPlayer)
            {
                velocity = Vector3.zero;
                if (lookAtPlayer)
                {
                    Vector3 lookDir = (player.position - transform.position);
                    lookDir.y = 0f;
                    if (lookDir.magnitude > 0.1f)
                        transform.forward = lookDir.normalized;
                }
                return;
            }
        }
        velocity += force * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        velocity.y = 0f;
        transform.position += velocity * Time.deltaTime;

        if (lookAtPlayer && player != null)
        {
            Vector3 lookDir = (player.position - transform.position);
            lookDir.y = 0f;
            if (lookDir.magnitude > 0.1f)
                transform.forward = lookDir.normalized;
        }
        else if (velocity.magnitude > 0.1f)
        {
            Vector3 lookDirection = velocity.normalized;
            lookDirection.y = 0f;
            if (lookDirection.magnitude > 0.1f)
                transform.forward = lookDirection;
        }
    }

    public void Stop()
    {
        velocity = Vector3.zero;
    }

    // 近戰攻擊時對玩家造成傷害（前方90度扇形範圍判定）
    public void DealMeleeDamage(float damage)
    {
        if (player == null) return;
        Vector3 toPlayer = (player.position - transform.position);
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        if (distance < attackRange + 1f) // 距離判定
        {
            Vector3 forward = transform.forward;
            float angle = Vector3.Angle(forward, toPlayer);
            if (angle <= 45f) // 前方90度扇形
            {
                var status = player.GetComponent<PlayerStatus>();
                if (status != null)
                {
                    status.TakeDamage(damage);
                }
            }
        }
    }

    // 新增：平滑轉向玩家的方法
    public void SmoothLookAt(Vector3 targetPos, float turnSpeed = -1f)
    {
        if (turnSpeed < 0f) turnSpeed = lookAtTurnSpeed;
        Vector3 lookDir = targetPos - transform.position;
        lookDir.y = 0f;
        if (lookDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, turnSpeed * Time.deltaTime * 60f);
        }
    }

    // 動畫事件呼叫
    public void ParryWindowStart()
    {
        canBeParried = true;
        // 這時可以加特效、音效
    }

    public void ParryWindowEnd()
    {
        canBeParried = false;
        // 這時可以關閉特效
    }

    //// 視覺化調試
    //void OnDrawGizmos()
    //{
    //    // 繪製敵人避障範圍
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, enemyAvoidDistance);
        
    //    // 繪製攻擊範圍
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, attackRange);
        
    //    // 繪製視覺範圍
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawWireSphere(transform.position, visionRange);
        
    //    // 繪製偵測立方體
    //    if (showDetectionBox)
    //    {
    //        Vector3 boxCenter = transform.position + transform.forward * detectionBoxOffset + Vector3.up * detectionBoxYOffset;
            
    //        // 根據是否檢測到障礙物改變顏色
    //        Gizmos.color = isObstacleDetected ? Color.red : Color.green;
            
    //        // 繪製立方體邊線
    //        Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
    //        Gizmos.DrawWireCube(Vector3.zero, detectionBoxSize);
    //        Gizmos.matrix = Matrix4x4.identity;
            
    //        // 繪製立方體中心點（調試用）
    //        Gizmos.color = Color.white;
    //        Gizmos.DrawWireSphere(boxCenter, 0.1f);
            
    //        // 繪製從敵人到立方體中心的線（調試用）
    //        Gizmos.color = Color.cyan;
    //        Gizmos.DrawLine(transform.position, boxCenter);
            
    //        // 繪製撤退偵測立方體（橙色，基於背向玩家方向）
    //        Vector3 retreatDirection = (transform.position - player.position).normalized;
    //        retreatDirection.y = 0f;
    //        Vector3 retreatBoxCenter = transform.position + retreatDirection * retreatBoxOffset + Vector3.up * retreatBoxYOffset;
    //        Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // 橙色
    //        // 使用基於背向玩家方向的旋轉
    //        Quaternion retreatBoxRotation = Quaternion.LookRotation(retreatDirection);
    //        Gizmos.matrix = Matrix4x4.TRS(retreatBoxCenter, retreatBoxRotation, Vector3.one);
    //        Gizmos.DrawWireCube(Vector3.zero, retreatBoxSize);
    //        Gizmos.matrix = Matrix4x4.identity;
            
    //        // 繪製撤退立方體中心點
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawWireSphere(retreatBoxCenter, 0.1f);
            
    //        // 繪製從敵人到撤退立方體中心的線
    //        Gizmos.color = Color.magenta;
    //        Gizmos.DrawLine(transform.position, retreatBoxCenter);
    //    }
    //}

    // 撤退專用避障方法 - 使用後方立方體檢測
    public Vector3 RetreatObstacleAvoid()
    {
        Vector3 avoid = Vector3.zero;
        
        // 計算背向玩家的方向（用於撤退立方體位置和避障）
        Vector3 retreatDirection = (transform.position - player.position).normalized;
        retreatDirection.y = 0f;
        
        // 計算後方偵測立方體的位置和旋轉（基於背向玩家方向）
        Vector3 retreatBoxCenter = transform.position + retreatDirection * retreatBoxOffset + Vector3.up * retreatBoxYOffset;
        // 使用基於背向玩家方向的旋轉
        Quaternion retreatBoxRotation = Quaternion.LookRotation(retreatDirection);
        
        // 使用 OverlapBox 檢測後方立方體內的障礙物（使用更小的撤退立方體）
        Collider[] hitColliders = Physics.OverlapBox(retreatBoxCenter, retreatBoxSize * 0.5f, retreatBoxRotation, retreatObstacleMask);
        
        if (showDebugInfo)
        {
            Debug.Log($"RetreatObstacleAvoid - 檢測到 {hitColliders.Length} 個指定層的 Collider (Obstacle + Enemy)");
            Debug.Log($"RetreatObstacleAvoid - retreatObstacleMask: {retreatObstacleMask.value}");
        }
        
        if (hitColliders.Length > 0)
        {
            // 計算避障方向 - 遠離所有檢測到的障礙物
            foreach (var hitCollider in hitColliders)
            {
                // 跳過自己的 Collider
                if (hitCollider.gameObject == gameObject)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"RetreatObstacleAvoid - 跳過自己的 Collider: {hitCollider.name}");
                    }
                    continue;
                }
                
                Vector3 directionToObstacle = hitCollider.transform.position - transform.position;
                directionToObstacle.y = 0f; // 只考慮水平方向
                
                if (directionToObstacle.magnitude > 0.1f)
                {
                    Vector3 awayFromObstacle = -directionToObstacle.normalized;
                    float distance = directionToObstacle.magnitude;
                    
                    // 根據距離調整避障強度，撤退時使用正常避障強度
                    float distanceRatio = Mathf.Clamp01(1f - (distance / retreatBoxSize.z));
                    float adjustedStrength = avoidStrength * distanceRatio; // 使用正常避障強度
                    
                    avoid += awayFromObstacle * adjustedStrength;
                    
                    if (showDebugInfo)
                    {
                        string objectType = hitCollider.gameObject.layer == 9 ? "障礙物" : "敵人";
                        Debug.Log($"RetreatObstacleAvoid - 檢測到{objectType}: {hitCollider.name}, Layer: {hitCollider.gameObject.layer}, 距離: {distance:F2}, 避障力: {awayFromObstacle * adjustedStrength}");
                    }
                }
            }
            
            // 限制避障力的最大值
            if (avoid.magnitude > maxSpeed)
            {
                avoid = avoid.normalized * maxSpeed;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"RetreatObstacleAvoid - 最終避障力: {avoid}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log("RetreatObstacleAvoid - 沒有檢測到障礙物或敵人，不產生避障力");
            }
        }
        
        return avoid;
    }
}