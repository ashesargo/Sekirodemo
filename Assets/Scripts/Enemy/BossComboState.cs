using UnityEngine;

public class BossComboState : IEnemyState
{
    private bool hasTriggeredCombo = false;
    private string currentComboAnimation = "";
    private int currentAnimationIndex = 0; // 當前播放的動畫索引
    private string[] comboSequence = null; // 連擊序列動畫
    private float timer = 0f;
    private float maxComboTime = 15f; // 最多等待15秒，避免卡死

    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("BossComboState: Boss進入連擊攻擊狀態");
        enemy.Stop();
        
        // 獲取BossAI組件
        BossAI bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null)
        {
            // 調試：顯示當前連擊狀態
            bossAI.DebugComboStatus();
            
            // 獲取下一個連擊動畫
            currentComboAnimation = bossAI.GetNextComboAnimation();
            
            // 設定連擊序列動畫
            SetupComboSequence(enemy, currentComboAnimation);
            
            // 播放第一個動畫
            if (comboSequence != null && comboSequence.Length > 0)
            {
                // 播放第一個動畫（如Combo1）
                enemy.animator.Play(comboSequence[0]);
                Debug.Log($"BossComboState: 開始播放連擊序列 {currentComboAnimation}, 第一個動畫: {comboSequence[0]}");
            }
            else
            {
                // 如果沒有對應的動畫，使用Attack動畫
                enemy.animator.SetTrigger("Attack");
                Debug.Log($"BossComboState: 沒有找到 {currentComboAnimation} 動畫，使用Attack動畫");
                currentComboAnimation = "Attack";
            }
        }
        else
        {
            // 如果不是BossAI，使用普通Attack動畫
            enemy.animator.SetTrigger("Attack");
            Debug.Log("BossComboState: 不是BossAI，使用普通Attack動畫");
        }
        
        hasTriggeredCombo = false;
        currentAnimationIndex = 0;
        timer = 0f;
        enemy.canAutoAttack = false; // 攻擊時禁用自動攻擊
    }
    
    // 設定連擊序列動畫
    private void SetupComboSequence(EnemyAI enemy, string comboName)
    {
        // 根據連擊名稱設定對應的動畫序列
        switch (comboName)
        {
            case "Combo1":
                // Combo1: 先播放Combo1，然後是6個子動畫
                comboSequence = new string[] { 
                    "Combo1", "Combo1_1", "Combo1_2", "Combo1_3", 
                    "Combo1_4", "Combo1_5", "Combo1_6" 
                };
                break;
            case "Combo2":
                // Combo2: 先播放Combo2，然後是子動畫序列
                comboSequence = new string[] { 
                    "Combo2", "Combo2_1", "Combo2_2", "Combo2_3" 
                };
                break;
            case "Combo3":
                // Combo3: 先播放Combo3，然後是子動畫序列
                comboSequence = new string[] { 
                    "Combo3", "Combo3_1", "Combo3_2", "Combo3_3" 
                };
                break;
            default:
                // 如果沒有找到對應的序列，使用單一動畫
                if (enemy.animator.HasState(0, Animator.StringToHash(comboName)))
                {
                    comboSequence = new string[] { comboName };
                }
                else
                {
                    comboSequence = new string[] { "Attack" };
                }
                break;
        }
        
        // 檢查動畫是否存在，如果不存在則使用Attack
        for (int i = 0; i < comboSequence.Length; i++)
        {
            if (!enemy.animator.HasState(0, Animator.StringToHash(comboSequence[i])))
            {
                Debug.LogWarning($"BossComboState: 找不到動畫 {comboSequence[i]}，使用Attack代替");
                comboSequence[i] = "Attack";
            }
        }
        
        Debug.Log($"BossComboState: 設定連擊序列 {comboName}, 動畫數量: {comboSequence.Length}");
    }
    
    // 根據當前combo獲取對應的持續時間
    private float GetCurrentComboDuration(EnemyAI enemy)
    {
        BossAI bossAI = enemy.GetComponent<BossAI>();
        if (bossAI != null)
        {
            return bossAI.GetComboDuration(currentComboAnimation);
        }
        return maxComboTime; // 預設值
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        
        // 讓Boss一直面向玩家（改為慢慢轉向）
        if (enemy.player != null)
        {
            enemy.SmoothLookAt(enemy.player.position); // 使用 Inspector 可調整的 lookAtTurnSpeed
        }
        
        // 檢測連擊動畫開始
        if (!hasTriggeredCombo && comboSequence != null && currentAnimationIndex < comboSequence.Length)
        {
            if (stateInfo.IsName(comboSequence[currentAnimationIndex]))
            {
                hasTriggeredCombo = true;
                Debug.Log($"BossComboState: 連擊動畫開始播放: {comboSequence[currentAnimationIndex]}");
            }
        }
        
        // 等待當前動畫結束，然後播放下一個動畫
        if (hasTriggeredCombo && comboSequence != null && currentAnimationIndex < comboSequence.Length)
        {
            if (stateInfo.IsName(comboSequence[currentAnimationIndex]) && stateInfo.normalizedTime >= 1.0f)
            {
                // 當前動畫完成，播放下一個動畫
                currentAnimationIndex++;
                
                if (currentAnimationIndex < comboSequence.Length)
                {
                    // 還有更多動畫要播放
                    enemy.animator.Play(comboSequence[currentAnimationIndex]);
                    hasTriggeredCombo = false; // 重置觸發標誌，等待下一個動畫開始
                    Debug.Log($"BossComboState: 播放下一個動畫: {comboSequence[currentAnimationIndex]} ({currentAnimationIndex + 1}/{comboSequence.Length})");
                }
                else
                {
                    // 所有動畫都播放完成，進入撤退狀態
                    Debug.Log($"BossComboState: 所有連擊動畫播放完成 ({comboSequence.Length} 個動畫)，進入撤退狀態");
                    
                    // 增加連擊計數
                    BossAI bossAI = enemy.GetComponent<BossAI>();
                    if (bossAI != null)
                    {
                        bossAI.IncrementComboCount();
                    }
                    
                    // 切換到撤退狀態
                    enemy.SwitchState(new RetreatState());
                    return;
                }
            }
        }
        
        // 根據當前combo設定超時時間
        float currentMaxTime = GetCurrentComboDuration(enemy);
        
        // 超時保護
        if (timer > currentMaxTime)
        {
            Debug.Log($"BossComboState: 連擊超時 ({timer:F2}s > {currentMaxTime}s)，強制切換到撤退狀態");
            
            // 增加連擊計數
            BossAI bossAI = enemy.GetComponent<BossAI>();
            if (bossAI != null)
            {
                bossAI.IncrementComboCount();
            }
            
            // 切換到撤退狀態
            enemy.SwitchState(new RetreatState());
            return;
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log("BossComboState: Boss退出連擊攻擊狀態");
        enemy.Stop();
    }
} 