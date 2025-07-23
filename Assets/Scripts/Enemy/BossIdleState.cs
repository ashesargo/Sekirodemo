using UnityEngine;

public class BossIdleState : IEnemyState
{
    public void EnterState(EnemyAI enemy)
    {
        enemy.animator.Play("Idle");
        enemy.Stop();
        enemy.canAutoAttack = true;
        Debug.Log("BossIdleState: Boss進入待機狀態");
    }

    public void UpdateState(EnemyAI enemy)
    {
        // 檢查 player 是否為 null
        if (enemy.player == null)
        {
            Debug.LogWarning("Player is null! Check if Player GameObject has Tag = 'Player'");
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
        bool canSee = enemy.CanSeePlayer();

        if (canSee)
        {
            var bossAI = enemy.GetComponent<BossAI>();
            if (bossAI != null && !bossAI.hasDoneIntroCombo)
            {
                // 只有距離夠近才進入Combo，否則先追擊
                if (distance < enemy.attackRange)
                {
                    Debug.Log("BossIdleState: Boss第一次靠近玩家，進入開場Combo");
                    enemy.SwitchState(new BossIntroComboState());
                }
                else
                {
                    Debug.Log("BossIdleState: Boss發現玩家，先追擊");
                    bossAI.pendingIntroComboStep = -1;
                    bossAI.pendingJumpAttack = false;
                    enemy.SwitchState(new BossChaseState());
                }
            }
            else
            {
                Debug.Log("BossIdleState: Boss發現玩家，直接進入追擊狀態");
                enemy.SwitchState(new BossChaseState());
            }
        }

        // 新增：待機時慢慢轉向玩家
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position); // 使用 Inspector 可調整的 lookAtTurnSpeed
        }
    }

    public void ExitState(EnemyAI enemy) 
    {
        Debug.Log("BossIdleState: Boss退出待機狀態");
    }
} 