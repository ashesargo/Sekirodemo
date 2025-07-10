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
        enemy.canAutoAttack = false;
        // 移除 dodgeDir 隨機邏輯
    }

    public void UpdateState(EnemyAI enemy)
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        if (!hasAttacked && stateInfo.IsName("Attack"))
        {
            hasAttacked = true;
        }

        // 等待攻擊動畫結束
        if (hasAttacked && stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 1.0f)
        {
            enemy.SwitchState(new RetreatState());
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        enemy.Stop();
    }
}