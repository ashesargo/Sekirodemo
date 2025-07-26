using UnityEngine;

public class IdleState : BaseEnemyState
{
    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        enemy.animator.Play("Idle");
        enemy.Stop();
        enemy.canAutoAttack = true; // 啟用自動攻擊
    }

    public override void UpdateState(EnemyAI enemy)
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
        
        //Debug.Log($"Distance to player: {distance:F2}, Vision range: {enemy.visionRange}, Can see player: {canSee}");
        
        if (canSee)
        {
            enemy.SwitchState(new AlertState());
        }
    }

    public override void ExitState(EnemyAI enemy) { base.ExitState(enemy); }
    public override bool ShouldUseRootMotion() => true;
}