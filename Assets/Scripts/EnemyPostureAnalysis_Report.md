# 敵人架勢系統分析報告

## 最新更新 (2024年最新)

### 死亡動畫修復功能

**問題描述：**
敵人如果進入失衡狀態，玩家沒即時處決敵人，敵人恢復行動後，玩家再次攻擊敵人導致敵人血量歸零，敵人不會播放死亡動畫，ANIMATOR顯示卡在ATTACK動畫。

**原因分析：**
1. **動畫衝突**：當敵人從失衡狀態恢復後進入攻擊狀態，如果此時血量歸零，`DieState` 會嘗試播放死亡動畫，但與正在播放的攻擊動畫產生衝突
2. **動畫參數未重置**：進入死亡狀態時沒有重置其他動畫參數，導致動畫系統混亂
3. **失衡恢復邏輯問題**：當敵人從失衡狀態恢復時，沒有檢查血量是否歸零，直接切換到其他狀態

**解決方案：**
1. **修改 `DieState.cs` 的 `EnterState` 方法**：
   - 重置所有動畫參數，避免與其他動畫衝突
   - 停止當前動畫，使用 `animator.Play("Idle", 0, 0f)` 停止所有動畫層
   - 確保動畫清潔，避免與其他狀態的動畫產生衝突
2. **修改 `StaggerState.cs` 的 `UpdateState` 方法**：
   - 在失衡時間結束時優先檢查血量是否歸零
   - 如果血量歸零，直接進入死亡狀態，不切換到其他狀態

**測試腳本：**
- `DeathAnimationTest.cs` - 專門測試死亡動畫修復功能

**使用方法：**
1. 將 `DeathAnimationTest.cs` 添加到敵人物件上
2. 按 `S` 鍵測試失衡恢復後死亡
3. 按 `A` 鍵測試攻擊時死亡
4. 按 `D` 鍵測試失衡恢復時死亡
5. 按 `H` 鍵設置低血量
6. 按 `R` 鍵重置狀態

### 死亡優先級功能

**問題描述：**
用戶希望當敵人血量歸零時，無論架勢條是否滿，都要進入死亡狀態並播放死亡動畫。

**解決方案：**
1. **修改 `EnemyTest.TakeDamage()`**：
   - 在受傷和防禦後，優先檢查血量是否歸零
   - 如果血量歸零，直接進入死亡狀態，跳過架勢值檢查
   - 確保死亡檢查優先於架勢值檢查
2. **死亡優先級邏輯**：
   - 血量歸零 → 進入死亡狀態
   - 血量未歸零但架勢值滿 → 進入失衡狀態
   - 血量未歸零且架勢值未滿 → 播放受傷/防禦動畫

**測試腳本：**
- `DeathPriorityTest.cs` - 專門測試死亡優先級功能

**使用方法：**
1. 將 `DeathPriorityTest.cs` 添加到敵人物件上
2. 按 `L` 鍵測試低血量死亡
3. 按 `H` 鍵測試高架勢值
4. 按 `B` 鍵測試同時低血量和架勢值
5. 按 `R` 鍵重置狀態

### 直接進入失衡狀態功能

**問題描述：**
用戶希望修改失衡狀態邏輯，當敵人架勢值滿時直接播放失衡動畫，而不是先播放防禦或受擊動畫再播放失衡動畫。

**解決方案：**
1. **修改 `EnemyTest.TakeDamage()`**：
   - 在防禦成功後立即檢查架勢值，如果達到100%則直接進入失衡狀態
   - 在受傷後立即檢查架勢值，如果達到100%則直接進入失衡狀態
   - 跳過 `HitState` 的防禦或受擊動畫，直接播放失衡動畫
2. **修改 `HitState.cs`**：
   - 移除架勢值檢查邏輯，因為現在 `TakeDamage` 方法已經在架勢值滿時直接進入失衡狀態
   - 簡化狀態轉換邏輯

**測試腳本：**
- `StaggerDirectTest.cs` - 專門測試直接進入失衡狀態功能

**使用方法：**
1. 將 `StaggerDirectTest.cs` 添加到敵人物件上
2. 按 `P` 鍵設置高架勢值（85%）
3. 按 `D` 鍵測試防禦後直接失衡
4. 按 `H` 鍵測試受傷後直接失衡
5. 按 `R` 鍵重置狀態

### 失衡狀態免疫攻擊功能

