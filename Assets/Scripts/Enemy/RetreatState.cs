using UnityEngine;

public class RetreatState : IEnemyState
{
    private float timer = 0f;
    private Vector3 retreatDir;

    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("RetreatState: 進入後退狀態");
        // 如果沒有 Retreat 動畫，使用 Idle 動畫代替
        if (enemy.animator.HasState(0, Animator.StringToHash("Retreat")))
        {
            enemy.animator.Play("Retreat");
        }
        else
        {
            enemy.animator.Play("Idle");
        }
        
        timer = 0f;
        
        // 計算背向玩家的方向
        retreatDir = (enemy.transform.position - enemy.player.position).normalized;
        retreatDir.y = 0f;
        
        //Debug.Log($"RetreatState: 後退方向 = {retreatDir}");
        enemy.canAutoAttack = false; // 後退時禁用自動攻擊
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        
        // 計算後退力，直接使用 retreatDir 方向
        Vector3 retreatForce = retreatDir * enemy.maxSpeed * 0.5f;
        
        // 使用專門的撤退避障方法，檢測後方和側面
        Vector3 obstacleAvoid = enemy.RetreatObstacleAvoid();
        
        // 結合後退力和障礙物避障
        Vector3 totalForce = retreatForce + obstacleAvoid;
        
        enemy.Move(totalForce, true); // 傳 true 讓敵人始終面向玩家
        
        Debug.Log($"RetreatState: 後退中，時間 = {timer:F2}, 後退力 = {retreatForce}, 障礙物避障 = {obstacleAvoid}, 總力 = {totalForce}, 距離玩家 = {Vector3.Distance(enemy.transform.position, enemy.player.position):F2}");
        
        if (timer >= enemy.retreatTime)
        {
            Debug.Log("RetreatState: 後退結束，切換到 ChaseState");
            enemy.canAutoAttack = true; // 後退結束時重新啟用自動攻擊
            enemy.SwitchState(new ChaseState());
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log("RetreatState: 退出後退狀態");
        enemy.Stop();
    }
} 