using UnityEngine;

public class BossJumpAttackState : IEnemyState
{
    private string jumpAnim = "Jump Attack1";
    private bool hasStarted = false;
    private float timer = 0f;
    private float maxJumpTime = 3f;

    public void EnterState(EnemyAI enemy)
    {
        string[] jumpAnims = { "Jump Attack1", "Jump Attack2" };
        jumpAnim = jumpAnims[Random.Range(0, jumpAnims.Length)];
        Debug.Log($"BossJumpAttackState: 進入 {jumpAnim}");
        enemy.animator.Play(jumpAnim);
        hasStarted = false;
        timer = 0f;
        enemy.Stop();
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        if (!hasStarted && stateInfo.IsName(jumpAnim) && stateInfo.normalizedTime > 0.1f)
        {
            hasStarted = true;
            // 這裡可加特效、攻擊判定等
        }
        if ((hasStarted && stateInfo.IsName(jumpAnim) && stateInfo.normalizedTime >= 1.0f) || timer > maxJumpTime)
        {
            Debug.Log($"BossJumpAttackState: {jumpAnim} 結束，切換到撤退");
            enemy.SwitchState(new RetreatState());
        }
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position);
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log($"BossJumpAttackState: 退出 {jumpAnim}");
        enemy.Stop();
    }
} 