**問題描述：**
用戶報告敵人在失衡狀態時仍然可以被玩家攻擊，希望敵人在失衡狀態時完全免疫攻擊，只能被處決。

**解決方案：**
1. **新增 `isStaggered` 標記**：在 `EnemyTest.cs` 中添加 `public bool isStaggered = false;` 標記
2. **修改 `StaggerState.cs`**：
   - 進入狀態時設置 `enemyTest.isStaggered = true`
   - 退出狀態時清除 `enemyTest.isStaggered = false`
   - 失衡結束後優先切換到追擊狀態（如果能看到玩家）
3. **修改 `EnemyTest.TakeDamage()`**：在傷害處理開始時檢查 `isStaggered` 標記，如果為 true 則直接返回，免疫所有傷害
4. **修改 `ExecutedState.cs`**：在進入被處決狀態時清除 `isStaggered` 標記，允許處決攻擊

**測試腳本：**
- `StaggerImmunityTest.cs` - 專門測試失衡狀態免疫攻擊功能

**使用方法：**
1. 將 `StaggerImmunityTest.cs` 添加到敵人物件上
2. 按 `S` 鍵讓敵人進入失衡狀態
3. 按 `A` 鍵測試攻擊敵人（應該被免疫）
4. 按 `E` 鍵測試處決（應該成功）
5. 按 `R` 鍵重置狀態

---

## 問題分析與解決方案

### 問題1：敵人從失衡恢復後死亡時動畫衝突

**原因分析：**
1. **動畫衝突**：當敵人從失衡狀態恢復後進入攻擊狀態，如果此時血量歸零，`DieState` 會嘗試播放死亡動畫，但與正在播放的攻擊動畫產生衝突
2. **動畫參數未重置**：進入死亡狀態時沒有重置其他動畫參數，導致動畫系統混亂
3. **失衡恢復邏輯問題**：當敵人從失衡狀態恢復時，沒有檢查血量是否歸零，直接切換到其他狀態

**解決方案：**
1. **修改 `DieState.cs` 的 `EnterState` 方法**：

```csharp
// 重置所有動畫參數，避免與其他動畫衝突
enemy.animator.ResetTrigger("Attack");
enemy.animator.ResetTrigger("Stagger");
enemy.animator.ResetTrigger("Executed");
enemy.animator.ResetTrigger("Hit");
enemy.animator.ResetTrigger("Defend");
enemy.animator.ResetTrigger("Run");
enemy.animator.ResetTrigger("Idle");

// 停止所有動畫層
enemy.animator.Play("Idle", 0, 0f);
```

2. **修改 `StaggerState.cs` 的 `UpdateState` 方法**：

```csharp
// 優先檢查是否死亡
if (enemyTest != null && enemyTest.GetCurrentHP() <= 0 && !enemyTest.isDead)
{
    enemyTest.isDead = true;
    Debug.Log($"[StaggerState] 敵人 {enemy.name} 失衡狀態結束時血量歸零，進入死亡狀態！");
    enemy.SwitchState(new DieState());
    return;
}
```

### 問題2：敵人血量歸零時沒有正確進入死亡狀態

**原因分析：**
1. **優先級問題**：架勢值檢查在死亡檢查之前，導致血量歸零時可能進入失衡狀態而不是死亡狀態
2. **邏輯順序**：`TakeDamage` 方法中架勢值檢查優先於死亡檢查

**解決方案：**
修改 `EnemyTest.TakeDamage()` 方法，讓死亡檢查優先於架勢值檢查：

```csharp
// 優先檢查是否死亡（死亡檢查優先於架勢值檢查）
if (GetCurrentHP() <= 0 && !isDead)
{
    isDead = true;
    Debug.Log($"[EnemyTest] 敵人 {gameObject.name} 血量歸零，進入死亡狀態！");
    if (ai != null)
        ai.SwitchState(new DieState());
    StartCoroutine(ReturnToPoolAfterDeath());
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
```

### 問題3：敵人防禦後架勢值滿但不會進入失衡狀態

**原因分析：**
1. **狀態衝突**：`HitState` 在防禦動畫結束後強制切換到 `ChaseState` 或 `IdleState`，覆蓋了 `OnPostureBroken` 觸發的 `StaggerState` 轉換
2. **時序問題**：`OnPostureBroken` 事件在防禦動畫播放期間觸發，但 `HitState` 在動畫結束後才檢查狀態轉換

**解決方案：**
修改 `EnemyTest.TakeDamage()` 方法，在增加架勢值後立即檢查是否達到100%：

