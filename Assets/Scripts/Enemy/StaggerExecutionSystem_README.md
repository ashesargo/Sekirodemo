# 失衡和處決系統說明

## 系統概述

這個系統實現了類似《隻狼》的失衡和處決機制：

1. **失衡狀態**：當敵人架勢條滿時，敵人會進入失衡狀態
2. **處決機制**：玩家可以在失衡敵人附近按下Q鍵進行處決

## 新增的腳本

### 1. StaggerState.cs (失衡狀態)
- **功能**：當敵人架勢滿時自動進入此狀態
- **行為**：
  - 播放失衡動畫
  - 禁用防禦和攻擊能力
  - 持續3秒後自動恢復
  - 恢復時重置架勢值

### 2. ExecutedState.cs (被處決狀態)
- **功能**：當敵人被玩家處決時進入此狀態
- **行為**：
  - 播放被處決動畫
  - 動畫播放完畢後進入死亡狀態

### 3. ExecutionSystem.cs (玩家處決系統)
- **功能**：處理玩家的處決操作
- **使用方法**：在失衡敵人附近按下Q鍵
- **條件**：
  - 敵人在失衡狀態
  - 敵人在玩家前方60度範圍內
  - 敵人在處決範圍內（預設2米）

## 修改的腳本

### 1. HealthPostureController.cs
- 修改了`OnPostureBroken`方法，讓敵人在架勢滿時進入失衡狀態
- 將`canIncreasePosture`字段改為public，方便外部訪問
- 將`SetPostureValue`方法改為public，並添加`ResetPosture()`方法

### 2. TPController.cs
- 在Start方法中自動添加ExecutionSystem組件

## 動畫要求

需要在Animator中設置以下動畫參數：

### 敵人動畫
- `Stagger` (Trigger) - 失衡動畫
- `Executed` (Trigger) - 被處決動畫

### 玩家動畫
- `Execute` (Trigger) - 處決動畫

## 使用方法

1. **觸發失衡**：
   - 攻擊敵人增加架勢值
   - 當架勢條滿時，敵人自動進入失衡狀態

2. **執行處決**：
   - 在失衡敵人附近
   - 面向敵人
   - 按下Q鍵

3. **系統流程**：
   ```
   敵人架勢滿 → 進入失衡狀態 → 玩家按Q處決 → 進入被處決狀態 → 死亡
   ```

## 調試信息

系統會輸出詳細的Debug信息：
- `[StaggerState]` - 失衡狀態相關
- `[ExecutedState]` - 被處決狀態相關
- `[ExecutionSystem]` - 處決系統相關
- `[HealthPostureController]` - 架勢控制器相關

## 注意事項

1. 確保敵人Animator中有相應的動畫狀態
2. 確保玩家Animator中有處決動畫
3. 敵人的Layer設置為6（Enemy層）
4. 處決範圍可以在ExecutionSystem中調整 