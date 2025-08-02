using UnityEngine;

public class ExecutedState : BaseEnemyState
{
    private bool executionAnimationStarted = false;
    private bool isDead = false;
    private float executionStartTime = 0f;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        
        // 重置標記
        executionAnimationStarted = false;
        isDead = false;
        executionStartTime = 0f;
        
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
        
        // 不要設置生命值為0，讓ReviveState來處理復活邏輯
        // 這樣可以避免觸發OnDead事件，讓ExecutedState自己控制狀態轉換
        
        Debug.Log($"[ExecutedState] 敵人 {enemy.name} 進入被處決狀態");
    }

    public override void UpdateState(EnemyAI enemy)
    {
        // 播放被處決動畫（只播放一次）
        if (!executionAnimationStarted)
        {
            enemy.animator.SetTrigger("Executed");
            executionAnimationStarted = true;
            executionStartTime = Time.time;
            Debug.Log($"[ExecutedState] 敵人 {enemy.name} 播放被處決動畫");
        }
        
        // 檢查動畫是否播放完畢
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        
        // 調試信息
        if (executionAnimationStarted && !isDead)
        {
            Debug.Log($"[ExecutedState] 當前動畫: {stateInfo.fullPathHash}, 標籤: {stateInfo.tagHash}, 播放進度: {stateInfo.normalizedTime:F2}");
        }
        
        // 檢查多種可能的動畫結束條件
        bool animEnd = false;
        
        // 檢查Executed標籤動畫是否播放完畢
        if (stateInfo.IsTag("Executed") && stateInfo.normalizedTime >= 0.9f)
        {
            animEnd = true;
            Debug.Log($"[ExecutedState] Executed標籤動畫播放完畢");
        }
        // 檢查是否有Executed名稱的動畫播放完畢
        else if (stateInfo.IsName("Executed") && stateInfo.normalizedTime >= 0.9f)
        {
            animEnd = true;
            Debug.Log($"[ExecutedState] Executed名稱動畫播放完畢");
        }
        // 檢查是否有Death標籤的動畫播放完畢
        else if (stateInfo.IsTag("Death") && stateInfo.normalizedTime >= 0.9f)
        {
            animEnd = true;
            Debug.Log($"[ExecutedState] Death標籤動畫播放完畢");
        }
        // 超時保護（避免卡死）
        else if (Time.time - executionStartTime > 5f)
        {
            animEnd = true;
            Debug.Log($"[ExecutedState] 動畫播放超時，強制結束");
        }
        
        if (animEnd && !isDead)
        {
            // 動畫播放完畢，檢查是否還有復活次數
            isDead = true;
            
            // 檢查復活次數
            HealthPostureController healthController = enemy.GetComponent<HealthPostureController>();
            if (healthController != null && healthController.live > 0)
            {
                // 還有復活次數，進入復活狀態
                Debug.Log($"[ExecutedState] 敵人 {enemy.name} 還有復活次數 ({healthController.live})，進入復活狀態");
                enemy.SwitchState(new ReviveState());
            }
            else
            {
                // 沒有復活次數，被處決的敵人需要正確處理死亡流程
                Debug.Log($"[ExecutedState] 敵人 {enemy.name} 沒有復活次數，準備進入ExecutedDieState");
                
                // 不要設置生命值為0，避免觸發OnDead事件
                // 讓ExecutedDieState自己處理死亡邏輯
                Debug.Log($"[ExecutedState] 敵人 {enemy.name} 沒有復活次數，被處決死亡，切換到ExecutedDieState");
                
                // 進入ExecutedDieState但跳過死亡動畫，直接處理死亡邏輯
                enemy.SwitchState(new ExecutedDieState());
            }
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