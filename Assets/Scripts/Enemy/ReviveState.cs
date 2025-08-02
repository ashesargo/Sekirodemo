using UnityEngine;

public class ReviveState : BaseEnemyState
{
    private bool reviveAnimationStarted = false;
    private bool reviveCompleted = false;
    private float reviveTimer = 0f;
    private float maxReviveTime = 3f; // 最大復活時間，避免卡死

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        
        // 重置標記
        reviveAnimationStarted = false;
        reviveCompleted = false;
        reviveTimer = 0f;
        
        // 停止移動
        enemy.Stop();
        
        // 禁用所有能力
        enemy.canBeParried = false;
        enemy.canAutoAttack = false;
        
        Debug.Log($"[ReviveState] 敵人 {enemy.name} 進入復活狀態");
    }

    public override void UpdateState(EnemyAI enemy)
    {
        reviveTimer += Time.deltaTime;
        
        // 播放復活動畫（只播放一次）
        if (!reviveAnimationStarted)
        {
            // 檢查是否有Revive動畫狀態
            bool hasReviveState = enemy.animator.HasState(0, Animator.StringToHash("Revive"));
            Debug.Log($"[ReviveState] 檢查Revive動畫狀態: {hasReviveState}");
            
            if (hasReviveState)
            {
                enemy.animator.SetTrigger("Revive");
                Debug.Log($"[ReviveState] 敵人 {enemy.name} 播放Revive動畫");
            }
            else
            {
                // 如果沒有Revive動畫，使用Idle動畫
                enemy.animator.Play("Idle");
                Debug.Log($"[ReviveState] 敵人 {enemy.name} 使用Idle動畫作為復活動畫");
            }
            reviveAnimationStarted = true;
        }
        
        // 檢查動畫是否播放完畢或超時
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        bool animEnd = false;
        
        // 檢查Revive動畫是否播放完畢
        if (stateInfo.IsName("Revive") && stateInfo.normalizedTime >= 0.9f)
        {
            animEnd = true;
            Debug.Log($"[ReviveState] Revive動畫播放完畢");
        }
        // 檢查是否有Revive標籤的動畫
        else if (stateInfo.IsTag("Revive") && stateInfo.normalizedTime >= 0.9f)
        {
            animEnd = true;
            Debug.Log($"[ReviveState] Revive標籤動畫播放完畢");
        }
        // 檢查Idle動畫是否播放到一半
        else if (stateInfo.IsName("Idle") && stateInfo.normalizedTime >= 0.5f)
        {
            animEnd = true;
            Debug.Log($"[ReviveState] Idle動畫播放到一半，結束復活");
        }
        // 檢查是否超時
        else if (reviveTimer > maxReviveTime)
        {
            animEnd = true;
            Debug.Log($"[ReviveState] 復活超時，強制結束");
        }
        
        // 調試信息
        if (reviveTimer > 0.1f && !animEnd)
        {
            Debug.Log($"[ReviveState] 當前動畫: {stateInfo.fullPathHash}, 播放進度: {stateInfo.normalizedTime:F2}, 時間: {reviveTimer:F2}");
        }
        
        if (animEnd && !reviveCompleted)
        {
            reviveCompleted = true;
            
            // 執行復活邏輯
            HealthPostureController healthController = enemy.GetComponent<HealthPostureController>();
            if (healthController != null)
            {
                // 減少復活次數
                healthController.live--;
                
                // 重置生命值和架勢值
                healthController.ResetHealth();
                healthController.ResetPosture();
                healthController.canIncreasePosture = true;
                
                // 確保敵人不是死亡狀態
                EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
                if (enemyTest != null)
                {
                    enemyTest.isDead = false;
                    enemyTest.isStaggered = false;
                }
                
                // 重置敵人組件
                ResetEnemyComponents(enemy);
                
                // 播放復活特效
                healthController.PlayReviveEffect();
                
                // 更新UI
                if (healthController.healthPostureUI != null)
                {
                    healthController.healthPostureUI.UpdateLifeBalls(healthController.live, healthController.maxLive);
                }
                
                Debug.Log($"[ReviveState] 敵人 {enemy.name} 復活完成，剩餘復活次數: {healthController.live}");
            }
            
            // 復活完成後，根據敵人類型切換到適當狀態
            BossAI bossAI = enemy.GetComponent<BossAI>();
            if (bossAI != null)
            {
                // Boss復活後進入追擊狀態
                enemy.SwitchState(new BossChaseState());
                Debug.Log($"[ReviveState] Boss {enemy.name} 復活後進入追擊狀態");
            }
            else
            {
                // 一般敵人復活後檢查是否能看到玩家
                if (enemy.CanSeePlayer())
                {
                    enemy.SwitchState(new ChaseState());
                    Debug.Log($"[ReviveState] 敵人 {enemy.name} 復活後進入追擊狀態");
                }
                else
                {
                    enemy.SwitchState(new AlertState());
                    Debug.Log($"[ReviveState] 敵人 {enemy.name} 復活後進入警戒狀態");
                }
            }
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        
        // 確保動畫參數被重置
        enemy.animator.ResetTrigger("Revive");
        
        // 重置其他可能影響的動畫參數
        enemy.animator.ResetTrigger("Executed");
        enemy.animator.ResetTrigger("Death");
        
        Debug.Log($"[ReviveState] 敵人 {enemy.name} 退出復活狀態");
    }

    // 重置敵人組件
    private void ResetEnemyComponents(EnemyAI enemy)
    {
        // 重新啟用AI組件（如果被禁用）
        if (!enemy.enabled)
        {
            enemy.enabled = true;
        }
        
        // 重置敵人測試組件
        EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.RestoreControl();
        }
        
        // 重新啟用能力
        enemy.canAutoAttack = true;
        enemy.canBeParried = true;
        
        // 重新啟用碰撞器
        Collider enemyCollider = enemy.GetComponent<Collider>();
        if (enemyCollider != null && !enemyCollider.enabled)
        {
            enemyCollider.enabled = true;
        }
        
        // 重新啟用CharacterController
        CharacterController characterController = enemy.GetComponent<CharacterController>();
        if (characterController != null && !characterController.enabled)
        {
            characterController.enabled = true;
        }
        
        // 確保位置正常
        if (characterController != null)
        {
            characterController.Move(Vector3.zero);
        }
        
        if (enemy.transform.position.y < -10f)
        {
            enemy.transform.position = new Vector3(enemy.transform.position.x, 1f, enemy.transform.position.z);
        }
    }

    public override bool ShouldUseRootMotion() => true;
} 