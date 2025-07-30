using UnityEngine;

public class HitState : BaseEnemyState
{
    private float timer = 0f;
    private float maxHitTime = 1.5f; // 最多等待1.5秒，避免卡死
    private bool isParry = false;
    private bool isDefending = false; // 是否正在防禦
    private string parryAnim = "";
    private string defendAnim = "Defend"; // 防禦動畫名稱
    
    // 靜態變數來記錄是否應該防禦
    public static bool shouldDefend = false;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        var enemyTest = enemy.GetComponent<EnemyTest>();
        if (enemyTest != null && enemyTest.isDead)
            return; // 死亡時不再進入 Hit
        Debug.Log($"[HitState] EnterState: {enemy.name}");

        // 使用靜態變數來決定是否防禦
        bool shouldDefendThisTime = shouldDefend;
        shouldDefend = false; // 重置靜態變數

        // Boss隨機Parry動畫
        BossAI bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null)
        {
            if (shouldDefendThisTime)
            {
                // Boss防禦動畫
                string[] defendAnims = { "Defend", "Parry", "Parry2" };
                int idx = Random.Range(0, defendAnims.Length);
                defendAnim = defendAnims[idx];
                isDefending = true;
                isParry = false;
                enemy.animator.Play(defendAnim);
                Debug.Log($"[HitState] Boss防禦動畫: {defendAnim} (70%機率)");
            }
            else
            {
                // Boss受傷動畫
                string[] parryAnims = { "Parry", "Parry2" };
                int idx = Random.Range(0, parryAnims.Length);
                parryAnim = parryAnims[idx];
                isParry = true;
                isDefending = false;
                enemy.animator.Play(parryAnim);
                Debug.Log($"[HitState] Boss受傷動畫: {parryAnim} (30%機率)");
            }
        }
        else
        {
            // 一般敵人
            if (shouldDefendThisTime)
            {
                // 防禦動畫
                isDefending = true;
                isParry = false;
                enemy.animator.SetTrigger("Defend");
                Debug.Log($"[HitState] 一般敵人防禦動畫 (70%機率)");
            }
            else
            {
                // 受傷動畫
                isDefending = false;
                isParry = false;
                enemy.animator.SetTrigger("Hit");
                Debug.Log($"[HitState] 一般敵人受傷動畫 (30%機率)");
            }
        }
        
        timer = 0f;
        enemy.canAutoAttack = false; // 受傷時禁用自動攻擊
    }

    public override void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[HitState] UpdateState: {enemy.name}, timer={timer:F2}, state={stateInfo.fullPathHash}, normalizedTime={stateInfo.normalizedTime:F2}");

        // 取得血量
        var enemyTest = enemy.GetComponent<EnemyTest>();
        int hp = enemyTest != null ? enemyTest.GetCurrentHP() : 1;

        // 判斷動畫結束或超時
        bool animEnd = false;
        
        if (isDefending)
        {
            // 防禦動畫結束檢查
            if (stateInfo.IsName("Defend") && stateInfo.normalizedTime >= 1.0f)
                animEnd = true;
            else if (stateInfo.IsName("Parry") && stateInfo.normalizedTime >= 1.0f)
                animEnd = true;
            else if (stateInfo.IsName("Parry2") && stateInfo.normalizedTime >= 1.0f)
                animEnd = true;
        }
        else if (isParry)
        {
            // Boss受傷動畫結束檢查
            if (stateInfo.IsName(parryAnim) && stateInfo.normalizedTime >= 1.0f)
                animEnd = true;
        }
        else
        {
            // 一般敵人受傷動畫結束檢查
            if (stateInfo.IsName("Hit") && stateInfo.normalizedTime >= 1.0f)
                animEnd = true;
        }
        
        if (timer > maxHitTime)
            animEnd = true;

        if (animEnd)
        {
            if (hp <= 0)
            {
                Debug.Log($"[HitState] 進入死亡狀態: {enemy.name}");
                enemy.SwitchState(new DieState());
            }
            else
            {
                Debug.Log($"[HitState] 動畫結束或超時，切換狀態: {enemy.name}");
                enemy.canAutoAttack = true; // 受傷結束後重新啟用自動攻擊
                // 檢查是否為Boss
                BossAI bossAI = enemy.GetComponent<BossAI>();
                if (bossAI != null)
                {
                    // 移除防禦後的重置連擊計數，保持連擊順序
                    Debug.Log("HitState: Boss防禦後保持連擊計數，直接進入追擊狀態");
                    enemy.SwitchState(new BossChaseState());
                }
                else
                {
                    if (enemy.CanSeePlayer())
                        enemy.SwitchState(new ChaseState());
                    else
                        enemy.SwitchState(new IdleState());
                }
            }
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        Debug.Log($"[HitState] ExitState: {enemy.name}");
    }
    
    // 檢查是否正在防禦
    public bool IsDefending()
    {
        return isDefending;
    }
    
    public override bool ShouldUseRootMotion() => true;
} 