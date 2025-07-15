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

        // 使用 HealthPostureController 處理傷害
        if (healthController != null)
        {
            healthController.TakeDamage(damage);
        }

        // 檢查是否死亡
        if (GetCurrentHP() <= 0 && !isDead)
        {
            isDead = true;
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null)
                ai.SwitchState(new DieState());
            StartCoroutine(ReturnToPoolAfterDeath());
        }
        else if (GetCurrentHP() > 0)
        {
            // 受傷但未死亡
            EnemyAI ai = GetComponent<EnemyAI>();
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
}

