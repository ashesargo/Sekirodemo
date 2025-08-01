using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    [Header("危攻擊設定")]
    public float dangerousAttackDamage = 30f;
    public float dangerousAttackRange = 2.5f;
    public LayerMask playerLayer = 1 << 7; // Player層
    public Transform attackPoint;
    
    [Header("特效設定")]
    public GameObject dangerousAttackEffect;
    public GameObject normalAttackEffect;
    
    private EnemyAI enemyAI;
    private DangerousAttackHandler dangerousAttackHandler;
    private BossDangerousAttackState bossDangerousAttackState;

    void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        dangerousAttackHandler = GetComponent<DangerousAttackHandler>();
        
        if (attackPoint == null)
        {
            attackPoint = transform;
        }
    }

    // === 危攻擊動畫事件 ===
    
    // 開始危攻擊
    public void OnDangerousAttackStart()
    {
        Debug.Log("AnimationEventHandler: 危攻擊開始");
        
        // 通知DangerousAttackHandler
        if (dangerousAttackHandler != null)
        {
            dangerousAttackHandler.OnDangerousAttackStart();
        }
        
        // 通知BossDangerousAttackState
        if (enemyAI != null && enemyAI.CurrentState is BossDangerousAttackState)
        {
            bossDangerousAttackState = (BossDangerousAttackState)enemyAI.CurrentState;
            bossDangerousAttackState.OnBossDangerousAttackStart();
        }
        
        // 播放危攻擊特效
        if (dangerousAttackEffect != null)
        {
            Vector3 effectPosition = attackPoint.position + attackPoint.forward * 2f;
            Instantiate(dangerousAttackEffect, effectPosition, attackPoint.rotation);
        }
    }

    // 危攻擊傷害判定
    public void OnDangerousAttackDamage()
    {
        Debug.Log("AnimationEventHandler: 執行危攻擊傷害判定");
        
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
                    Debug.Log($"AnimationEventHandler: 對玩家造成危攻擊傷害 {dangerousAttackDamage}");
                    playerStatus.TakeDangerousDamage(dangerousAttackDamage);
                }
            }
        }
    }

    // 結束危攻擊
    public void OnDangerousAttackEnd()
    {
        Debug.Log("AnimationEventHandler: 危攻擊結束");
        
        // 通知DangerousAttackHandler
        if (dangerousAttackHandler != null)
        {
            dangerousAttackHandler.OnDangerousAttackEnd();
        }
        
        // 通知BossDangerousAttackState
        if (enemyAI != null && enemyAI.CurrentState is BossDangerousAttackState)
        {
            bossDangerousAttackState = (BossDangerousAttackState)enemyAI.CurrentState;
            bossDangerousAttackState.OnBossDangerousAttackEnd();
        }
    }

    // === 一般攻擊動畫事件 ===
    
    // 開始一般攻擊
    public void OnAttackStart()
    {
        Debug.Log("AnimationEventHandler: 一般攻擊開始");
        
        // 播放一般攻擊特效
        if (normalAttackEffect != null)
        {
            Vector3 effectPosition = attackPoint.position + attackPoint.forward * 1.5f;
            Instantiate(normalAttackEffect, effectPosition, attackPoint.rotation);
        }
    }

    // 一般攻擊傷害判定
    public void OnAttackDamage()
    {
        Debug.Log("AnimationEventHandler: 執行一般攻擊傷害判定");
        
        // 這裡可以添加一般攻擊的傷害邏輯
        // 一般攻擊可以被防禦，所以使用正常的傷害處理
    }

    // 結束一般攻擊
    public void OnAttackEnd()
    {
        Debug.Log("AnimationEventHandler: 一般攻擊結束");
    }

    // === 其他動畫事件 ===
    
    // 腳步聲
    public void OnFootstep()
    {
        // 可以在這裡播放腳步聲
        Debug.Log("AnimationEventHandler: 播放腳步聲");
    }

    // 武器揮舞聲
    public void OnWeaponSwing()
    {
        // 可以在這裡播放武器揮舞聲
        Debug.Log("AnimationEventHandler: 播放武器揮舞聲");
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