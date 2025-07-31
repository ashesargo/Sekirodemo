using UnityEngine;

public class DieState : BaseEnemyState
{
    private bool deathAnimationStarted = false;
    private bool poolReturnStarted = false;
    private float deathTimer = 0f;
    private float deathAnimationDuration = 2f; // 死亡動畫持續時間

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        
        // 設置死亡標記
        EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.isDead = true;
        }
        
        // 停止移動
        enemy.Stop();
        
        // 禁用所有能力
        enemy.canBeParried = false;
        enemy.canAutoAttack = false;
        
        // 重置所有動畫參數，避免與其他動畫衝突
        enemy.animator.ResetTrigger("Attack");
        enemy.animator.ResetTrigger("Stagger");
        enemy.animator.ResetTrigger("Executed");
        enemy.animator.ResetTrigger("Hit");
        enemy.animator.ResetTrigger("Defend");
        enemy.animator.ResetTrigger("Run");
        enemy.animator.ResetTrigger("Idle");
        
        // 立即播放死亡動畫
        enemy.animator.SetTrigger("Death");
        deathAnimationStarted = true;
        deathTimer = 0f;
        Debug.Log($"[DieState] 敵人 {enemy.name} 進入死亡狀態並播放死亡動畫");
        
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
        
        Debug.Log($"[DieState] 敵人 {enemy.name} 進入死亡狀態");
    }

    public override void UpdateState(EnemyAI enemy)
    {
        if (deathAnimationStarted && !poolReturnStarted)
        {
            deathTimer += Time.deltaTime;
            
            // 等待死亡動畫播放完畢後開始物件池回收
            if (deathTimer >= deathAnimationDuration)
            {
                poolReturnStarted = true;
                
                // 回歸物件池
                ObjectPool objectPool = ObjectPool.Instance;
                if (objectPool != null)
                {
                    objectPool.ReturnPooledObject(enemy.gameObject);
                    Debug.Log($"[DieState] 敵人 {enemy.name} 已回歸物件池");
                }
            }
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        
        // 確保動畫參數被重置
        enemy.animator.ResetTrigger("Death");
        
        Debug.Log($"[DieState] 敵人 {enemy.name} 退出死亡狀態");
    }

    public override bool ShouldUseRootMotion() => true;
} 