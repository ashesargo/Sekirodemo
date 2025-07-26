using UnityEngine;

public class BossChaseState : BaseEnemyState
{
    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
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

    public override void UpdateState(EnemyAI enemy)
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
        var bossAI = enemy.GetComponent<BossAI>();

        // 只要還沒做過開場Combo，無論距離多遠都只能追擊
        if (bossAI != null && !bossAI.hasDoneIntroCombo)
        {
            // 若有待續的開場Combo步驟，且進入攻擊範圍，則切回BossIntroComboState
            if (bossAI.pendingIntroComboStep >= 0 && enemy.IsInAttackRange())
            {
                enemy.SwitchState(new BossIntroComboState());
                return;
            }
            // 進入攻擊範圍才切到Combo123
            if (enemy.IsInAttackRange())
            {
                Debug.Log("BossChaseState: Boss第一次靠近玩家，進入開場Combo");
                enemy.SwitchState(new BossIntroComboState());
                return;
            }
            // 其餘情況都只追擊
            Vector3 seek = enemy.Seek(enemy.player.position);
            Vector3 obstacleAvoid = enemy.ObstacleAvoid();
            Vector3 enemyAvoid = enemy.EnemyAvoid();
            Vector3 totalForce = seek + obstacleAvoid + enemyAvoid;
            if (enemy.player != null)
                enemy.SmoothLookAt(enemy.player.position);
            enemy.Move(totalForce);
            return;
        }

        // ====== 以下是已做過開場Combo後的正常AI邏輯 ======
        if (enemy.IsInAttackRange())
        {
            // 若有待執行JumpAttack，優先執行
            if (bossAI != null && bossAI.pendingJumpAttack)
            {
                bossAI.pendingJumpAttack = false;
                enemy.SwitchState(new BossJumpAttackState());
                return;
            }
            // 剛做過RangeAttack或Rush，必須先進入Combo
            if (bossAI != null && bossAI.hasJustRangedOrRushed)
            {
                bossAI.hasJustRangedOrRushed = false;
                Debug.Log("BossChaseState: 剛做過遠程/突進，必須先進入Combo");
                enemy.SwitchState(new BossComboState());
                return;
            }
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
        // 超過40f才觸發RangeAttack1
        if (distanceToPlayer > 40f && bossAI != null && !bossAI.hasJustRangedOrRushed)
        {
            Debug.Log("BossChaseState: 玩家距離超過40f，切換到 Range Attack1");
            enemy.SwitchState(new BossRangedAttackState(1));
            return;
        }
        // 超過25f才隨機選擇RangeAttack2或Rush
        if (distanceToPlayer > 25f && bossAI != null && !bossAI.hasJustRangedOrRushed)
        {
            int idx = Random.Range(0, 2);
            if (idx == 0)
            {
                Debug.Log("BossChaseState: 玩家距離超過25f，切換到 Range Attack2");
                enemy.SwitchState(new BossRangedAttackState(2));
                return;
            }
            else
            {
                Debug.Log("BossChaseState: 玩家距離超過25f，切換到 Rush");
                enemy.SwitchState(new BossRushState());
                return;
            }
        }
        // 其餘情況都追擊
        Vector3 seek2 = enemy.Seek(enemy.player.position);
        Vector3 obstacleAvoid2 = enemy.ObstacleAvoid();
        Vector3 enemyAvoid2 = enemy.EnemyAvoid();
        Vector3 totalForce2 = seek2 + obstacleAvoid2 + enemyAvoid2;
        if (enemy.player != null)
            enemy.SmoothLookAt(enemy.player.position);
        enemy.Move(totalForce2);
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log("BossChaseState: Boss退出追擊狀態");
        enemy.Stop();
    }
    public override bool ShouldUseRootMotion() => false;
} 