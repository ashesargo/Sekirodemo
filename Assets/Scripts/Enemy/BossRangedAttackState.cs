using UnityEngine;

public class BossRangedAttackState : BaseEnemyState
{
    private int attackType = 1; // 1: Range Attack1, 2: Range Attack2
    private string animName = "RangedAttack";

    public BossRangedAttackState() { attackType = 1; animName = "RangedAttack"; }
    public BossRangedAttackState(int type)
    {
        attackType = type;
        if (type == 1) animName = "Range Attack1";
        else if (type == 2) animName = "Range Attack2";
        else animName = "RangedAttack";
    }

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log($"BossRangedAttackState: 進入遠程攻擊狀態 {animName}");
        enemy.animator.Play(animName);
        enemy.Stop();
        var bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null)
            bossAI.hasJustRangedOrRushed = true;
        var bossRanged = enemy.GetComponent<BossRangedAttack>();
        if (bossRanged != null)
        {
            bossRanged.FireBossProjectile();
        }
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
            enemy.SwitchState(new BossChaseState());
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log($"BossRangedAttackState: 退出遠程攻擊狀態 {animName}");
        enemy.Stop();
    }
    public override bool ShouldUseRootMotion() => true;
} 