# 敵人防禦架勢值增加邏輯分析報告

## 🔍 問題描述

用戶反映：敵人因防禦增加的架勢值導致架勢條滿時不會進入失衡狀態，是否共用到玩家的成功parry不會讓架勢值達到上限的邏輯。

## 📋 系統邏輯分析

### 1. 架勢值增加邏輯

#### 敵人防禦時：
```csharp
// EnemyTest.cs
healthController.AddPosture(postureIncrease, false); // isParry = false
```

#### 玩家Parry時：
```csharp
// HealthPostureSystem.cs
if (isParry)
{
    int maxParryPosture = Mathf.RoundToInt(postureAmountMax * 0.99f);
    postureAmount = Mathf.Clamp(postureAmount, 0, maxParryPosture);
}
```

### 2. 架勢值達到100%的觸發條件

```csharp
// HealthPostureSystem.cs
// 檢查架勢是否集滿（只有非 Parry 時才會觸發）
if (postureAmount == postureAmountMax && !isParry)
{
    Debug.Log("[HealthPostureSystem] 架勢已集滿！");
    OnPostureBroken?.Invoke(this, EventArgs.Empty);
}
```

### 3. 架勢值恢復機制

```csharp
// HealthPostureUI.cs
// 架勢自動恢復（所有角色都正常自動恢復）
if (healthPostureSystem != null)
{
    healthPostureSystem.HandlePostureRecovery();
}

// HealthPostureSystem.cs
public void HandlePostureRecovery()
{
    postureRecoveryTimer += Time.deltaTime;
    
    if (postureRecoveryTimer >= postureRecoveryInterval) // 2秒
    {
        PostureDecrease(postureRecoveryAmount); // 減少1點
        postureRecoveryTimer = 1.7f;
    }
}
```

## ✅ 邏輯正確性確認

### 1. 敵人防禦架勢值增加邏輯是正確的

- **敵人防禦**：使用 `isParry = false`，可以達到100%架勢值
- **玩家Parry**：使用 `isParry = true`，限制在99%架勢值
- **設計意圖**：敵人防禦應該能夠累積架勢值到100%並進入失衡狀態

### 2. 架勢值恢復機制

- **恢復間隔**：每2秒
- **恢復量**：每次減少1點
- **影響**：可能阻止架勢值達到100%

## 🚨 發現的核心問題

### **敵人防禦動畫卡住失衡狀態**

#### 問題流程：
1. **架勢值達到100%** → 觸發 `OnPostureBroken` → 嘗試切換到 `StaggerState`
2. **敵人正在播放防禦動畫** → 處於 `HitState`
3. **防禦動畫結束** → `HitState` 強制切換到 `ChaseState` 或 `IdleState`
4. **結果**：`StaggerState` 被 `HitState` 覆蓋，敵人無法進入失衡狀態

#### 問題代碼：
```csharp
// HitState.cs - 原始邏輯
if (animEnd)
{
    // 動畫結束後，HitState會強制切換到其他狀態
    if (enemy.CanSeePlayer())
        enemy.SwitchState(new ChaseState()); // ← 覆蓋了StaggerState
    else
        enemy.SwitchState(new IdleState());
}
```

## 🔧 解決方案

### 修改 HitState 的狀態切換邏輯

```csharp
// HitState.cs - 修改後的邏輯
if (animEnd)
{
    // 檢查架勢值是否已滿，如果已滿則優先進入失衡狀態
    HealthPostureController healthController = enemy.GetComponent<HealthPostureController>();
    if (healthController != null)
    {
        float posturePercentage = healthController.GetPosturePercentage();
        Debug.Log($"[HitState] 檢查架勢值: {posturePercentage * 100:F1}%");
        
        if (posturePercentage >= 1.0f)
        {
            Debug.Log($"[HitState] 架勢值已滿，優先進入失衡狀態: {enemy.name}");
            enemy.SwitchState(new StaggerState());
            return;
        }
    }
    
    // 如果架勢值未滿，則按原邏輯切換狀態
    if (enemy.CanSeePlayer())
        enemy.SwitchState(new ChaseState());
    else
        enemy.SwitchState(new IdleState());
}
```

## 🧪 測試建議

### 1. 使用測試腳本
- **`EnemyPostureTest.cs`**：測試架勢值增加邏輯
- **`StaggerStateTest.cs`**：測試防禦後進入失衡狀態

### 2. 測試步驟
1. 將 `StaggerStateTest.cs` 添加到敵人物件
2. 按 `F` 測試防禦後進入失衡
3. 按 `M` 設置最大架勢值
4. 按 `R` 重置架勢值

### 3. 預期結果
- 敵人防禦動畫結束後，如果架勢值為100%，應該進入 `StaggerState`
- 播放失衡動畫
- 玩家可以按Q鍵處決敵人

