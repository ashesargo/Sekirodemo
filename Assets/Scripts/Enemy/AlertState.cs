using UnityEngine;

public class AlertState : BaseEnemyState
{
    private float alertTime = 1.5f;
    private float timer = 0f;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        enemy.animator.Play("Alert");
        enemy.Stop();
        timer = 0f;
        enemy.canAutoAttack = true;
    }

    public override void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position);
        }
        if (timer > alertTime)
        {
            if (enemy.IsInAttackRange())
            {
                var rangedEnemy = enemy.GetComponent<RangedEnemy>();
                if (rangedEnemy != null)
                {
                    enemy.SwitchState(new AimState());
                }
                else
                {
                    enemy.SwitchState(new AttackState());
                }
            }
            else
                enemy.SwitchState(new ChaseState());
        }
    }

    public override void ExitState(EnemyAI enemy) { base.ExitState(enemy); }
    public override bool ShouldUseRootMotion() => true;
}