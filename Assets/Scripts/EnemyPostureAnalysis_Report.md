# 敵人架勢系統分析報告

## 最新更新 (2024年最新)

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

### 問題1：敵人防禦後架勢值滿但不會進入失衡狀態

**原因分析：**
1. **狀態衝突**：`HitState` 在防禦動畫結束後強制切換到 `ChaseState` 或 `IdleState`，覆蓋了 `OnPostureBroken` 觸發的 `StaggerState` 轉換
2. **時序問題**：`OnPostureBroken` 事件在防禦動畫播放期間觸發，但 `HitState` 在動畫結束後才檢查狀態轉換

**解決方案：**
修改 `HitState.cs` 的 `UpdateState` 方法，在動畫結束後優先檢查架勢值：

```csharp
// 檢查架勢值是否已滿，如果已滿則優先進入失衡狀態
HealthPostureController healthController = enemy.GetComponent<HealthPostureController>();
if (healthController != null)
{
    float posturePercentage = healthController.GetPosturePercentage();
    if (posturePercentage >= 1.0f)
    {
        enemy.SwitchState(new StaggerState());
        return;
    }
}
```

### 問題2：敵人被處決後沒有正確進入死亡狀態

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

### 問題3：架勢值訪問權限問題

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
4. **`EnemyPostureTest.cs`** - 測試架勢值增加邏輯

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

1. **狀態優先級**：`StaggerState` > `HitState` > 其他狀態
2. **死亡處理**：確保所有組件都被正確禁用
3. **動畫同步**：使用 `normalizedTime` 檢查動畫播放進度
4. **事件清理**：在狀態退出時重置動畫參數

## 未來改進

1. **視覺反饋**：添加失衡狀態的視覺提示
2. **音效系統**：添加失衡和處決音效
3. **粒子效果**：添加處決特效
4. **UI提示**：當敵人可處決時顯示提示 