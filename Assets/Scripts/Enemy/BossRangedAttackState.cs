using UnityEngine;

public class BossRangedAttackState : IEnemyState
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

    public void EnterState(EnemyAI enemy)
    {
        enemy.SetRootMotion(true);
        Debug.Log($"BossRangedAttackState: 進入遠程攻擊狀態 {animName}");
        enemy.animator.Play(animName); // 直接Play對應動畫
        enemy.Stop();
        // 標記剛做過遠程
        var bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null)
            bossAI.hasJustRangedOrRushed = true;
        // 這裡可呼叫Boss專屬遠程攻擊方法
        var bossRanged = enemy.GetComponent<BossRangedAttack>();
        if (bossRanged != null)
        {
            bossRanged.FireBossProjectile();
        }
    }

    public void UpdateState(EnemyAI enemy)
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        // 攻擊期間持續追蹤玩家
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position, turnSpeed: 8f);
        }
        // 只要動畫已經自動Transition到Idle（或ComboEnd等），就切換狀態
        if (stateInfo.IsName("Idle"))
        {
            enemy.SwitchState(new BossChaseState());
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        enemy.SetRootMotion(false);
        Debug.Log($"BossRangedAttackState: 退出遠程攻擊狀態 {animName}");
        enemy.Stop();
    }
} 