## 💡 其他可能的問題原因

### 1. 架勢值增加量不足
- **一般敵人防禦**：`defendPostureIncrease = 20`
- **Boss防禦**：`bossDefendPostureIncrease = 15`
- **架勢值恢復**：每2秒減少1點

### 2. 架勢值恢復抵消
如果敵人防禦頻率不高，架勢值可能在達到100%之前就被恢復機制抵消了。

### 3. 防禦機率設置
- **一般敵人**：70%防禦機率
- **Boss**：80%防禦機率

## 📊 預期行為

### 正確的行為應該是：
1. 敵人防禦成功 → 架勢值增加
2. 架勢值達到100% → 觸發 `OnPostureBroken` 事件
3. 敵人進入 `StaggerState` → 播放失衡動畫
4. 玩家可以按Q鍵處決敵人

### 修復後的行為：
1. 敵人防禦成功 → 架勢值增加
2. 架勢值達到100% → 防禦動畫結束後檢查架勢值
3. 架勢值為100% → 優先進入 `StaggerState`
4. 播放失衡動畫 → 玩家可以按Q鍵處決敵人

## 🚨 新增問題：敵人被處決後沒有正確死亡

### 問題描述
用戶反映：敵人被處決後沒進入死亡狀態，還是會被玩家攻擊到。

### 問題分析
1. **`ExecutedState`** 只是播放處決動畫，沒有設置死亡標記
2. **`DieState`** 只是播放死亡動畫，沒有禁用碰撞器和AI組件
3. **敵人仍然可以被攻擊**，因為 `isDead` 標記沒有正確設置

### 解決方案

#### 1. 修改 `ExecutedState`
```csharp
public override void EnterState(EnemyAI enemy)
{
    // ... 其他邏輯 ...
    
    // 設置敵人為死亡狀態
    EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
    if (enemyTest != null)
    {
        enemyTest.isDead = true;
    }
    
    // 設置生命值為0（確保死亡）
    HealthPostureController healthController = enemy.GetComponent<HealthPostureController>();
    if (healthController != null)
    {
        healthController.SetHealthValue(0f);
    }
}
```

#### 2. 修改 `DieState`
```csharp
public override void EnterState(EnemyAI enemy)
{
    // 設置死亡標記
    EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
    if (enemyTest != null)
    {
        enemyTest.isDead = true;
    }
    
    // 禁用AI組件
    enemy.enabled = false;
    
    // 禁用碰撞器
    Collider enemyCollider = enemy.GetComponent<Collider>();
    if (enemyCollider != null)
    {
        enemyCollider.enabled = false;
    }
    
    // 禁用CharacterController
    CharacterController characterController = enemy.GetComponent<CharacterController>();
    if (characterController != null)
    {
        characterController.enabled = false;
    }
}
```

#### 3. 添加生命值設置方法
```csharp
// HealthPostureSystem.cs
public void SetHealthNormalized(float normalizedValue)
{
    healthAmount = Mathf.RoundToInt(healthAmountMax * Mathf.Clamp01(normalizedValue));
    OnHealthChanged?.Invoke(this, EventArgs.Empty);
    
    // 檢查是否死亡
    if (healthAmount == 0)
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }
}

// HealthPostureController.cs
public void SetHealthValue(float normalizedValue)
{
    if (healthPostureSystem != null)
    {
        healthPostureSystem.SetHealthNormalized(normalizedValue);
    }
}
```

### 測試方法
使用 `EnemyDeathTest.cs` 腳本進行驗證：
- 按 `E` 測試處決死亡
- 按 `D` 測試正常死亡
- 按 `R` 重置狀態

### 預期結果
- 敵人被處決後，`isDead` 標記為 `true`
- 碰撞器被禁用，無法被攻擊
- AI組件被禁用，不再移動或攻擊
- 播放死亡動畫後回歸物件池

## 🎯 結論

**主要問題**：
1. 敵人防禦動畫結束後，`HitState` 會強制切換到其他狀態，覆蓋了 `StaggerState`
2. 敵人被處決後沒有正確設置死亡狀態，仍然可以被攻擊

**解決方案**：
1. 修改 `HitState` 的狀態切換邏輯，在動畫結束後優先檢查架勢值
2. 修改 `ExecutedState` 和 `DieState`，確保敵人死亡時正確設置標記和禁用組件
3. 添加生命值設置方法，支持直接設置生命值百分比

**測試方法**：
- 使用 `StaggerStateTest.cs` 測試失衡狀態
- 使用 `EnemyDeathTest.cs` 測試死亡狀態

這些修復確保了敵人防禦時架勢值達到100%後能夠正確進入失衡狀態，以及敵人被處決後能夠正確死亡並無法被攻擊。 