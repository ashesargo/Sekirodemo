using UnityEngine;

public class DieState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        enemy.animator.SetTrigger("Death");
        enemy.Stop(); // 停止移動
    }

    public void UpdateState(EnemyAI enemy)
    {
        // 死亡狀態不做任何事，等待物件池回收
    }

    public void ExitState(EnemyAI enemy) { }
} 