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
            // 檢查是否為遠程兵種
            var rangedEnemy = enemy.GetComponent<RangedEnemy>();
            if (rangedEnemy != null)
            {
                Debug.Log("ChaseState: 進入攻擊範圍，切換到 AimState (遠程兵種)");
                enemy.SwitchState(new AimState());
            }
            else
            {
                Debug.Log("ChaseState: 進入攻擊範圍，切換到 AttackState (近戰兵種)");
                enemy.SwitchState(new AttackState());
            }
            return;
        }

        Vector3 seek = enemy.Seek(enemy.player.position);
        Vector3 obstacleAvoid = enemy.ObstacleAvoid(); // 前方避障（追擊專用）
        Vector3 enemyAvoid = enemy.EnemyAvoid(); // 敵人避障
        Vector3 totalForce = seek + obstacleAvoid + enemyAvoid; // 結合所有力
        
        // 調試信息
        if (enemy.showDebugInfo)
        {
            Debug.Log($"ChaseState Debug - Seek: {seek}, 前方避障: {obstacleAvoid}, 敵人避障: {enemyAvoid}, 總力: {totalForce}");
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