using UnityEngine;

public class BossRushState : BaseEnemyState
{
    private Vector3 rushDirection;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("BossRushState: 進入突進狀態");
        enemy.animator.Play("Rush");
        enemy.Stop();
        var bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null)
            bossAI.hasJustRangedOrRushed = true;
        if (enemy.player != null)
        {
            rushDirection = (enemy.player.position - enemy.transform.position).normalized;
            rushDirection.y = 0f;
        }
        else
        {
            rushDirection = enemy.transform.forward;
        }
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
            enemy.SwitchState(new BossChaseState());
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log("BossRushState: 退出突進狀態");
        enemy.Stop();
    }
    public override bool ShouldUseRootMotion() => true;
} 