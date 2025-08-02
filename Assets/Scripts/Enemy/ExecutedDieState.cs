using UnityEngine;

public class ExecutedDieState : BaseEnemyState
{
    private bool deathProcessStarted = false;
    private float deathTimer = 0f;
    private float deathProcessDuration = 0.5f; // 死亡處理時間

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        
        // 重置標記
        deathProcessStarted = false;
        deathTimer = 0f;
        
        // 停止移動
        enemy.Stop();
        
        // 禁用所有能力
        enemy.canBeParried = false;
        enemy.canAutoAttack = false;
        
        // 設置死亡標記
        EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.isDead = true;
        }
        
        // 不要在這裡禁用AI組件，讓UpdateState處理完死亡邏輯後再禁用
        
        Debug.Log($"[ExecutedDieState] 敵人 {enemy.name} 進入被處決死亡狀態");
    }

    public override void UpdateState(EnemyAI enemy)
    {
        if (!deathProcessStarted)
        {
            deathProcessStarted = true;
            
            // 處理死亡邏輯（UI、音樂等）
            HealthPostureController healthController = enemy.GetComponent<HealthPostureController>();
            if (healthController != null)
            {
                Debug.Log($"[ExecutedDieState] 處理敵人 {enemy.name} 的死亡邏輯");
                
                // 不要設置生命值為0，避免觸發OnDead事件
                // 直接處理死亡邏輯
                
                // 隱藏血條
                healthController.HideHealthBar();
                Debug.Log($"[ExecutedDieState] 已隱藏血條");
                
                // 如果是Boss或Elite，通知死亡
                if (healthController.IsBoss())
                {
                    Debug.Log($"[ExecutedDieState] 敵人 {enemy.name} 是Boss/Elite，通知死亡");
                    // 立即通知Boss死亡，因為GameObject可能已被禁用
                    healthController.NotifyBossDeath();
                    Debug.Log($"[ExecutedDieState] 已調用NotifyBossDeath");
                }
                else
                {
                    Debug.Log($"[ExecutedDieState] 敵人 {enemy.name} 是一般敵人，IsBoss()返回false");
                    // 對於一般敵人，確保血條隱藏
                    if (healthController.healthPostureUI != null)
                    {
                        healthController.healthPostureUI.gameObject.SetActive(false);
                        Debug.Log($"[ExecutedDieState] 已隱藏一般敵人血條UI");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[ExecutedDieState] 敵人 {enemy.name} 沒有HealthPostureController組件");
            }
            
            // 處理完死亡邏輯後，禁用組件
            Debug.Log($"[ExecutedDieState] 禁用敵人 {enemy.name} 的組件");
            
            // 禁用AI組件
            enemy.enabled = false;
            
            // 禁用碰撞器
            Collider enemyCollider = enemy.GetComponent<Collider>();
            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }
            
            // 禁用CharacterController
            CharacterController characterController = enemy.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }
        }
        
        deathTimer += Time.deltaTime;
        
        // 等待一小段時間讓UI和音樂處理完成，然後回歸物件池
        if (deathTimer >= deathProcessDuration)
        {
            // 回歸物件池
            ObjectPool objectPool = ObjectPool.Instance;
            if (objectPool != null)
            {
                objectPool.ReturnPooledObject(enemy.gameObject);
                Debug.Log($"[ExecutedDieState] 敵人 {enemy.name} 被處決死亡，回歸物件池");
            }
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        
        // 確保動畫參數被重置
        enemy.animator.ResetTrigger("Executed");
        enemy.animator.ResetTrigger("Death");
        
        Debug.Log($"[ExecutedDieState] 敵人 {enemy.name} 退出被處決死亡狀態");
    }

    public override bool ShouldUseRootMotion() => true;
} 