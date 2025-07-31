using UnityEngine;

public class ExecutedState : BaseEnemyState
{
    private bool executionAnimationStarted = false;
    private bool isDead = false;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        
        // 重置標記
        executionAnimationStarted = false;
        isDead = false;
        
        // 停止移動
        enemy.Stop();
        
        // 禁用所有能力
        enemy.canBeParried = false;
        enemy.canAutoAttack = false;
        
        // 清除失衡標記（允許處決）
        EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.isStaggered = false;
            enemyTest.isDead = true;
        }
        
        // 設置生命值為0（確保死亡）
        HealthPostureController healthController = enemy.GetComponent<HealthPostureController>();
        if (healthController != null)
        {
            healthController.SetHealthValue(0f);
        }
        
        Debug.Log($"[ExecutedState] 敵人 {enemy.name} 進入被處決狀態");
    }

    public override void UpdateState(EnemyAI enemy)
    {
        // 播放被處決動畫（只播放一次）
        if (!executionAnimationStarted)
        {
            enemy.animator.SetTrigger("Executed");
            executionAnimationStarted = true;
            Debug.Log($"[ExecutedState] 敵人 {enemy.name} 播放被處決動畫");
        }
        
        // 檢查動畫是否播放完畢
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsTag("Executed") && stateInfo.normalizedTime >= 0.9f && !isDead)
        {
            // 動畫播放完畢，進入死亡狀態
            isDead = true;
            enemy.SwitchState(new DieState());
            Debug.Log($"[ExecutedState] 敵人 {enemy.name} 被處決動畫播放完畢，進入死亡狀態");
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        
        // 確保動畫參數被重置
        enemy.animator.ResetTrigger("Executed");
        
        Debug.Log($"[ExecutedState] 敵人 {enemy.name} 退出被處決狀態");
    }

    public override bool ShouldUseRootMotion() => true;
} 