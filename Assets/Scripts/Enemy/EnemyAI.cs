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

    private IEnemyState currentState;
    [HideInInspector] public Vector3 velocity;

    void Start()
    {
        animator = GetComponent<Animator>();
        FindPlayer();
        SwitchState(new IdleState());
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
            Debug.Log("Player found: " + player.name);
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
        
        Debug.Log($"Seek - Direction: {direction}, Desired: {desired}, Steering: {steering}");
        
        return steering;
    }

    public Vector3 ObstacleAvoid()
    {
        Vector3 avoid = Vector3.zero;
        Vector3[] dirs = {
            transform.forward,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized
        };

        foreach (var dir in dirs)
        {
            Ray ray = new Ray(transform.position, dir);
            if (Physics.Raycast(ray, out RaycastHit hit, avoidDistance, obstacleMask))
            {
                Vector3 away = (transform.position - hit.point).normalized;
                away.y = 0f; // 只考慮水平避障
                avoid += away * avoidStrength;
            }
        }
        
        // 限制避障力的最大值
        if (avoid.magnitude > maxSpeed)
        {
            avoid = avoid.normalized * maxSpeed;
        }
        
        Debug.Log($"ObstacleAvoid - Avoid force: {avoid}");
        
        return avoid;
    }

    public void Move(Vector3 force)
    {
        // 限制 Y 軸移動，只允許水平移動
        force.y = 0f;
        
        velocity += force * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        
        // 確保 Y 軸速度為 0，防止浮空
        velocity.y = 0f;
        
        transform.position += velocity * Time.deltaTime;
        
        // 只有當速度足夠大時才改變朝向
        if (velocity.magnitude > 0.1f)
        {
            Vector3 lookDirection = velocity.normalized;
            lookDirection.y = 0f; // 確保朝向也是水平方向
            if (lookDirection.magnitude > 0.1f)
            {
                transform.forward = lookDirection;
            }
        }

        if (IsInAttackRange())
        {
            velocity = Vector3.zero;
            animator.Play("Attack"); // 進入攻擊範圍時自動切換 Attack 動畫
        }
    }

    public void Stop()
    {
        velocity = Vector3.zero;
    }
}