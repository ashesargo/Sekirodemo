using UnityEngine;

public class BossRushState : IEnemyState
{
    private bool hasRushed = false;
    private float rushSpeed = 20f; // 突進速度，可依需求調整
    private float rushDuration = 1.0f; // 突進持續時間
    private float timer = 0f;
    private Vector3 rushDirection;

    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("BossRushState: 進入突進狀態");
        enemy.animator.Play("Rush"); // 直接播放Rush動畫
        hasRushed = false;
        timer = 0f;
        enemy.Stop();
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
        timer += Time.deltaTime;
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        // 動畫開始時觸發突進
        if (!hasRushed && stateInfo.IsName("Rush") && stateInfo.normalizedTime > 0.1f)
        {
            hasRushed = true;
            // Boss突進移動
            enemy.velocity = rushDirection * rushSpeed;
        }
        // 持續突進
        if (hasRushed && timer < rushDuration)
        {
            enemy.transform.position += rushDirection * rushSpeed * Time.deltaTime;
        }
        // 動畫結束或突進時間到，切回追擊
        if ((hasRushed && timer >= rushDuration) || (stateInfo.IsName("Rush") && stateInfo.normalizedTime >= 1.0f))
        {
            enemy.SwitchState(new BossChaseState());
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log("BossRushState: 退出突進狀態");
        enemy.Stop();
    }
} 