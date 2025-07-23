using UnityEngine;

public class HitState : IEnemyState
{
    private float timer = 0f;
    private float maxHitTime = 1.5f; // 最多等待1.5秒，避免卡死

    public void EnterState(EnemyAI enemy)
    {
        var enemyTest = enemy.GetComponent<EnemyTest>();
        if (enemyTest != null && enemyTest.isDead)
            return; // 死亡時不再進入 Hit
        Debug.Log($"[HitState] EnterState: {enemy.name}");
        enemy.animator.SetTrigger("Hit");
        timer = 0f;
        enemy.canAutoAttack = false; // 受傷時禁用自動攻擊
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[HitState] UpdateState: {enemy.name}, timer={timer:F2}, state={stateInfo.fullPathHash}, normalizedTime={stateInfo.normalizedTime:F2}");

        // 取得血量
        var enemyTest = enemy.GetComponent<EnemyTest>();
        int hp = enemyTest != null ? enemyTest.GetCurrentHP() : 1;

        if ((stateInfo.IsName("Hit") && stateInfo.normalizedTime >= 1.0f) || timer > maxHitTime)
        {
            if (hp <= 0)
            {
                Debug.Log($"[HitState] 進入死亡狀態: {enemy.name}");
                enemy.SwitchState(new DieState());
            }
            else
            {
                Debug.Log($"[HitState] 動畫結束或超時，切換狀態: {enemy.name}");
                enemy.canAutoAttack = true; // 受傷結束後重新啟用自動攻擊
                
                // 檢查是否為Boss
                BossAI bossAI = enemy.GetComponent<BossAI>();
                if (bossAI != null)
                {
                    // Boss受傷後重置連擊計數
                    bossAI.ResetComboCount();
                    
                    // Boss受傷後直接進入追擊狀態，不回到Idle
                    Debug.Log("HitState: Boss受傷後直接進入追擊狀態");
                    enemy.SwitchState(new BossChaseState());
                }
                else
                {
                    if (enemy.CanSeePlayer())
                        enemy.SwitchState(new ChaseState());
                    else
                        enemy.SwitchState(new IdleState());
                }
            }
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log($"[HitState] ExitState: {enemy.name}");
    }
} 