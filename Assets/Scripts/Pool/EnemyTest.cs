using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敵人測試
public class EnemyTest : MonoBehaviour
{
    public static int maxHP = 100;
    private Animator _animator;
    public bool isDead = false;
    
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

        // 決定是否受傷（70%防禦，30%受傷）
        float randomValue = Random.value;
        bool shouldTakeDamage = randomValue >= 0.7f; // 30%機率受傷

        if (!shouldTakeDamage)
        {
            Debug.Log($"敵人成功防禦，不受傷害！(70%機率)");
            // 設置HitState應該播放防禦動畫
            HitState.shouldDefend = true;
            // 即使防禦也要進入HitState來播放防禦動畫
            if (ai != null)
                ai.SwitchState(new HitState());
            return;
        }

        Debug.Log($"敵人受傷！(30%機率)");
        // 設置HitState應該播放受傷動畫
        HitState.shouldDefend = false;

        // 使用 HealthPostureController 處理傷害
        if (healthController != null)
        {
            healthController.TakeDamage(damage,0);
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

