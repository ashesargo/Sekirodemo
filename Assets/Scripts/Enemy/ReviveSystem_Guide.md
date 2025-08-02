# Boss與Elite敵人復活系統指南

## 概述

這個系統允許boss和elite敵人在被處決後復活，需要玩家多次處決才能真正擊敗敵人。

## 系統架構

### 核心組件

1. **ExecutedState** - 處決狀態
   - 檢查敵人是否還有復活次數
   - 如果有復活次數，進入ReviveState
   - 如果沒有復活次數，進入DieState

2. **ReviveState** - 復活狀態
   - 播放復活動畫
   - 重置生命值和架勢值
   - 減少復活次數
   - 復活完成後進入適當的戰鬥狀態

3. **HealthPostureController** - 生命值控制器
   - 管理復活次數 (`live` 屬性)
   - 提供復活特效播放功能
   - 重置敵人狀態

### 復活流程

1. **處決階段**
   - 敵人被處決，進入ExecutedState
   - 播放被處決動畫

2. **復活檢查**
   - 動畫播放完畢後檢查復活次數
   - 如果 `live > 0`，進入ReviveState
   - 如果 `live <= 0`，進入DieState

3. **復活階段**
   - 播放復活動畫（使用Idle動畫或Revive動畫）
   - 重置生命值和架勢值
   - 減少復活次數
   - 播放復活特效

4. **狀態恢復**
   - Boss復活後進入BossChaseState
   - 一般敵人復活後進入ChaseState或AlertState
   - 重新啟用所有組件和能力

## 設置方法

### 1. 敵人Prefab設置

在敵人的HealthPostureController組件中設置：
- `maxLive`: 最大復活次數
- `live`: 當前復活次數（通常等於maxLive）

### 2. 動畫設置

#### 可選的Revive動畫
如果敵人有專門的復活動畫：
1. 在Animator中添加"Revive"狀態
2. 設置Revive動畫片段
3. 添加Revive Trigger參數

#### 使用Idle動畫作為復活動畫
如果沒有專門的復活動畫，系統會自動使用Idle動畫作為復活動畫。

### 3. 復活特效設置

在HealthPostureController中設置：
- `reviveEffectPrefab`: 復活特效Prefab
- `reviveEffectPosition`: 特效生成位置

## 測試

### 使用測試腳本

1. 將 `ReviveSystemTest.cs` 添加到敵人物件上
2. 設置測試參數：
   - `testLiveCount`: 測試用的復活次數
   - `testExecuteKey`: 測試處決的按鍵
   - `testReviveKey`: 測試復活的按鍵
   - `resetKey`: 重置狀態的按鍵

3. 測試按鍵：
   - `E`: 測試處決
   - `D`: 測試死亡
   - `R`: 測試復活
   - `T`: 重置狀態

### 手動測試

1. 設置敵人的復活次數
2. 讓玩家處決敵人
3. 觀察敵人是否正確復活
4. 檢查復活後是否進入正確的戰鬥狀態

## 注意事項

### 動畫設置
- 如果沒有Revive動畫，系統會使用Idle動畫
- 確保動畫控制器中有Idle狀態
- 復活動畫播放到一半就會結束復活流程

### 狀態轉換
- Boss復活後直接進入追擊狀態
- 一般敵人復活後會檢查是否能看到玩家
- 如果看不到玩家，會進入警戒狀態

### 組件重置
- 復活時會重新啟用所有必要的組件
- 包括AI、碰撞器、CharacterController等
- 確保敵人能正常行動

## 故障排除

### 常見問題

1. **敵人復活後不動**
   - 檢查AI組件是否被正確啟用
   - 確認敵人進入了正確的狀態

2. **復活動畫不播放**
   - 檢查是否有Idle動畫
   - 確認動畫控制器設置正確

3. **復活次數不減少**
   - 檢查HealthPostureController的live屬性
   - 確認ReviveState正確執行了復活邏輯

### 調試信息

系統會輸出詳細的調試信息：
- `[ExecutedState]` - 處決狀態相關信息
- `[ReviveState]` - 復活狀態相關信息
- `[ReviveSystemTest]` - 測試腳本相關信息

## 擴展功能

### 自定義復活動畫
可以修改ReviveState來支援更多種類的復活動畫：
```csharp
// 在ReviveState中添加更多動畫檢查
if (enemy.animator.HasState(0, Animator.StringToHash("CustomRevive")))
{
    enemy.animator.SetTrigger("CustomRevive");
}
```

### 復活特效
可以通過HealthPostureController的PlayCustomReviveEffect方法播放自定義特效：
```csharp
healthController.PlayCustomReviveEffect(customEffectPrefab, customPosition, destroyDelay);
``` 