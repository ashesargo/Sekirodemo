using UnityEngine;

public class ReloadState : BaseEnemyState
{
    private float reloadTime = 2f; // 裝彈時間
    private float timer = 0f;
    private bool hasStartedReloading = false;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("ReloadState: 進入裝彈狀態");
        enemy.Stop();
        
        // 播放裝彈動畫
        if (enemy.animator.HasState(0, Animator.StringToHash("Reload")))
        {
            enemy.animator.Play("Reload");
        }
        else
        {
            // 如果沒有Reload動畫，使用Idle動畫
            enemy.animator.Play("Idle");
        }
        
        timer = 0f;
        hasStartedReloading = false;
        enemy.canAutoAttack = false; // 裝彈時禁用自動攻擊
    }

    public override void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        
        // 檢測裝彈動畫開始
        if (!hasStartedReloading && stateInfo.IsName("Reload"))
        {
            hasStartedReloading = true;
            Debug.Log("ReloadState: 裝彈動畫開始播放");
        }
        
        // 檢查玩家是否太近（需要先攻擊再撤退）
        if (enemy.player != null)
        {
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
            if (distanceToPlayer < 20f) // 如果玩家距離小於20f
            {
                Debug.Log("ReloadState: 玩家太近，先攻擊再撤退，切換到 CloseCombatState");
                enemy.SwitchState(new CloseCombatState());
                return;
            }
        }
        
        // 裝彈完成後檢查下一步行動
        if (timer >= reloadTime)
        {
            if (enemy.player != null)
            {
                float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
                
                if (enemy.IsInAttackRange())
                {
                    Debug.Log("ReloadState: 裝彈完成，玩家在攻擊範圍內，切換到 AimState");
                    enemy.SwitchState(new AimState());
                }
                else
                {
                    Debug.Log("ReloadState: 裝彈完成，玩家不在攻擊範圍內，切換到 ChaseState");
                    enemy.SwitchState(new ChaseState());
                }
            }
            else
            {
                Debug.Log("ReloadState: 裝彈完成，找不到玩家，切換到 IdleState");
                enemy.SwitchState(new IdleState());
            }
        }
        
        // 調試信息
        if (enemy.showDebugInfo)
        {
            Debug.Log($"ReloadState: 裝彈中，時間 = {timer:F2}/{reloadTime:F2}");
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log("ReloadState: 退出裝彈狀態");
        enemy.Stop();
    }
    public override bool ShouldUseRootMotion() => true;
} 