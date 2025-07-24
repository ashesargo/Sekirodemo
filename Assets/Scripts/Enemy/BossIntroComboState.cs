using UnityEngine;

public class BossIntroComboState : BaseEnemyState
{
    private int comboStep = 0;
    private string[] combos = { "Combo1", "Combo2", "Combo3" };

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        var bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null && bossAI.pendingIntroComboStep >= 0)
        {
            comboStep = bossAI.pendingIntroComboStep;
            bossAI.pendingIntroComboStep = -1;
        }
        else
        {
            comboStep = 0;
        }
        PlayNextCombo(enemy);
    }

    public override void UpdateState(EnemyAI enemy)
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position, turnSpeed: 8f);
        }
        if (stateInfo.IsName("Idle"))
        {
            comboStep++;
            if (comboStep < combos.Length)
            {
                if (!enemy.IsInAttackRange())
                {
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

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        enemy.Stop();
    }
    public override bool ShouldUseRootMotion() => true;
} 