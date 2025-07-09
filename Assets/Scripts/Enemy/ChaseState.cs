using UnityEngine;

public class ChaseState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("Entering ChaseState");
        enemy.animator.Play("Run");
    }

    public void UpdateState(EnemyAI enemy)
    {
        if (enemy.IsInAttackRange())
        {
            Debug.Log("In attack range, switching to AttackState");
            enemy.SwitchState(new AttackState());
            return;
        }

        Vector3 seek = enemy.Seek(enemy.player.position);
        Vector3 avoid = enemy.ObstacleAvoid();
        Vector3 totalForce = seek + avoid;
        
        Debug.Log($"Seek force: {seek}, Avoid force: {avoid}, Total force: {totalForce}, Velocity: {enemy.velocity}");
        
        enemy.Move(totalForce);
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log("Exiting ChaseState");
        enemy.Stop();
    }
}