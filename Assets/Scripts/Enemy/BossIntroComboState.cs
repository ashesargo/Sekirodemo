using UnityEngine;

public class BossIntroComboState : IEnemyState
{
    private int comboStep = 0;
    private string[] combos = { "Combo1", "Combo2", "Combo3" };

    public void EnterState(EnemyAI enemy)
    {
        enemy.SetRootMotion(true);
        var bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null && bossAI.pendingIntroComboStep >= 0)
        {
            comboStep = bossAI.pendingIntroComboStep;
            bossAI.pendingIntroComboStep = -1; // 進入時馬上重設，避免重複
        }
        else
        {
            comboStep = 0;
        }
        PlayNextCombo(enemy);
    }

    public void UpdateState(EnemyAI enemy)
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        // 攻擊期間持續追蹤玩家
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position, turnSpeed: 8f);
        }
        if (stateInfo.IsName("Idle"))
        {
            comboStep++; // 每次Idle都++
            if (comboStep < combos.Length)
            {
                if (!enemy.IsInAttackRange())
                {
                    // 記錄下一個comboStep，切換到Chase
                    var bossAI = enemy.GetComponent<BossAI>();
                    if (bossAI != null)
                        bossAI.pendingIntroComboStep = comboStep;
                    enemy.SwitchState(new BossChaseState());
                    return;
                }
                PlayNextCombo(enemy);
            }
            else
            {
                // 開場Combo結束，進入正常AI
                var bossAI = enemy.GetComponent<BossAI>();
                if (bossAI != null)
                {
                    bossAI.hasDoneIntroCombo = true;
                    bossAI.pendingIntroComboStep = -1;
                }
                enemy.SwitchState(new BossChaseState());
            }
        }
    }

    private void PlayNextCombo(EnemyAI enemy)
    {
        enemy.animator.Play(combos[comboStep]);
    }

    public void ExitState(EnemyAI enemy)
    {
        enemy.SetRootMotion(false);
        enemy.Stop();
    }
} 