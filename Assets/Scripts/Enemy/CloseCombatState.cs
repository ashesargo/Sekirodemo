using UnityEngine;

public class CloseCombatState : IEnemyState
{
    private bool hasAttacked = false;
    private bool hasTriggeredAttack = false;
    private float timer = 0f;
    private float maxAttackTime = 3f; // 最多等待3秒，避免卡死

    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("CloseCombatState: 進入近戰狀態");
        enemy.Stop();
        
        // 播放攻擊動畫
        enemy.animator.SetTrigger("Attack");
        
        hasAttacked = false;
        hasTriggeredAttack = false;
        timer = 0f;
        enemy.canAutoAttack = false; // 攻擊時禁用自動攻擊
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        
        // 檢測攻擊動畫開始
        if (!hasTriggeredAttack && stateInfo.IsName("Attack"))
        {
            hasTriggeredAttack = true;
            Debug.Log("CloseCombatState: 近戰攻擊動畫開始播放");
        }
        
        // 等待攻擊動畫結束或超時
        if ((hasTriggeredAttack && stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 1.0f) || timer > maxAttackTime)
        {
            if (timer > maxAttackTime)
            {
                Debug.Log("CloseCombatState: 攻擊超時，強制切換到 RetreatState");
            }
            else
            {
                Debug.Log("CloseCombatState: 近戰攻擊完成，切換到 RetreatState");
            }
            enemy.SwitchState(new RetreatState());
        }
        
        // 調試信息
        if (enemy.showDebugInfo)
        {
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
            Debug.Log($"CloseCombatState: 近戰攻擊中，時間 = {timer:F2}/{maxAttackTime:F2}, 距離玩家 = {distanceToPlayer:F2}");
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log("CloseCombatState: 退出近戰狀態");
        enemy.Stop();
    }
} 