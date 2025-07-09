using UnityEngine;

public class AlertState : IEnemyState
{
    private float alertTime = 1.5f;
    private float timer = 0f;

    public void EnterState(EnemyAI enemy)
    {
        enemy.animator.Play("Alert");
        enemy.Stop();
        timer = 0f;
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        if (timer > alertTime)
        {
            if (enemy.IsInAttackRange())
                enemy.SwitchState(new AttackState());
            else
                enemy.SwitchState(new ChaseState());
        }
    }

    public void ExitState(EnemyAI enemy) { }
}