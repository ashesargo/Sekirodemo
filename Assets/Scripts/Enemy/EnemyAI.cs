using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public Animator animator;

    public float visionRange = 1000f;
    public float attackRange = 2f;

    public float maxSpeed = 3f;
    public float avoidDistance = 1.5f;
    public float avoidStrength = 10f;
    public LayerMask obstacleMask;

    [Header("AI 參數")]
    public float retreatTime = 2f;

    [Header("敵人碰撞避免")]
    public float enemyAvoidDistance = 2f;
    public float enemyAvoidStrength = 15f;
    public LayerMask enemyLayerMask = -1; // 預設為所有層

    [Header("調試選項")]
    public bool showObstacleRays = true;
    public bool showDebugInfo = false;

    private IEnemyState currentState;
    [HideInInspector] public Vector3 velocity;
    public bool canAutoAttack = true;

    public IEnemyState CurrentState => currentState;

    void Start()
    {
        animator = GetComponent<Animator>();
        FindPlayer();
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
        // 如果 player 尚未找到，每幀嘗試尋找
        if (player == null)
        {
            FindPlayer();
            if (player == null) return; // 找不到就不執行狀態機
        }
        currentState?.UpdateState(this);
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            //Debug.Log("Player found: " + player.name);
        }
        else
        {
            Debug.LogWarning("找不到 Player，請確認場景中有物件 Tag = 'Player'");
        }
    }

    public void SwitchState(IEnemyState newState)
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
        
        //Debug.Log($"Seek - Direction: {direction}, Desired: {desired}, Steering: {steering}");
        
        return steering;
    }

    public Vector3 ObstacleAvoid()
    {
        Vector3 avoid = Vector3.zero;
        
        // 調試：檢查 obstacleMask
        if (showDebugInfo)
        {
            Debug.Log($"ObstacleAvoid - obstacleMask: {obstacleMask.value}, avoidDistance: {avoidDistance}");
        }
        
        // 簡單測試：檢查前方是否有任何障礙物
        Ray testRay = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(testRay, out RaycastHit testHit, avoidDistance, obstacleMask))
        {
            if (showDebugInfo)
            {
                Debug.Log($"ObstacleAvoid: 檢測到障礙物 {testHit.collider.name} 在距離 {testHit.distance}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log("ObstacleAvoid: 前方沒有檢測到障礙物");
            }
        }
        
        // 增加射線密度，包含更多檢測方向
        Vector3[] dirs = {
            transform.forward,                                    // 前方
            (transform.forward + transform.right * 0.5f).normalized,     // 右前方30度
            (transform.forward - transform.right * 0.5f).normalized,     // 左前方30度
            (transform.forward + transform.right).normalized,     // 右前方45度
            (transform.forward - transform.right).normalized,     // 左前方45度
            (transform.forward + transform.right * 1.5f).normalized,     // 右前方60度
            (transform.forward - transform.right * 1.5f).normalized,     // 左前方60度
            transform.right,                                      // 右方
            -transform.right,                                     // 左方
        };

        foreach (var dir in dirs)
        {
            // 水平射線檢測
            Ray ray = new Ray(transform.position, dir);
            if (Physics.Raycast(ray, out RaycastHit hit, avoidDistance, obstacleMask))
            {
                Vector3 away = (transform.position - hit.point).normalized;
                away.y = 0f; // 只考慮水平避障
                
                // 根據距離調整避障強度，距離越近強度越大
                float distanceRatio = 1f - (hit.distance / avoidDistance);
                float adjustedStrength = avoidStrength * distanceRatio;
                
                avoid += away * adjustedStrength;
                
                // 調試信息
                if (showDebugInfo)
                {
                    Debug.Log($"Hit obstacle: {hit.collider.name} at distance {hit.distance}, force: {away * adjustedStrength}");
                }
                
                if (showObstacleRays)
                {
                    Debug.DrawRay(transform.position, dir * hit.distance, Color.red);
                }
            }
            else
            {
                // 調試信息 - 沒有碰撞的射線
                if (showObstacleRays)
                {
                    Debug.DrawRay(transform.position, dir * avoidDistance, Color.green);
                }
            }
            
            // 懸空障礙物檢測 - 向上射線
            Vector3 upDir = new Vector3(dir.x, 1f, dir.z).normalized;
            Ray upRay = new Ray(transform.position, upDir);
            if (Physics.Raycast(upRay, out RaycastHit upHit, avoidDistance * 0.8f, obstacleMask))
            {
                Vector3 away = (transform.position - upHit.point).normalized;
                away.y = 0f; // 只考慮水平避障
                
                // 懸空障礙物的避障強度較小
                float distanceRatio = 1f - (upHit.distance / (avoidDistance * 0.8f));
                float adjustedStrength = avoidStrength * distanceRatio * 0.6f; // 懸空障礙物避障強度較小
                
                avoid += away * adjustedStrength;
                
                // 調試信息
                if (showDebugInfo)
                {
                    Debug.Log($"Hit overhead obstacle: {upHit.collider.name} at distance {upHit.distance}, force: {away * adjustedStrength}");
                }
                
                if (showObstacleRays)
                {
                    Debug.DrawRay(transform.position, upDir * upHit.distance, Color.magenta);
                }
            }
            else
            {
                // 調試信息 - 沒有懸空障礙物
                if (showObstacleRays)
                {
                    Debug.DrawRay(transform.position, upDir * (avoidDistance * 0.8f), Color.cyan);
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
            Debug.Log($"ObstacleAvoid - Total avoid force: {avoid}");
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
            
            float distance = directionToEnemy.magnitude;
            if (distance < 0.1f) // 避免除以零
                continue;
                
            // 計算避障力，距離越近力越大
            float avoidForce = enemyAvoidStrength / (distance * distance);
            Vector3 awayFromEnemy = -directionToEnemy.normalized * avoidForce;
            
            avoid += awayFromEnemy;
            
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAvoid - 敵人: {otherEnemy.name}, 距離: {distance:F2}, 避障力: {awayFromEnemy}");
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

    public void Move(Vector3 force, bool lookAtPlayer = false)
    {
        force.y = 0f;
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

        // 移除自動攻擊邏輯，讓狀態機自己控制
    }

    public void Stop()
    {
        velocity = Vector3.zero;
    }

    // 視覺化調試
    void OnDrawGizmosSelected()
    {
        // 繪製敵人避障範圍
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyAvoidDistance);
        
        // 繪製攻擊範圍
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 繪製視覺範圍
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        
        // 繪製障礙物避障範圍
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, avoidDistance);
        
        // 繪製增加密度的避障檢測射線
        Vector3[] dirs = {
            transform.forward,
            (transform.forward + transform.right * 0.5f).normalized,
            (transform.forward - transform.right * 0.5f).normalized,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized,
            (transform.forward + transform.right * 1.5f).normalized,
            (transform.forward - transform.right * 1.5f).normalized,
            transform.right,
            -transform.right
        };
        
        // 水平射線
        Gizmos.color = Color.cyan;
        foreach (var dir in dirs)
        {
            Gizmos.DrawRay(transform.position, dir * avoidDistance);
        }
        
        // 懸空障礙物檢測射線
        Gizmos.color = Color.magenta;
        foreach (var dir in dirs)
        {
            Vector3 upDir = new Vector3(dir.x, 1f, dir.z).normalized;
            Gizmos.DrawRay(transform.position, upDir * (avoidDistance * 0.8f));
        }
        
        // 繪製撤退避障射線
        Vector3[] retreatDirs = {
            -transform.forward,
            (-transform.forward + transform.right).normalized,
            (-transform.forward - transform.right).normalized,
            transform.right,
            -transform.right,
            (-transform.forward + transform.right * 0.5f).normalized,
            (-transform.forward - transform.right * 0.5f).normalized
        };
        
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // 橙色
        foreach (var dir in retreatDirs)
        {
            Gizmos.DrawRay(transform.position, dir * avoidDistance);
        }
    }

    // 測試障礙物檢測的方法（可在 Inspector 中調用）
    [ContextMenu("Test Obstacle Detection")]
    public void TestObstacleDetection()
    {
        Debug.Log("=== 高密度射線障礙物檢測測試 ===");
        Debug.Log($"obstacleMask: {obstacleMask.value}");
        Debug.Log($"avoidDistance: {avoidDistance}");
        
        // 測試前方射線
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, avoidDistance, obstacleMask))
        {
            Debug.Log($"前方射線檢測到: {hit.collider.name}, 距離: {hit.distance}, 層: {hit.collider.gameObject.layer}");
        }
        else
        {
            Debug.Log("前方射線沒有檢測到障礙物");
        }
        
        // 測試右前方30度射線
        Ray rayRight30 = new Ray(transform.position, (transform.forward + transform.right * 0.5f).normalized);
        if (Physics.Raycast(rayRight30, out RaycastHit hitRight30, avoidDistance, obstacleMask))
        {
            Debug.Log($"右前方30度射線檢測到: {hitRight30.collider.name}, 距離: {hitRight30.distance}");
        }
        else
        {
            Debug.Log("右前方30度射線沒有檢測到障礙物");
        }
        
        // 測試左前方30度射線
        Ray rayLeft30 = new Ray(transform.position, (transform.forward - transform.right * 0.5f).normalized);
        if (Physics.Raycast(rayLeft30, out RaycastHit hitLeft30, avoidDistance, obstacleMask))
        {
            Debug.Log($"左前方30度射線檢測到: {hitLeft30.collider.name}, 距離: {hitLeft30.distance}");
        }
        else
        {
            Debug.Log("左前方30度射線沒有檢測到障礙物");
        }
        
        // 測試懸空障礙物 - 前方向上射線
        Vector3 upDir = new Vector3(transform.forward.x, 1f, transform.forward.z).normalized;
        Ray upRay = new Ray(transform.position, upDir);
        if (Physics.Raycast(upRay, out RaycastHit upHit, avoidDistance * 0.8f, obstacleMask))
        {
            Debug.Log($"前方懸空射線檢測到: {upHit.collider.name}, 距離: {upHit.distance}");
        }
        else
        {
            Debug.Log("前方懸空射線沒有檢測到障礙物");
        }
        
        // 測試右方射線
        Ray rayRight = new Ray(transform.position, transform.right);
        if (Physics.Raycast(rayRight, out RaycastHit hitRight, avoidDistance, obstacleMask))
        {
            Debug.Log($"右方射線檢測到: {hitRight.collider.name}, 距離: {hitRight.distance}");
        }
        else
        {
            Debug.Log("右方射線沒有檢測到障礙物");
        }
        
        // 測試左方射線
        Ray rayLeft = new Ray(transform.position, -transform.right);
        if (Physics.Raycast(rayLeft, out RaycastHit hitLeft, avoidDistance, obstacleMask))
        {
            Debug.Log($"左方射線檢測到: {hitLeft.collider.name}, 距離: {hitLeft.distance}");
        }
        else
        {
            Debug.Log("左方射線沒有檢測到障礙物");
        }
    }

    // 測試場景中所有障礙物的方法
    [ContextMenu("Test Scene Obstacles")]
    public void TestSceneObstacles()
    {
        Debug.Log("=== 場景障礙物檢測測試 ===");
        
        // 查找場景中所有有 Collider 的物件
        Collider[] allColliders = FindObjectsOfType<Collider>();
        Debug.Log($"場景中共有 {allColliders.Length} 個 Collider");
        
        foreach (var collider in allColliders)
        {
            Debug.Log($"物件: {collider.name}, Layer: {collider.gameObject.layer}, Tag: {collider.gameObject.tag}");
        }
        
        // 檢查敵人周圍的物件
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, avoidDistance * 2f);
        Debug.Log($"敵人周圍 {avoidDistance * 2f} 距離內有 {nearbyColliders.Length} 個 Collider");
        
        foreach (var collider in nearbyColliders)
        {
            if (collider.gameObject != gameObject) // 排除自己
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                Debug.Log($"附近物件: {collider.name}, Layer: {collider.gameObject.layer}, 距離: {distance:F2}");
            }
        }
    }

    // 測試敵人避障的方法
    [ContextMenu("Test Enemy Avoidance")]
    public void TestEnemyAvoidance()
    {
        Debug.Log("=== 敵人避障測試 ===");
        Debug.Log($"enemyAvoidDistance: {enemyAvoidDistance}");
        Debug.Log($"enemyAvoidStrength: {enemyAvoidStrength}");
        Debug.Log($"enemyLayerMask: {enemyLayerMask.value}");
        
        // 檢測周圍的敵人
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, enemyAvoidDistance, enemyLayerMask);
        Debug.Log($"檢測到 {nearbyEnemies.Length} 個附近的 Collider");
        
        foreach (var enemyCollider in nearbyEnemies)
        {
            if (enemyCollider.gameObject == gameObject)
            {
                Debug.Log($"跳過自己: {enemyCollider.name}");
                continue;
            }
            
            EnemyAI otherEnemy = enemyCollider.GetComponent<EnemyAI>();
            if (otherEnemy == null)
            {
                Debug.Log($"不是敵人: {enemyCollider.name}");
                continue;
            }
            
            float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
            Debug.Log($"敵人: {otherEnemy.name}, 距離: {distance:F2}");
        }
        
        // 測試所有層的檢測
        Collider[] allNearby = Physics.OverlapSphere(transform.position, enemyAvoidDistance);
        Debug.Log($"所有層檢測到 {allNearby.Length} 個 Collider");
        
        foreach (var collider in allNearby)
        {
            if (collider.gameObject != gameObject)
            {
                EnemyAI otherEnemy = collider.GetComponent<EnemyAI>();
                if (otherEnemy != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    Debug.Log($"所有層檢測到敵人: {otherEnemy.name}, Layer: {collider.gameObject.layer}, 距離: {distance:F2}");
                }
            }
        }
    }

    // 簡化的避障方法（備選方案）
    public Vector3 SimpleObstacleAvoid()
    {
        Vector3 avoid = Vector3.zero;
        
        // 檢測前方是否有障礙物
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, avoidDistance))
        {
            // 如果檢測到任何物體（不只是 obstacleMask）
            Vector3 away = (transform.position - hit.point).normalized;
            away.y = 0f;
            avoid = away * avoidStrength;
            
            if (showDebugInfo)
            {
                Debug.Log($"SimpleObstacleAvoid - Hit: {hit.collider.name}, Force: {avoid}");
            }
        }
        
        return avoid;
    }

    // 備用避障方法 - 不依賴 obstacleMask
    public Vector3 BackupObstacleAvoid()
    {
        Vector3 avoid = Vector3.zero;
        
        // 診斷：檢查射線檢測是否正常工作
        Debug.Log($"BackupObstacleAvoid: 開始檢測，位置 = {transform.position}, 前方 = {transform.forward}, 檢測距離 = {avoidDistance}");
        
        // 檢測前方是否有任何障礙物（不限制層）
        Ray ray = new Ray(transform.position, transform.forward);
        
        // 先測試所有層的射線檢測
        if (Physics.Raycast(ray, out RaycastHit hit, avoidDistance))
        {
            Vector3 away = (transform.position - hit.point).normalized;
            away.y = 0f;
            avoid = away * avoidStrength;
            
            Debug.Log($"BackupObstacleAvoid - 檢測到物體: {hit.collider.name}, Layer: {hit.collider.gameObject.layer}, 距離: {hit.distance}, 避障力: {avoid}");
        }
        else
        {
            Debug.Log($"BackupObstacleAvoid - 前方 {avoidDistance} 距離內沒有檢測到任何物體");
            
            // 測試更長的距離
            if (Physics.Raycast(ray, out RaycastHit longHit, avoidDistance * 2f))
            {
                Debug.Log($"BackupObstacleAvoid - 在 {avoidDistance * 2f} 距離內檢測到: {longHit.collider.name}, Layer: {longHit.collider.gameObject.layer}");
            }
            else
            {
                Debug.Log($"BackupObstacleAvoid - 即使在 {avoidDistance * 2f} 距離內也沒有檢測到任何物體");
            }
        }
        
        return avoid;
    }

    // 撤退專用避障方法 - 檢測後方和側面
    public Vector3 RetreatObstacleAvoid()
    {
        Vector3 avoid = Vector3.zero;
        
        // 檢測後方和側面的障礙物
        Vector3[] retreatDirs = {
            -transform.forward,                                   // 後方
            (-transform.forward + transform.right).normalized,    // 右後方
            (-transform.forward - transform.right).normalized,    // 左後方
            transform.right,                                      // 右方
            -transform.right,                                     // 左方
            (-transform.forward + transform.right * 0.5f).normalized,  // 右後方30度
            (-transform.forward - transform.right * 0.5f).normalized   // 左後方30度
        };

        foreach (var dir in retreatDirs)
        {
            Ray ray = new Ray(transform.position, dir);
            if (Physics.Raycast(ray, out RaycastHit hit, avoidDistance, obstacleMask))
            {
                Vector3 away = (transform.position - hit.point).normalized;
                away.y = 0f;
                
                // 根據距離調整避障強度
                float distanceRatio = 1f - (hit.distance / avoidDistance);
                float adjustedStrength = avoidStrength * distanceRatio;
                
                avoid += away * adjustedStrength;
                
                Debug.Log($"RetreatObstacleAvoid - 檢測到障礙物: {hit.collider.name}, 方向: {dir}, 距離: {hit.distance}, 避障力: {away * adjustedStrength}");
            }
            else
            {
                // 如果 obstacleMask 沒有檢測到，嘗試檢測所有層
                if (Physics.Raycast(ray, out RaycastHit hitAll, avoidDistance))
                {
                    Vector3 away = (transform.position - hitAll.point).normalized;
                    away.y = 0f;
                    
                    float distanceRatio = 1f - (hitAll.distance / avoidDistance);
                    float adjustedStrength = avoidStrength * distanceRatio;
                    
                    avoid += away * adjustedStrength;
                    
                    Debug.Log($"RetreatObstacleAvoid - 檢測到物體(所有層): {hitAll.collider.name}, 方向: {dir}, 距離: {hitAll.distance}, 避障力: {away * adjustedStrength}");
                }
            }
        }
        
        // 限制避障力的最大值
        if (avoid.magnitude > maxSpeed)
        {
            avoid = avoid.normalized * maxSpeed;
        }
        
        return avoid;
    }
}