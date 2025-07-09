using UnityEngine;

public class IdleState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        enemy.animator.Play("Idle");
        enemy.Stop();
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
        
        Debug.Log($"Distance to player: {distance:F2}, Vision range: {enemy.visionRange}, Can see player: {canSee}");
        
        if (canSee)
        {
            enemy.SwitchState(new AlertState());
        }
    }

    public void ExitState(EnemyAI enemy) { }
}