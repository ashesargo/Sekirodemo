using UnityEngine;

public class BossIdleState : BaseEnemyState
{
    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        enemy.animator.Play("Idle");
        enemy.Stop();
        enemy.canAutoAttack = true;
        Debug.Log("BossIdleState: Boss進入待機狀態");
    }

    public override void UpdateState(EnemyAI enemy)
    {
        if (enemy.player == null)
        {
            Debug.LogWarning("Player is null! Check if Player GameObject has Tag = 'Player'");
            return;
        }
        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
        bool canSee = enemy.CanSeePlayer();
        if (canSee)
        {
            var bossAI = enemy.GetComponent<BossAI>();
            if (bossAI != null && !bossAI.hasDoneIntroCombo)
            {
                if (distance < enemy.attackRange)
                {
                    Debug.Log("BossIdleState: Boss第一次靠近玩家，進入開場Combo");
                    enemy.SwitchState(new BossIntroComboState());
                }
                else
                {
                    Debug.Log("BossIdleState: Boss發現玩家，先追擊");
                    bossAI.pendingIntroComboStep = -1;
                    bossAI.pendingJumpAttack = false;
                    enemy.SwitchState(new BossChaseState());
                }
            }
            else
            {
                Debug.Log("BossIdleState: Boss發現玩家，直接進入追擊狀態");
                enemy.SwitchState(new BossChaseState());
            }
        }
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position);
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log("BossIdleState: Boss退出待機狀態");
    }
    public override bool ShouldUseRootMotion() => true;
} 