using UnityEngine;

public class BossRangedAttackState : IEnemyState
{
    private bool hasAttacked = false;
    private float timer = 0f;
    private float maxAttackTime = 2.0f; // 遠程攻擊動畫最大等待時間

    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("BossRangedAttackState: 進入遠程攻擊狀態");
        enemy.animator.SetTrigger("RangedAttack"); // Animator需有RangedAttack Trigger
        hasAttacked = false;
        timer = 0f;
        enemy.Stop();
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        // 動畫開始時觸發遠程攻擊
        if (!hasAttacked && stateInfo.IsName("RangedAttack") && stateInfo.normalizedTime > 0.1f)
        {
            hasAttacked = true;
            // 這裡可呼叫Boss專屬遠程攻擊方法
            var bossRanged = enemy.GetComponent<BossRangedAttack>();
            if (bossRanged != null)
            {
                bossRanged.FireBossProjectile();
            }
        }
        // 動畫結束或超時，切回追擊
        if ((hasAttacked && stateInfo.normalizedTime >= 1.0f) || timer > maxAttackTime)
        {
            enemy.SwitchState(new BossChaseState());
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log("BossRangedAttackState: 退出遠程攻擊狀態");
        enemy.Stop();
    }
} 