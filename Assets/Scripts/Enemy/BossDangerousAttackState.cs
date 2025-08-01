using UnityEngine;

public class BossDangerousAttackState : BaseEnemyState
{
    private string currentDangerousAnimation = "";
    private bool hasAttacked = false;
    private bool isDangerousAttackActive = false;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("BossDangerousAttackState: 進入Boss危攻擊狀態");
        enemy.Stop();
        
        BossAI bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null)
        {
            // 從BossAI獲取危攻擊動畫名稱
            currentDangerousAnimation = bossAI.GetDangerousAttackAnimation();
            enemy.animator.Play(currentDangerousAnimation);
        }
        else
        {
            // 如果沒有BossAI，使用預設的危攻擊動畫
            enemy.animator.ResetTrigger("DangerousAttack");
            enemy.animator.SetTrigger("DangerousAttack");
        }
        
        hasAttacked = false;
        isDangerousAttackActive = false;
        enemy.canAutoAttack = false;
    }

    public override void UpdateState(EnemyAI enemy)
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        
        // 面向玩家
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position, turnSpeed: 8f);
        }
        
        // 檢查是否正在播放危攻擊動畫
        if (!hasAttacked && (stateInfo.IsName("DangerousAttack") || stateInfo.IsName(currentDangerousAnimation)))
        {
            hasAttacked = true;
            Debug.Log("BossDangerousAttackState: Boss危攻擊動畫開始播放");
            enemy.Stop();
        }
        
        // 等待攻擊動畫結束
        if (hasAttacked && (stateInfo.IsName("DangerousAttack") || stateInfo.IsName(currentDangerousAnimation)) && stateInfo.normalizedTime >= 1.0f)
        {
            Debug.Log("BossDangerousAttackState: Boss危攻擊動畫結束");
            
            // 根據BossAI的設定決定下一步行動
            BossAI bossAI = enemy.GetComponent<BossAI>();
            if (bossAI != null)
            {
                // 可以設定Boss危攻擊後的特定行為
                if (Random.value < 0.3f) // 30%機率繼續攻擊
                {
                    enemy.SwitchState(new BossComboState());
                }
                else
                {
                    enemy.SwitchState(new RetreatState());
                }
            }
            else
            {
                enemy.SwitchState(new RetreatState());
            }
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log("BossDangerousAttackState: Boss退出危攻擊狀態");
        enemy.Stop();
        isDangerousAttackActive = false;
    }

    public override bool ShouldUseRootMotion() => true;

    // 動畫事件調用：開始危攻擊
    public void OnBossDangerousAttackStart()
    {
        isDangerousAttackActive = true;
        Debug.Log("BossDangerousAttackState: Boss危攻擊開始，無視防禦！");
    }

    // 動畫事件調用：結束危攻擊
    public void OnBossDangerousAttackEnd()
    {
        isDangerousAttackActive = false;
        Debug.Log("BossDangerousAttackState: Boss危攻擊結束");
    }

    // 檢查是否正在進行危攻擊
    public bool IsDangerousAttackActive()
    {
        return isDangerousAttackActive;
    }
} 