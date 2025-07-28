using UnityEngine;

public class StrongEnemyComboState : BaseEnemyState
{
    private int currentComboCount = 0;
    private bool isFirstComboSequence = true; // 是否是首次完整的Combo123序列
    private string[] comboAnimations = { "Combo1", "Combo2", "Combo3" };
    private bool hasStartedCombo = false;
    private string currentComboAnimation = "";
    private StrongEnemyAI strongEnemyAI = null;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("StrongEnemyComboState: 進入強力小兵Combo攻擊");
        Debug.Log($"StrongEnemyComboState: 敵人類型 = {enemy.GetType().Name}");
        
        // 獲取StrongEnemyAI組件
        strongEnemyAI = enemy.GetComponent<StrongEnemyAI>();
        
        enemy.Stop();
        currentComboCount = 0;
        isFirstComboSequence = !strongEnemyAI.hasDoneFirstCombo; // 根據是否已完成首次Combo來決定
        hasStartedCombo = false;
        enemy.canAutoAttack = false;
        
        // 開始第一個Combo
        PlayComboAnimation(enemy);
    }

    public override void UpdateState(EnemyAI enemy)
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        
        // 面向玩家
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position, turnSpeed: 8f);
        }
        
        // 檢查動畫是否回到Idle狀態（表示Combo動畫結束）
        if (stateInfo.IsName("Idle"))
        {
            currentComboCount++;
            
            if (isFirstComboSequence)
            {
                // 首次完整序列：Combo1 -> Combo2 -> Combo3
                if (currentComboCount < 3)
                {
                    PlayComboAnimation(enemy);
                }
                else
                {
                    // 首次完整序列結束，標記已完成並切換到撤退
                    if (strongEnemyAI != null)
                    {
                        strongEnemyAI.hasDoneFirstCombo = true;
                    }
                    Debug.Log("StrongEnemyComboState: 首次完整Combo序列結束，切換到 RetreatState");
                    enemy.SwitchState(new RetreatState());
                }
            }
            else
            {
                // 非首次：只播放一個隨機Combo後結束
                Debug.Log("StrongEnemyComboState: 單次Combo結束，切換到 RetreatState");
                enemy.SwitchState(new RetreatState());
            }
        }
    }

    private void PlayComboAnimation(EnemyAI enemy)
    {
        // 獲取下一個Combo動畫
        currentComboAnimation = GetNextComboAnimation();
        
        // 檢查動畫是否存在
        if (enemy.animator.HasState(0, Animator.StringToHash(currentComboAnimation)))
        {
            // 直接播放Combo動畫，不使用Trigger
            enemy.animator.Play(currentComboAnimation);
            hasStartedCombo = true;
            Debug.Log($"StrongEnemyComboState: 播放 {currentComboAnimation} (第 {currentComboCount + 1} 次Combo)");
        }
        else
        {
            Debug.LogError($"StrongEnemyComboState: 找不到動畫 {currentComboAnimation}，使用預設攻擊");
            // 如果找不到Combo動畫，使用預設攻擊
            enemy.animator.SetTrigger("Attack");
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        enemy.canAutoAttack = true;
        Debug.Log("StrongEnemyComboState: 退出Combo攻擊狀態");
    }

    // 獲取下一個Combo動畫
    private string GetNextComboAnimation()
    {
        if (isFirstComboSequence)
        {
            // 首次完整序列：按順序播放Combo1、Combo2、Combo3
            string nextAnimation = comboAnimations[currentComboCount];
            Debug.Log($"StrongEnemyComboState: 首次完整序列 - 按順序播放連擊 {currentComboCount + 1}, 動畫: {nextAnimation}");
            return nextAnimation;
        }
        else
        {
            // 非首次：隨機選擇一個Combo
            int randomIndex = Random.Range(0, comboAnimations.Length);
            string randomAnimation = comboAnimations[randomIndex];
            Debug.Log($"StrongEnemyComboState: 非首次 - 隨機選擇連擊動畫, 索引: {randomIndex}, 動畫: {randomAnimation}");
            return randomAnimation;
        }
    }
    


    public override bool ShouldUseRootMotion() => true;
} 