using UnityEngine;

public class ChaseState : BaseEnemyState
{
    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("Entering ChaseState");
        enemy.animator.Play("Run");
        // 移除 canAutoAttack 設置，由 RetreatState 控制
    }

    public override void UpdateState(EnemyAI enemy)
    {
        // 檢查是否在攻擊範圍內
        if (enemy.IsInAttackRange())
        {
            // 加入調試信息
            if (enemy.player != null)
            {
                float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
                float attackRange = enemy.attackRange;
                if (enemy is StrongEnemyAI strongEnemy)
                {
                    attackRange = strongEnemy.comboAttackRange;
                }
                Debug.Log($"ChaseState Debug - 距離玩家: {distance:F2}, 攻擊範圍: {attackRange}, 敵人類型: {enemy.GetType().Name}");
            }
            
            // 檢查是否為強力小兵
            if (enemy is StrongEnemyAI)
            {
                Debug.Log("ChaseState: 進入攻擊範圍，切換到 StrongEnemyComboState (強力小兵)");
                enemy.SwitchState(new StrongEnemyComboState());
                return;
            }
            else
            {
                Debug.Log($"ChaseState: 敵人不是StrongEnemyAI，實際類型是: {enemy.GetType().Name}");
            }
            
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
        // 新增：如果距離玩家太近，停止移動，避免推擠
        float minDistance = enemy.attackRange; // 或可加一個 buffer
        if (Vector3.Distance(enemy.transform.position, enemy.player.position) <= minDistance)
        {
            enemy.velocity = Vector3.zero;
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
        // 新增：追擊時慢慢轉向玩家
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position); // 使用 Inspector 可調整的 lookAtTurnSpeed
        }
        
        //Debug.Log($"Seek force: {seek}, Avoid force: {avoid}, Enemy avoid: {enemyAvoid}, Total force: {totalForce}, Velocity: {enemy.velocity}");
        
        enemy.Move(totalForce);
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log("Exiting ChaseState");
        enemy.Stop();
    }
    public override bool ShouldUseRootMotion() => false;
}