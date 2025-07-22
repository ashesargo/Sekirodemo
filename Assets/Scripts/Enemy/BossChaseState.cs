using UnityEngine;

public class BossChaseState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("BossChaseState: Boss進入追擊狀態");
        
        // 檢查是否有Run動畫，如果沒有則使用Idle
        if (enemy.animator.HasState(0, Animator.StringToHash("Run")))
        {
            enemy.animator.Play("Run");
        }
        else
        {
            enemy.animator.Play("Idle");
            Debug.Log("BossChaseState: 沒有找到Run動畫，使用Idle動畫");
        }
    }

    public void UpdateState(EnemyAI enemy)
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
        Debug.Log($"[BossChaseState] 距離玩家: {distanceToPlayer:F2}, 攻擊範圍: {enemy.attackRange}, IsInAttackRange: {enemy.IsInAttackRange()}");
        // 檢查是否在攻擊範圍內
        if (enemy.IsInAttackRange())
        {
            Debug.Log("BossChaseState: Boss進入攻擊範圍，開始連擊攻擊");
            enemy.SwitchState(new BossComboState());
            return;
        }
        float minDistance = enemy.attackRange;
        if (distanceToPlayer <= minDistance)
        {
            enemy.velocity = Vector3.zero;
            return;
        }
        // 只有在玩家非常遠時才考慮遠程攻擊或Rush
        if (distanceToPlayer > 40f)
        {
            int idx = Random.Range(0, 3);
            if (idx == 0)
            {
                Debug.Log("BossChaseState: 玩家非常遠，切換到 Range Attack1");
                enemy.SwitchState(new BossRangedAttackState(1));
                return;
            }
            else if (idx == 1)
            {
                Debug.Log("BossChaseState: 玩家非常遠，切換到 Range Attack2");
                enemy.SwitchState(new BossRangedAttackState(2));
                return;
            }
            else
            {
                Debug.Log("BossChaseState: 玩家非常遠，切換到 Rush");
                enemy.SwitchState(new BossRushState());
                return;
            }
        }
        // 其餘情況都追擊
        Vector3 seek = enemy.Seek(enemy.player.position);
        Vector3 obstacleAvoid = enemy.ObstacleAvoid(); // 前方避障（追擊專用）
        Vector3 enemyAvoid = enemy.EnemyAvoid(); // 敵人避障
        Vector3 totalForce = seek + obstacleAvoid + enemyAvoid; // 結合所有力
        
        // 調試信息（暫時關閉）
        //if (enemy.showDebugInfo)
        //{
        //    Debug.Log($"BossChaseState Debug - Seek: {seek}, 前方避障: {obstacleAvoid}, 敵人避障: {enemyAvoid}, 總力: {totalForce}");
        //}
        
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position);
        }
        enemy.Move(totalForce);
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log("BossChaseState: Boss退出追擊狀態");
        enemy.Stop();
    }
} 