```csharp
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
```

### 問題4：敵人被處決後沒有正確進入死亡狀態

**原因分析：**
1. **`DieState` 不完整**：只播放動畫，沒有正確設置死亡標記和禁用組件
2. **`ExecutedState` 不完整**：沒有確保敵人生命值歸零，可能導致 `EnemyTest.TakeDamage` 的死亡檢查失敗

**解決方案：**
1. **修改 `DieState.cs`**：
   ```csharp
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
   ```

2. **修改 `ExecutedState.cs`**：
   ```csharp
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
   ```

3. **新增方法**：
   - `HealthPostureSystem.cs` 新增 `SetHealthNormalized(float normalizedValue)`
   - `HealthPostureController.cs` 新增 `SetHealthValue(float normalizedValue)`

### 問題5：架勢值訪問權限問題

**錯誤信息：**
```
Assets\Scripts\Enemy\StaggerState.cs(53,34): error CS0122: 'HealthPostureController.SetPostureValue(float)' is inaccessible due to its protection level
```

**解決方案：**
1. 將 `HealthPostureController.SetPostureValue()` 改為 `public`
2. 新增 `public void ResetPosture()` 方法作為便捷方法

## 系統架構

### 核心組件

1. **`EnemyAI`** - 敵人AI主控制器
2. **`HealthPostureController`** - 生命值和架勢值管理
3. **`HealthPostureSystem`** - 架勢值計算核心
4. **`EnemyTest`** - 敵人傷害處理和狀態標記

### 狀態機

1. **`IdleState`** - 閒置狀態
2. **`ChaseState`** - 追擊狀態
3. **`HitState`** - 受傷/防禦狀態
4. **`StaggerState`** - 失衡狀態（新增）
5. **`ExecutedState`** - 被處決狀態（新增）
6. **`DieState`** - 死亡狀態

### 處決系統

1. **`ExecutionSystem`** - 玩家處決控制器
2. **處決條件**：
   - 敵人在失衡狀態
   - 玩家在攻擊範圍內
   - 敵人在玩家前方60度範圍內
   - 玩家按Q鍵

## 測試工具

### 測試腳本

1. **`StaggerStateTest.cs`** - 測試失衡狀態轉換
2. **`EnemyDeathTest.cs`** - 測試死亡狀態處理
3. **`StaggerImmunityTest.cs`** - 測試失衡狀態免疫攻擊
4. **`StaggerDirectTest.cs`** - 測試直接進入失衡狀態
5. **`DeathPriorityTest.cs`** - 測試死亡優先級
6. **`DeathAnimationTest.cs`** - 測試死亡動畫修復
7. **`EnemyPostureTest.cs`** - 測試架勢值增加邏輯

### 使用方法

1. 將對應的測試腳本添加到敵人物件上
2. 在遊戲中按指定按鍵進行測試
3. 查看Console輸出確認功能正常

## 動畫設置

### 敵人Animator設置

1. **新增參數**：
   - `Stagger` (Trigger) - 失衡動畫
   - `Executed` (Trigger) - 被處決動畫

2. **動畫狀態**：
   - Stagger State - 失衡動畫
   - Executed State - 被處決動畫

### 玩家Animator設置

1. **新增參數**：
   - `Execute` (Trigger) - 處決動畫

2. **動畫狀態**：
   - Execute State - 處決動畫

## 配置參數

### 失衡系統
- **失衡持續時間**：3秒
- **處決範圍**：2米
- **處決角度**：60度

### 架勢值系統
- **一般敵人防禦架勢增加**：20點
- **Boss防禦架勢增加**：可配置
- **受傷架勢增加**：30點

## 注意事項

1. **狀態優先級**：`DieState` > `StaggerState` > `HitState` > 其他狀態
2. **死亡處理**：確保所有組件都被正確禁用
3. **動畫同步**：使用 `normalizedTime` 檢查動畫播放進度
4. **事件清理**：在狀態退出時重置動畫參數
5. **直接失衡**：架勢值滿時直接進入失衡狀態，跳過防禦/受擊動畫
6. **死亡優先**：血量歸零時優先進入死亡狀態，無論架勢值如何
7. **動畫衝突處理**：進入死亡狀態時重置所有動畫參數，避免衝突

## 未來改進

1. **視覺反饋**：添加失衡狀態的視覺提示
2. **音效系統**：添加失衡和處決音效
3. **粒子效果**：添加處決特效
4. **UI提示**：當敵人可處決時顯示提示 