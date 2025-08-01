using UnityEngine;

public class DangerousAttackHandler : MonoBehaviour
{
    [Header("危攻擊設定")]
    public float dangerousAttackDamage = 30f; // 危攻擊傷害
    public float dangerousAttackRange = 2.5f; // 危攻擊範圍
    public LayerMask playerLayer = 1 << 7; // Player層 (Layer 7)
    
    [Header("特效設定")]
    public GameObject dangerousAttackEffect; // 危攻擊特效
    public Transform attackPoint; // 攻擊點位置
    
    private EnemyAI enemyAI;
    private DangerousAttackState dangerousAttackState;
    private bool isDangerousAttackActive = false;

    void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        if (attackPoint == null)
        {
            // 如果沒有指定攻擊點，使用敵人前方位置
            attackPoint = transform;
        }
    }

    void Update()
    {
        // 檢查是否正在進行危攻擊
        if (enemyAI != null && enemyAI.CurrentState is DangerousAttackState)
        {
            dangerousAttackState = (DangerousAttackState)enemyAI.CurrentState;
            isDangerousAttackActive = dangerousAttackState.IsDangerousAttackActive();
        }
        else
        {
            isDangerousAttackActive = false;
        }
    }

    // 動畫事件調用：開始危攻擊
    public void OnDangerousAttackStart()
    {
        isDangerousAttackActive = true;
        Debug.Log("DangerousAttackHandler: 危攻擊開始！");
        
        // 播放危攻擊特效
        if (dangerousAttackEffect != null)
        {
            Vector3 effectPosition = attackPoint != null ? attackPoint.position : transform.position + transform.forward * 2f;
            Instantiate(dangerousAttackEffect, effectPosition, transform.rotation);
        }
    }

    // 動畫事件調用：危攻擊傷害判定
    public void OnDangerousAttackDamage()
    {
        if (!isDangerousAttackActive) return;
        
        Debug.Log("DangerousAttackHandler: 執行危攻擊傷害判定");
        
        // 檢測範圍內的玩家
        Collider[] hits = Physics.OverlapSphere(transform.position, dangerousAttackRange, playerLayer);
        
        foreach (var hit in hits)
        {
            // 檢查是否在攻擊角度內
            Vector3 dirToPlayer = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            
            if (angle <= 120f) // 120度攻擊角度
            {
                // 對玩家造成危攻擊傷害
                PlayerStatus playerStatus = hit.GetComponent<PlayerStatus>();
                if (playerStatus != null)
                {
                    Debug.Log($"DangerousAttackHandler: 對玩家造成危攻擊傷害 {dangerousAttackDamage}");
                    playerStatus.TakeDangerousDamage(dangerousAttackDamage);
                }
            }
        }
    }

    // 動畫事件調用：結束危攻擊
    public void OnDangerousAttackEnd()
    {
        isDangerousAttackActive = false;
        Debug.Log("DangerousAttackHandler: 危攻擊結束");
    }

    // 檢查是否正在進行危攻擊
    public bool IsDangerousAttackActive()
    {
        return isDangerousAttackActive;
    }

    // 在Scene視圖中顯示攻擊範圍（僅在選中時顯示）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerousAttackRange);
        
        // 顯示攻擊角度
        Vector3 forward = transform.forward;
        Vector3 left = Quaternion.Euler(0, -60, 0) * forward;
        Vector3 right = Quaternion.Euler(0, 60, 0) * forward;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, left * dangerousAttackRange);
        Gizmos.DrawRay(transform.position, right * dangerousAttackRange);
    }
} 