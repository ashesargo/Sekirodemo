using UnityEngine;

public class BossIdleState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        enemy.animator.Play("Idle");
        enemy.Stop();
        enemy.canAutoAttack = true;
        Debug.Log("BossIdleState: Boss進入待機狀態");
    }

    public void UpdateState(EnemyAI enemy)
    {
        // 檢查 player 是否為 null
        if (enemy.player == null)
        {
            Debug.LogWarning("Player is null! Check if Player GameObject has Tag = 'Player'");
            return;
        }

        // 檢查距離
        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
        bool canSee = enemy.CanSeePlayer();
        
        if (canSee)
        {
            Debug.Log("BossIdleState: Boss發現玩家，直接進入追擊狀態");
            enemy.SwitchState(new BossChaseState());
        }

    }

    public void ExitState(EnemyAI enemy) 
    {
        Debug.Log("BossIdleState: Boss退出待機狀態");
    }
} 