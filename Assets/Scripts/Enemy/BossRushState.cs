using UnityEngine;

public class BossRushState : IEnemyState
{
    private Vector3 rushDirection;

    public void EnterState(EnemyAI enemy)
    {
        enemy.SetRootMotion(true);
        Debug.Log("BossRushState: 進入突進狀態");
        enemy.animator.Play("Rush"); // 直接播放Rush動畫
        enemy.Stop();
        // 標記剛做過突進
        var bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null)
            bossAI.hasJustRangedOrRushed = true;
        // 計算突進方向（朝向玩家）
        if (enemy.player != null)
        {
            rushDirection = (enemy.player.position - enemy.transform.position).normalized;
            rushDirection.y = 0f;
        }
        else
        {
            rushDirection = enemy.transform.forward;
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
        // Rush位移完全交給Root Motion，不再用程式碼推動
        // 只要動畫已經自動Transition到Idle（或ComboEnd等），就切換狀態
        if (stateInfo.IsName("Idle"))
        {
            enemy.SwitchState(new BossChaseState());
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        enemy.SetRootMotion(false);
        Debug.Log("BossRushState: 退出突進狀態");
        enemy.Stop();
    }
} 