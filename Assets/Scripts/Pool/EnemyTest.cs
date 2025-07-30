using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敵人測試
public class EnemyTest : MonoBehaviour
{
    public static int maxHP = 100;
    private Animator _animator;
    public bool isDead = false;
    
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
        Debug.Log($"TakeDamage called! HP: {GetCurrentHP()}, isDead: {isDead}, state: {GetComponent<EnemyAI>()?.CurrentState?.GetType().Name}");
        if (isDead) return; // 死亡後不再受傷

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
                healthController.AddPosture(postureIncrease, false); // 防禦時增加架勢值，不是Parry
                Debug.Log($"{enemyType}防禦時增加架勢值 {postureIncrease} 點！");
            }
            
            // 設置HitState應該播放防禦動畫
            HitState.shouldDefend = true;
            // 即使防禦也要進入HitState來播放防禦動畫
            if (ai != null)
                ai.SwitchState(new HitState());
            return;
        }

        string enemyType2 = bossAI != null ? "Boss" : "一般敵人";
        Debug.Log($"{enemyType2}受傷！({(1f - currentDefendChance) * 100:F0}%機率)");
        // 設置HitState應該播放受傷動畫
        HitState.shouldDefend = false;

        // 使用 HealthPostureController 處理傷害和架勢值
        if (healthController != null)
        {
            // 受傷時扣血並增加架勢值
            healthController.TakeDamage(damage, PlayerStatus.HitState.Hit);
            
            // 根據敵人類型決定額外的架勢值增加量
            int additionalPostureIncrease = bossAI != null ? bossAI.bossHitPostureIncrease : hitPostureIncrease;
            healthController.AddPosture(additionalPostureIncrease, false);
            Debug.Log($"{enemyType2}受傷時增加架勢值 {additionalPostureIncrease} 點！");
        }
        // 檢查是否死亡
        if (GetCurrentHP() <= 0 && !isDead)
        {
            isDead = true;
            if (ai != null)
                ai.SwitchState(new DieState());
            StartCoroutine(ReturnToPoolAfterDeath());
        }
        else if (GetCurrentHP() > 0)
        {
            // 受傷但未死亡
            if (ai != null)
                ai.SwitchState(new HitState());
        }
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

    // 敵人死亡後回歸物件池
    private IEnumerator ReturnToPoolAfterDeath()
    {
        // 等待死亡動畫播放完畢（2秒）
        yield return new WaitForSeconds(2f);

        // 回歸物件池
        ObjectPool objectPool = ObjectPool.Instance;
        if (objectPool != null)
        {
            objectPool.ReturnPooledObject(gameObject);
        }
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

