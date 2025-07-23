using UnityEngine;

public class BossJumpAttackState : IEnemyState
{
    private string jumpAnim = "Jump Attack1";

    public void EnterState(EnemyAI enemy)
    {
        enemy.SetRootMotion(true);
        string[] jumpAnims = { "Jump Attack1", "Jump Attack2" };
        jumpAnim = jumpAnims[Random.Range(0, jumpAnims.Length)];
        Debug.Log($"BossJumpAttackState: 進入 {jumpAnim}");
        enemy.animator.Play(jumpAnim);
        enemy.Stop();
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
            Debug.Log($"BossJumpAttackState: {jumpAnim} 結束，切換到撤退");
            enemy.SwitchState(new RetreatState());
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        enemy.SetRootMotion(false);
        Debug.Log($"BossJumpAttackState: 退出 {jumpAnim}");
        enemy.Stop();
    }
} 