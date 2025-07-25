using UnityEngine;

public class AttackState : BaseEnemyState
{
    private float moveTimer = 0f;
    private float cooldownTime = 2f;
    private Vector3 dodgeDir;
    private bool hasAttacked = false;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("AttackState: 進入攻擊狀態");
        enemy.Stop(); // 進入攻擊時完全靜止
        enemy.animator.ResetTrigger("Attack"); // 先重置Trigger
        enemy.animator.SetTrigger("Attack");   // 再設置Trigger
        hasAttacked = false;
        moveTimer = 0f;
        enemy.canAutoAttack = false;
    }

    public override void UpdateState(EnemyAI enemy)
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        if (!hasAttacked && stateInfo.IsName("Attack"))
        {
            hasAttacked = true;
            Debug.Log("AttackState: 攻擊動畫開始播放");
            enemy.Stop(); // 再次保險，攻擊動畫開始時也停止
        }
        // 等待攻擊動畫結束
        if (hasAttacked && stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 1.0f)
        {
            Debug.Log("AttackState: 攻擊動畫結束，切換到 RetreatState");
            enemy.SwitchState(new RetreatState()); // 攻擊後自動撤退
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log("AttackState: 退出攻擊狀態");
        enemy.Stop();
    }
    public override bool ShouldUseRootMotion() => true;
}