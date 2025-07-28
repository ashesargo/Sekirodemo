using UnityEngine;

public class RetreatState : BaseEnemyState
{
    private float timer = 0f;
    private Vector3 strafeDir; // 橫移方向
    private string strafeAnim = "Strafe";
    private int strafeSign = 1; // 1=右, -1=左

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        //Debug.Log("RetreatState: 進入橫移狀態");
        // 隨機決定橫移方向
        strafeSign = Random.value < 0.5f ? 1 : -1;
        strafeAnim = strafeSign == 1 ? "StrafeR" : "StrafeL";
        if (enemy.animator.HasState(0, Animator.StringToHash(strafeAnim)))
        {
            enemy.animator.Play(strafeAnim);
        }
        else
        {
            enemy.animator.Play("Idle");
        }
        timer = 0f;
        // 以玩家為基準，取得橫向（右手邊）方向
        Vector3 toPlayer = (enemy.player.position - enemy.transform.position).normalized;
        toPlayer.y = 0f;
        Vector3 right = Vector3.Cross(Vector3.up, toPlayer).normalized;
        strafeDir = right * strafeSign; // 右或左
        enemy.canAutoAttack = false; // 橫移時禁用自動攻擊
    }

    public override void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        // 以玩家為基準，動態取得橫向方向
        Vector3 toPlayer = (enemy.player.position - enemy.transform.position).normalized;
        toPlayer.y = 0f;
        Vector3 right = Vector3.Cross(Vector3.up, toPlayer).normalized;
        strafeDir = right * strafeSign;
        // 計算橫移力
        Vector3 strafeForce = strafeDir * enemy.maxSpeed * 0.5f;
        
        // 使用統一的避障方法
        Vector3 obstacleAvoid = enemy.ObstacleAvoid(); // 前方避障
        Vector3 enemyAvoid = enemy.EnemyAvoid(); // 敵人避障
        Vector3 totalForce = strafeForce + obstacleAvoid + enemyAvoid;
        // 橫移時面向玩家
        enemy.Move(totalForce, true);
        if (timer >= enemy.retreatTime)
        {
            BossAI bossAI = enemy.GetComponent<BossAI>();
            if (bossAI != null)
            {
                // Debug.Log("RetreatState: Boss橫移結束，切換到 BossChaseState");
                enemy.canAutoAttack = true;
                enemy.SwitchState(new BossChaseState());
            }
            else
            {
                // Debug.Log("RetreatState: 橫移結束，切換到 ChaseState");
                enemy.canAutoAttack = true;
                enemy.SwitchState(new ChaseState());
            }
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        // Debug.Log("RetreatState: 退出後退狀態");
        enemy.Stop();
    }
    
    public override bool ShouldUseRootMotion() => false;

    // 測試撤退方向的方法
    [ContextMenu("Test Retreat Direction")]
    public void TestRetreatDirection()
    {
        // Debug.Log("=== 撤退方向測試 ===");
        
        // 模擬敵人位置和玩家位置
        Vector3 enemyPos = Vector3.zero;
        Vector3 playerPos = new Vector3(5f, 0f, 0f); // 玩家在敵人前方
        
        Vector3 retreatDir = (enemyPos - playerPos).normalized;
        retreatDir.y = 0f;
        
        Debug.Log($"敵人位置: {enemyPos}");
        Debug.Log($"玩家位置: {playerPos}");
        Debug.Log($"撤退方向: {retreatDir}");
        Debug.Log($"撤退方向 X: {retreatDir.x:F3}, Z: {retreatDir.z:F3}");
        
        // 測試不同位置的撤退方向
        Vector3[] testPositions = {
            new Vector3(0f, 0f, 0f),    // 原點
            new Vector3(5f, 0f, 5f),    // 右前方
            new Vector3(-5f, 0f, 5f),   // 左前方
            new Vector3(0f, 0f, 10f),   // 正前方
        };
        
        foreach (var pos in testPositions)
        {
            Vector3 testRetreatDir = (pos - playerPos).normalized;
            testRetreatDir.y = 0f;
            Debug.Log($"位置 {pos} 的撤退方向: {testRetreatDir}");
        }
    }
} 