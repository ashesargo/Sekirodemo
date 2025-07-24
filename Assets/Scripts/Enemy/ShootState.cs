using UnityEngine;

public class ShootState : BaseEnemyState
{
    private bool hasShot = false;
    private bool hasTriggeredShot = false;
    private float timer = 0f;
    private float maxShootTime = 3f;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("ShootState: 進入射擊狀態");
        enemy.Stop();
        if (enemy.animator.HasState(0, Animator.StringToHash("Shoot")))
        {
            enemy.animator.Play("Shoot");
        }
        else
        {
            enemy.animator.SetTrigger("Attack");
        }
        hasShot = false;
        hasTriggeredShot = false;
        timer = 0f;
        enemy.canAutoAttack = false;
    }

    public override void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        if (!hasTriggeredShot && (stateInfo.IsName("Shoot") || stateInfo.IsName("Attack")))
        {
            hasTriggeredShot = true;
            Debug.Log("ShootState: 射擊動畫開始播放");
            PerformShoot(enemy);
        }
        if ((hasTriggeredShot && (stateInfo.IsName("Shoot") || stateInfo.IsName("Attack")) && stateInfo.normalizedTime >= 1.0f) || timer > maxShootTime)
        {
            if (timer > maxShootTime)
            {
                Debug.Log("ShootState: 射擊超時，強制切換狀態");
            }
            if (enemy.player != null)
            {
                float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
                if (distanceToPlayer < 20f)
                {
                    Debug.Log("ShootState: 射擊完成，但玩家太近，直接進行近戰攻擊");
                    enemy.SwitchState(new CloseCombatState());
                }
                else
                {
                    Debug.Log("ShootState: 射擊動畫結束，切換到 ReloadState");
                    enemy.SwitchState(new ReloadState());
                }
            }
            else
            {
                Debug.Log("ShootState: 射擊動畫結束，找不到玩家，切換到 ReloadState");
                enemy.SwitchState(new ReloadState());
            }
        }
        if (enemy.showDebugInfo)
        {
            float distanceToPlayer = enemy.player != null ? Vector3.Distance(enemy.transform.position, enemy.player.position) : 0f;
            Debug.Log($"ShootState: 射擊中，時間 = {timer:F2}/{maxShootTime:F2}, 距離玩家 = {distanceToPlayer:F2}");
        }
    }

    private void PerformShoot(EnemyAI enemy)
    {
        if (hasShot) return;
        hasShot = true;
        Debug.Log("ShootState: 執行射擊邏輯");
        var rangedEnemy = enemy.GetComponent<RangedEnemy>();
        if (rangedEnemy != null)
        {
            rangedEnemy.FireProjectile();
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log("ShootState: 退出射擊狀態");
        enemy.Stop();
    }
    public override bool ShouldUseRootMotion() => true;
} 