using System;
using UnityEngine;

// 角色生命值與架勢值資料
public class HealthPostureSystem
{
    public event EventHandler OnDead; // 當生命值歸零觸發
    public event EventHandler OnPostureBroken; // 架勢值集滿觸發
    public event EventHandler OnHealthChanged; // 生命值增減觸發
    public event EventHandler OnPostureChanged; // 架勢值增減觸發

    private int healthAmount; // 當前生命值
    private int healthAmountMax; // 最大生命值
    private int postureAmount; // 當前架勢值
    private int postureAmountMax; // 最大架勢值

    // 架勢自動恢復設定
    private float postureRecoveryInterval = 2f; // 架勢恢復間隔（秒）
    private int postureRecoveryAmount = 1; // 每次恢復的架勢值
    private float postureRecoveryTimer; // 架勢恢復計時器

    // 初始化生命與架勢系統
    public HealthPostureSystem(int healthAmountMax, int postureAmountMax)
    {
        this.healthAmountMax = healthAmountMax;
        this.postureAmountMax = postureAmountMax;
        this.healthAmount = healthAmountMax; // 初始生命值為最大值
        this.postureAmount = 0; // 初始架勢值為 0
        this.postureRecoveryTimer = 0f; // 初始化架勢恢復計時器
    }

    // 標準化生命值 (0~1)
    public float GetHealthNormalized()
    {
        return (float)healthAmount / healthAmountMax;
    }

    // 標準化架勢值 (0~1)
    public float GetPostureNormalized()
    {
        return (float)postureAmount / postureAmountMax;
    }

    // 設定架勢恢復參數
    public void SetPostureRecoverySettings(float interval, int amount)
    {
        postureRecoveryInterval = interval;
        postureRecoveryAmount = amount;
    }

    // 減少生命值
    public void HealthDamage(int damageAmount)
    {
        healthAmount -= damageAmount;
        // 確保生命值不會低於 0
        healthAmount = Mathf.Clamp(healthAmount, 0, healthAmountMax);

        // 觸發生命值改變事件
        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        // 檢查是否死亡
        if (healthAmount == 0)
        {
            OnDead?.Invoke(this, EventArgs.Empty);
        }
    }


    // 恢復生命值
    public void HealthHeal(int healAmount)
    {
        healthAmount += healAmount;
        // 確保生命值不會超過最大值
        healthAmount = Mathf.Clamp(healthAmount, 0, healthAmountMax);

        // 觸發生命值改變事件
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    // 增加架勢值
    public void PostureIncrease(int amount)
    {
        postureAmount += amount;
        postureAmount = Mathf.Clamp(postureAmount, 0, postureAmountMax);

        // 重置架勢恢復計時器
        postureRecoveryTimer = 0f;

        // 觸發架勢值增減事件
        OnPostureChanged?.Invoke(this, EventArgs.Empty);

        // 檢查架勢是否集滿
        if (postureAmount == postureAmountMax)
        {
            OnPostureBroken?.Invoke(this, EventArgs.Empty);
        }
    }

    // 減少架勢值
    public void PostureDecrease(int amount)
    {
        postureAmount -= amount;
        postureAmount = Mathf.Clamp(postureAmount, 0, postureAmountMax);

        // 觸發架勢值增減事件
        OnPostureChanged?.Invoke(this, EventArgs.Empty);
    }

    // 架勢自動恢復
    public void HandlePostureRecovery()
    {
        postureRecoveryTimer += Time.deltaTime;
        
        if (postureRecoveryTimer >= postureRecoveryInterval)
        {
            // 執行架勢恢復
            PostureDecrease(postureRecoveryAmount);
            // 計時器固定時間
            postureRecoveryTimer = postureRecoveryInterval;
        }
    }
}
