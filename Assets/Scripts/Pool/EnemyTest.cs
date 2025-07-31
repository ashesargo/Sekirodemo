using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敵人測試
public class EnemyTest : MonoBehaviour
{
    public static int maxHP = 100;
    private Animator _animator;
    public bool isDead = false;
    public bool isStaggered = false; // 新增：失衡狀態標記
    
    [Header("防禦設定")]
    [Range(0f, 1f)]
    public float defendChance = 0.7f; // 防禦機率，可在Inspector中調整
    
    [Header("架勢值設定")]
    [Range(0, 100)]
    public int defendPostureIncrease = 20; // 防禦時增加的架勢值
    [Range(0, 100)]
    public int hitPostureIncrease = 30; // 受傷時增加的架勢值

    private HealthPostureController healthController;  // 引用生命值控制器

    void Start()
    {
        _animator = GetComponent<Animator>();
        
        // 獲取 HealthPostureController 組件
        healthController = GetComponent<HealthPostureController>();
        
        // 如果沒有 HealthPostureController，創建一個
        if (healthController == null)
        {
            healthController = gameObject.AddComponent<HealthPostureController>();
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"[EnemyTest] {gameObject.name} 接收到傷害: {damage} - 時間: {Time.time:F3}");
        Debug.Log($"[EnemyTest] 當前狀態 - HP: {GetCurrentHP()}, isDead: {isDead}, isStaggered: {isStaggered}, state: {GetComponent<EnemyAI>()?.CurrentState?.GetType().Name}");
        Debug.Log($"[EnemyTest] 調用堆疊: {System.Environment.StackTrace}");
        
        if (isDead) return; // 死亡後不再受傷
        
        // 檢查是否處於失衡狀態，如果是則免疫所有傷害
        if (isStaggered)
        {
            Debug.Log($"[EnemyTest] 敵人 {gameObject.name} 處於失衡狀態，免疫所有傷害！");
            return;
        }

        // 檢查是否正在防禦
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null && ai.CurrentState is HitState hitState)
        {
            if (hitState.IsDefending())
            {
                Debug.Log($"敵人正在防禦，不受傷害！");
                return; // 防禦時不受傷
            }
        }

        // 決定是否受傷（使用Inspector中設定的防禦機率）
        float randomValue = Random.value;
        float currentDefendChance = defendChance; // 預設使用一般敵人的防禦機率
        
        // 檢查是否為Boss，如果是則使用Boss的防禦機率和架勢值設定
        BossAI bossAI = GetComponent<BossAI>();
        if (bossAI != null)
        {
            currentDefendChance = bossAI.bossDefendChance;
        }
        
        bool shouldTakeDamage = randomValue >= currentDefendChance; // 使用對應的防禦機率

        if (!shouldTakeDamage)
        {
            string enemyType = bossAI != null ? "Boss" : "一般敵人";
            Debug.Log($"{enemyType}成功防禦，不受傷害！({currentDefendChance * 100:F0}%機率)");
            
            // 防禦時增加架勢值（使用HealthPostureController）
            if (healthController != null)
            {
                // 根據敵人類型決定架勢值增加量
                int postureIncrease = bossAI != null ? bossAI.bossDefendPostureIncrease : defendPostureIncrease;
                Debug.Log($"[EnemyTest] {enemyType} 防禦成功！架勢值增加來源: EnemyTest.cs (敵人防禦)");
                Debug.Log($"[EnemyTest] 增加數值: {postureIncrease}, 是否為Parry: false");
                healthController.AddPosture(postureIncrease, false); // 防禦時增加架勢值，不是Parry
                Debug.Log($"{enemyType}防禦時增加架勢值 {postureIncrease} 點！");
                
                // 優先檢查是否死亡（死亡檢查優先於架勢值檢查）
                if (GetCurrentHP() <= 0 && !isDead)
                {
                    isDead = true;
                    Debug.Log($"[EnemyTest] 敵人 {gameObject.name} 血量歸零，進入死亡狀態！");
                    if (ai != null)
                        ai.SwitchState(new DieState());
                    // 注意：物件池回收現在在DieState中處理，不需要在這裡調用ReturnToPoolAfterDeath
                    return;
                }
                
                // 檢查架勢值是否已滿，如果已滿則直接進入失衡狀態
                float posturePercentage = healthController.GetPosturePercentage();
                if (posturePercentage >= 1.0f)
                {
                    Debug.Log($"[EnemyTest] 架勢值已滿 ({posturePercentage * 100:F1}%)，直接進入失衡狀態！");
                    if (ai != null)
                    {
                        ai.SwitchState(new StaggerState());
                    }
                    return;
                }
            }
            
            // 架勢值未滿，播放防禦動畫
            HitState.shouldDefend = true;
            if (ai != null)
                ai.SwitchState(new HitState());
            
            // 觸發敵人防禦成功事件（用於模糊效果）
            Vector3 defendPosition = transform.position + transform.forward * 2f;
            if (ai.OnEnemyGuardSuccess != null)
            {
                ai.OnEnemyGuardSuccess.Invoke(defendPosition);
            }
            return;
        }

