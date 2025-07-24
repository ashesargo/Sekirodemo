using UnityEngine;

public class BossComboState : IEnemyState
{
    private string currentComboAnimation = "";

    public void EnterState(EnemyAI enemy)
    {
        enemy.SetRootMotion(true);
        Debug.Log("BossComboState: 進入連擊攻擊狀態");
        enemy.Stop();

        BossAI bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null)
        {
            currentComboAnimation = bossAI.GetNextComboAnimation();
            enemy.animator.Play(currentComboAnimation); // 直接播放Combo動畫
        }
        else
        {
            enemy.animator.SetTrigger("Attack");
        }
        enemy.canAutoAttack = false;
    }

    public void UpdateState(EnemyAI enemy)
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        // 攻擊期間持續追蹤玩家
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position, turnSpeed: 8f);
        }
        // Combo動畫結束進入Idle時，隨機決定是否接Jump Attack
        if (stateInfo.IsName("Idle"))
        {
            BossAI bossAI = enemy.GetComponent<BossAI>();
            if (bossAI != null)
            {
                bossAI.IncrementComboCount();
            }
            // 50%機率接Jump Attack
            if (Random.value < 0.5f)
            {
                if (!enemy.IsInAttackRange())
                {
                    // 玩家不在攻擊範圍，先追擊，等追到再進入JumpAttack
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

    public void ExitState(EnemyAI enemy)
    {
        enemy.SetRootMotion(false);
        Debug.Log("BossComboState: Boss退出連擊攻擊狀態");
        enemy.Stop();
    }
} 