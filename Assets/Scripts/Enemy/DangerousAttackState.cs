using UnityEngine;

public class DangerousAttackState : BaseEnemyState
{
    private float moveTimer = 0f;
    private float cooldownTime = 3f; // 危攻擊冷卻時間更長
    private bool hasAttacked = false;
    private bool isDangerousAttackActive = false; // 追蹤危攻擊是否正在進行

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("DangerousAttackState: 進入危攻擊狀態");
        enemy.Stop(); // 進入攻擊時完全靜止
        enemy.animator.ResetTrigger("DangerousAttack"); // 先重置Trigger
        enemy.animator.SetTrigger("DangerousAttack");   // 再設置Trigger
        hasAttacked = false;
        isDangerousAttackActive = false;
        moveTimer = 0f;
        enemy.canAutoAttack = false;
    }

    public override void UpdateState(EnemyAI enemy)
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        
        // 檢查是否正在播放危攻擊動畫
        if (!hasAttacked && stateInfo.IsName("DangerousAttack"))
        {
            hasAttacked = true;
            Debug.Log("DangerousAttackState: 危攻擊動畫開始播放");
            enemy.Stop(); // 再次保險，攻擊動畫開始時也停止
        }
        
        // 等待攻擊動畫結束
        if (hasAttacked && stateInfo.IsName("DangerousAttack") && stateInfo.normalizedTime >= 1.0f)
        {
            Debug.Log("DangerousAttackState: 危攻擊動畫結束，切換到 RetreatState");
            enemy.SwitchState(new RetreatState()); // 攻擊後自動撤退
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log("DangerousAttackState: 退出危攻擊狀態");
        enemy.Stop();
        isDangerousAttackActive = false;
    }

    public override bool ShouldUseRootMotion() => true;

    // 動畫事件調用：開始危攻擊
    public void OnDangerousAttackStart()
    {
        isDangerousAttackActive = true;
        Debug.Log("DangerousAttackState: 危攻擊開始，無視防禦！");
    }

    // 動畫事件調用：結束危攻擊
    public void OnDangerousAttackEnd()
    {
        isDangerousAttackActive = false;
        Debug.Log("DangerousAttackState: 危攻擊結束");
    }

    // 檢查是否正在進行危攻擊
    public bool IsDangerousAttackActive()
    {
        return isDangerousAttackActive;
    }
} 