        string enemyType2 = bossAI != null ? "Boss" : "一般敵人";
        Debug.Log($"{enemyType2}受傷！({(1f - currentDefendChance) * 100:F0}%機率)");

        // 使用 HealthPostureController 處理傷害和架勢值
        if (healthController != null)
        {
            // 受傷時扣血並增加架勢值
            healthController.TakeDamage(damage, PlayerStatus.HitState.Hit);
            
            // 注意：HealthPostureController.TakeDamage 已經會根據 HitState.Hit 增加架勢值
            // 不需要再額外調用 AddPosture，避免重複增加架勢值
            Debug.Log($"[EnemyTest] {enemyType2} 受傷！架勢值已通過 HealthPostureController.TakeDamage 增加");
            
            // 優先檢查是否死亡（死亡檢查優先於架勢值檢查）
            if (GetCurrentHP() <= 0 && !isDead)
            {
                isDead = true;
                Debug.Log($"[EnemyTest] 敵人 {gameObject.name} 血量歸零，進入死亡狀態！");
                if (ai != null)
                    ai.SwitchState(new DieState());
                // 注意：物件池回收現在在DieState中處理，不需要在這裡調用ReturnToPoolAfterDeath
                return;
            }
            
            // 檢查架勢值是否已滿，如果已滿則直接進入失衡狀態
            float posturePercentage = healthController.GetPosturePercentage();
            if (posturePercentage >= 1.0f)
            {
                Debug.Log($"[EnemyTest] 架勢值已滿 ({posturePercentage * 100:F1}%)，直接進入失衡狀態！");
                if (ai != null)
                {
                    ai.SwitchState(new StaggerState());
                }
                return;
            }
        }
        
        // 架勢值未滿，播放受傷動畫
        HitState.shouldDefend = false;
        
        // 觸發敵人受傷事件（用於模糊效果）
        Vector3 hitPosition = transform.position + transform.forward * 2f;
        if (ai != null && ai.OnEnemyHitOccurred != null)
        {
            ai.OnEnemyHitOccurred.Invoke(hitPosition);
        }
        
        // 受傷但未死亡，且架勢值未滿
        if (ai != null)
            ai.SwitchState(new HitState());
    }

    // 獲取當前生命值
    public int GetCurrentHP()
    {
        if (healthController != null)
        {
            return Mathf.RoundToInt(healthController.GetHealthPercentage() * maxHP);
        }
        return 0;
    }



    // 移除敵人控制（架勢被打破時）
    public void RemoveControl()
    {
        // 禁用敵人 AI
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.enabled = false;
        }
        
        // 播放失衡動畫
        if (_animator != null)
        {
            _animator.SetTrigger("Stagger");
        }
    }

    // 恢復敵人控制
    public void RestoreControl()
    {
        // 重新啟用敵人 AI
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.enabled = true;
        }
    }
}

