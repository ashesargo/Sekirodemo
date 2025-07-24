using UnityEngine;

public class DieState : BaseEnemyState
{
    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        enemy.animator.SetTrigger("Death");
        enemy.Stop();
    }

    public override void UpdateState(EnemyAI enemy)
    {
        // 死亡狀態不做任何事，等待物件池回收
    }

    public override void ExitState(EnemyAI enemy) { base.ExitState(enemy); }
    public override bool ShouldUseRootMotion() => true;
} 