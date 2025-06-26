using System;
using UnityEngine; // 為了使用 Mathf.Clamp

/// <summary>
/// 管理角色生命值與架勢值的系統。
/// 這個類別不繼承 MonoBehaviour，純粹作為資料模型使用，可以在任何地方實例化。
/// </summary>
public class HealthPostureSystem {

    /// <summary>
    /// 當生命值歸零時觸發
    /// </summary>
    public event EventHandler OnDead;
    /// <summary>
    /// 當架勢值集滿時觸發
    /// </summary>
    public event EventHandler OnPostureBroken;
    /// <summary>
    /// 當生命值改變時觸發
    /// </summary>
    public event EventHandler OnHealthChanged;
    /// <summary>
    /// 當架勢值改變時觸發
    /// </summary>
    public event EventHandler OnPostureChanged;

    private int healthAmount; // 當前生命值
    private readonly int healthAmountMax; // 最大生命值
    private int postureAmount; // 當前架勢值
    private readonly int postureAmountMax; // 最大架勢值

    /// <summary>
    /// 建構函式，初始化生命與架勢系統。
    /// </summary>
    /// <param name="healthAmountMax">角色的最大生命值</param>
    /// <param name="postureAmountMax">角色的最大架勢值</param>
    public HealthPostureSystem(int healthAmountMax, int postureAmountMax) {
        this.healthAmountMax = healthAmountMax;
        this.postureAmountMax = postureAmountMax;
        this.healthAmount = healthAmountMax; // 初始生命值為最大值
        this.postureAmount = 0; // 初始架勢值為 0
    }

    /// <summary>
    /// 取得標準化後的生命值 (介於 0 到 1 之間)，方便 UI 顯示。
    /// </summary>
    /// <returns>標準化生命值</returns>
    public float GetHealthNormalized() {
        return (float)healthAmount / healthAmountMax;
    }

    /// <summary>
    /// 取得標準化後的架勢值 (介於 0 到 1 之間)，方便 UI 顯示。
    /// </summary>
    /// <returns>標準化架勢值</returns>
    public float GetPostureNormalized() {
        return (float)postureAmount / postureAmountMax;
    }

    /// <summary>
    /// 對角色造成生命傷害。
    /// </summary>
    /// <param name="damageAmount">傷害數值</param>
    public void HealthDamage(int damageAmount) {
        healthAmount -= damageAmount;
        // 使用 Clamp 確保生命值不會低於 0
        healthAmount = Mathf.Clamp(healthAmount, 0, healthAmountMax);

        // 觸發生命值改變事件
        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        // 檢查是否死亡
        if (healthAmount == 0) {
            OnDead?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 恢復角色生命值。
    /// </summary>
    /// <param name="healAmount">恢復數值</param>
    public void HealthHeal(int healAmount) {
        healthAmount += healAmount;
        // 使用 Clamp 確保生命值不會超過最大值
        healthAmount = Mathf.Clamp(healthAmount, 0, healthAmountMax);

        // 觸發生命值改變事件
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 增加架勢值。
    /// </summary>
    /// <param name="amount">增加數值</param>
    public void PostureIncrease(int amount) {
        postureAmount += amount;
        postureAmount = Mathf.Clamp(postureAmount, 0, postureAmountMax);

        // 觸發架勢值改變事件
        OnPostureChanged?.Invoke(this, EventArgs.Empty);

        // 檢查架勢是否被擊破
        if (postureAmount == postureAmountMax) {
            OnPostureBroken?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 減少架勢值（例如：隨時間恢復）。
    /// </summary>
    /// <param name="amount">減少數值</param>
    public void PostureDecrease(int amount) {
        postureAmount -= amount;
        postureAmount = Mathf.Clamp(postureAmount, 0, postureAmountMax);

        // 觸發架勢值改變事件
        OnPostureChanged?.Invoke(this, EventArgs.Empty);
    }
}
