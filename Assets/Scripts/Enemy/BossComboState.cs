using UnityEngine;

public class BossComboState : BaseEnemyState
{
    private string currentComboAnimation = "";

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("BossComboState: 進入連擊攻擊狀態");
        enemy.Stop();
        BossAI bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null)
        {
            currentComboAnimation = bossAI.GetNextComboAnimation();
            enemy.animator.Play(currentComboAnimation);
        }
        else
        {
            enemy.animator.SetTrigger("Attack");
        }
        enemy.canAutoAttack = false;
    }

    public override void UpdateState(EnemyAI enemy)
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position, turnSpeed: 8f);
        }
        if (stateInfo.IsName("Idle"))
        {
            BossAI bossAI = enemy.GetComponent<BossAI>();
            if (bossAI != null)
            {
                bossAI.IncrementComboCount();
                
                // 檢查是否應該觸發危攻擊
                if (bossAI.ShouldTriggerDangerousAttack())
                {
                    Debug.Log("BossComboState: 觸發危攻擊！");
                    
                    // 如果使用危攻擊轉換器，則啟用它
                    if (bossAI.useDangerousAttackConverter)
                    {
                        bossAI.EnableDangerousAttack();
                    }
                    else
                    {
                        bossAI.PerformDangerousAttack();
                    }
                    return;
                }
            }
            
            // 原有的邏輯
            if (Random.value < 0.5f)
            {
                if (!enemy.IsInAttackRange())
                {
                    if (bossAI != null)
                        bossAI.pendingJumpAttack = true;
                    enemy.SwitchState(new BossChaseState());
                }
                else
                {
                    enemy.SwitchState(new BossJumpAttackState());
                }
            }
            else
            {
                enemy.SwitchState(new RetreatState());
            }
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log("BossComboState: Boss退出連擊攻擊狀態");
        enemy.Stop();
    }
    public override bool ShouldUseRootMotion() => true;
} 