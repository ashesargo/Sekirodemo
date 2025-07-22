using UnityEngine;

public class BossRangedAttackState : IEnemyState
{
    private bool hasAttacked = false;
    private float timer = 0f;
    private float maxAttackTime = 2.0f; // 遠程攻擊動畫最大等待時間
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
        Debug.Log($"BossRangedAttackState: 進入遠程攻擊狀態 {animName}");
        enemy.animator.Play(animName); // 直接Play對應動畫
        hasAttacked = false;
        timer = 0f;
        enemy.Stop();
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        // 動畫開始時觸發遠程攻擊
        if (!hasAttacked && stateInfo.IsName(animName) && stateInfo.normalizedTime > 0.1f)
        {
            hasAttacked = true;
            // 這裡可呼叫Boss專屬遠程攻擊方法
            var bossRanged = enemy.GetComponent<BossRangedAttack>();
            if (bossRanged != null)
            {
                bossRanged.FireBossProjectile();
            }
        }
        // 動畫結束或超時，切回追擊
        if ((hasAttacked && stateInfo.normalizedTime >= 1.0f) || timer > maxAttackTime)
        {
            enemy.SwitchState(new BossChaseState());
        }
        // 新增：遠程攻擊時慢慢轉向玩家
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position); // 使用 Inspector 可調整的 lookAtTurnSpeed
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log($"BossRangedAttackState: 退出遠程攻擊狀態 {animName}");
        enemy.Stop();
    }
} 