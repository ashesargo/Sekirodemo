using UnityEngine;

public class DieState : BaseEnemyState
{
    private bool deathAnimationStarted = false;

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
        // 播放死亡動畫（只播放一次）
        if (!deathAnimationStarted)
        {
            enemy.animator.SetTrigger("Death");
            deathAnimationStarted = true;
            Debug.Log($"[DieState] 敵人 {enemy.name} 播放死亡動畫");
        }
        
        // 死亡狀態不做其他事情，等待物件池回收
        // 注意：物件池回收邏輯在 EnemyTest.ReturnToPoolAfterDeath() 中處理
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