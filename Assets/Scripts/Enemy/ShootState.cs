using UnityEngine;

public class ShootState : IEnemyState
{
    private bool hasShot = false;
    private bool hasTriggeredShot = false;
    private float timer = 0f;
    private float maxShootTime = 3f; // 最多等待3秒，避免卡死

    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("ShootState: 進入射擊狀態");
        enemy.Stop();
        
        // 播放射擊動畫
        if (enemy.animator.HasState(0, Animator.StringToHash("Shoot")))
        {
            enemy.animator.Play("Shoot");
        }
        else
        {
            // 如果沒有Shoot動畫，使用Attack動畫
            enemy.animator.SetTrigger("Attack");
        }
        
        hasShot = false;
        hasTriggeredShot = false;
        timer = 0f;
        enemy.canAutoAttack = false; // 射擊時禁用自動攻擊
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        
        // 檢測射擊動畫開始
        if (!hasTriggeredShot && (stateInfo.IsName("Shoot") || stateInfo.IsName("Attack")))
        {
            hasTriggeredShot = true;
            Debug.Log("ShootState: 射擊動畫開始播放");
            
            // 在這裡可以觸發實際的射擊邏輯
            PerformShoot(enemy);
        }
        
        // 等待射擊動畫結束或超時
        if ((hasTriggeredShot && (stateInfo.IsName("Shoot") || stateInfo.IsName("Attack")) && stateInfo.normalizedTime >= 1.0f) || timer > maxShootTime)
        {
            if (timer > maxShootTime)
            {
                Debug.Log("ShootState: 射擊超時，強制切換狀態");
            }
            
            // 射擊動畫結束後，檢查玩家距離
            if (enemy.player != null)
            {
                float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
                if (distanceToPlayer < 20f) // 如果玩家太近
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
        
        // 調試信息
        if (enemy.showDebugInfo)
        {
            float distanceToPlayer = enemy.player != null ? Vector3.Distance(enemy.transform.position, enemy.player.position) : 0f;
            Debug.Log($"ShootState: 射擊中，時間 = {timer:F2}/{maxShootTime:F2}, 距離玩家 = {distanceToPlayer:F2}");
        }
    }

    private void PerformShoot(EnemyAI enemy)
    {
        if (hasShot) return; // 避免重複射擊
        
        hasShot = true;
        
        // 這裡可以添加實際的射擊邏輯
        // 例如：生成子彈、播放射擊音效等
        
        Debug.Log("ShootState: 執行射擊邏輯");
        
        // 示例：可以調用敵人身上的射擊組件
        var rangedEnemy = enemy.GetComponent<RangedEnemy>();
        if (rangedEnemy != null)
        {
            rangedEnemy.FireProjectile();
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log("ShootState: 退出射擊狀態");
        enemy.Stop();
    }
} 