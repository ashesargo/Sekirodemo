using UnityEngine;

public class AimState : BaseEnemyState
{
    private float aimTime = 1.5f; // 瞄準時間
    private float timer = 0f;
    private bool hasStartedAiming = false;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("AimState: 進入瞄準狀態");
        enemy.Stop();
        
        // 播放瞄準動畫，如果沒有Aim動畫則使用Idle
        if (enemy.animator.HasState(0, Animator.StringToHash("Aim")))
        {
            enemy.animator.Play("Aim");
        }
        else
        {
            enemy.animator.Play("Idle");
        }
        
        timer = 0f;
        hasStartedAiming = false;
        enemy.canAutoAttack = false; // 瞄準時禁用自動攻擊
    }

    public override void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        
        // 面向玩家
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position); // 使用 Inspector 可調整的 lookAtTurnSpeed
        }
        
        // 檢查玩家是否還在攻擊範圍內
        if (!enemy.IsInAttackRange())
        {
            Debug.Log("AimState: 玩家離開攻擊範圍，切換到 ChaseState");
            enemy.SwitchState(new ChaseState());
            return;
        }
        
        // 檢查玩家是否太近（需要先攻擊再撤退）
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
        if (distanceToPlayer < 20f) // 如果玩家距離小於20f
        {
            Debug.Log("AimState: 玩家太近，先攻擊再撤退，切換到 CloseCombatState");
            enemy.SwitchState(new CloseCombatState());
            return;
        }
        
        // 瞄準完成後射擊
        if (timer >= aimTime)
        {
            Debug.Log("AimState: 瞄準完成，切換到 ShootState");
            enemy.SwitchState(new ShootState());
        }
        
        // 調試信息
        if (enemy.showDebugInfo)
        {
            Debug.Log($"AimState: 瞄準中，時間 = {timer:F2}/{aimTime:F2}, 距離玩家 = {distanceToPlayer:F2}");
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log("AimState: 退出瞄準狀態");
        enemy.Stop();
    }
    public override bool ShouldUseRootMotion() => true;
} 