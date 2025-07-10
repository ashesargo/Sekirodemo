using UnityEngine;

public class RetreatState : IEnemyState
{
    private float timer = 0f;
    private Vector3 retreatDir;

    public void EnterState(EnemyAI enemy)
    {
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
        enemy.canAutoAttack = false;
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        
        // 計算後退力，直接使用 retreatDir 方向
        Vector3 retreatForce = retreatDir * enemy.maxSpeed * 0.5f;
        enemy.Move(retreatForce, true); // 傳 true 讓敵人始終面向玩家
        
        Debug.Log($"RetreatState: 後退中，時間 = {timer:F2}, 力 = {retreatForce}");
        
        if (timer >= enemy.retreatTime)
        {
            Debug.Log("RetreatState: 後退結束，切換到 ChaseState");
            enemy.SwitchState(new ChaseState());
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        enemy.Stop();
    }
} 