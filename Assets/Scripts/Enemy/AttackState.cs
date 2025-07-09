using UnityEngine;

public class AttackState : IEnemyState
{
    private float moveTimer = 0f;
    private float cooldownTime = 2f;
    private Vector3 dodgeDir;
    private bool hasAttacked = false;

    public void EnterState(EnemyAI enemy)
    {
        enemy.Stop();
        enemy.animator.SetTrigger("Attack");
        hasAttacked = false;
        moveTimer = 0f;
        dodgeDir = Random.value > 0.5f ? enemy.transform.right : -enemy.transform.right;
    }

    public void UpdateState(EnemyAI enemy)
    {
        if (!hasAttacked && enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            hasAttacked = true;
        }

        if (hasAttacked)
        {
            moveTimer += Time.deltaTime;
            enemy.Move(dodgeDir); // simple dodge move
            if (moveTimer >= cooldownTime)
            {
                enemy.SwitchState(new ChaseState());
            }
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        enemy.Stop();
    }
}