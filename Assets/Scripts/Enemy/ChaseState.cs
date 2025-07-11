using UnityEngine;

public class ChaseState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("Entering ChaseState");
        enemy.animator.Play("Run");
        // 移除 canAutoAttack 設置，由 RetreatState 控制
    }

    public void UpdateState(EnemyAI enemy)
    {
        // 檢查是否在攻擊範圍內
        if (enemy.IsInAttackRange())
        {
            Debug.Log("ChaseState: 進入攻擊範圍，切換到 AttackState");
            enemy.SwitchState(new AttackState());
            return;
        }

        Vector3 seek = enemy.Seek(enemy.player.position);
        Vector3 avoid = enemy.ObstacleAvoid();
        Vector3 enemyAvoid = enemy.EnemyAvoid(); // 添加敵人避障
        Vector3 totalForce = seek + avoid + enemyAvoid; // 結合所有力
        
        // 調試信息
        if (enemy.showDebugInfo)
        {
            Debug.Log($"ChaseState Debug - Seek: {seek}, Obstacle Avoid: {avoid}, Enemy Avoid: {enemyAvoid}, Total: {totalForce}");
        }
        
        //Debug.Log($"Seek force: {seek}, Avoid force: {avoid}, Enemy avoid: {enemyAvoid}, Total force: {totalForce}, Velocity: {enemy.velocity}");
        
        enemy.Move(totalForce);
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log("Exiting ChaseState");
        enemy.Stop();
    }
}