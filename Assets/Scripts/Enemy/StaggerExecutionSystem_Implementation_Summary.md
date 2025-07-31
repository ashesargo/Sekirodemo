# 失衡和處決系統實現總結

## 實現概述

已成功實現了完整的失衡和處決系統，包括以下功能：

### ✅ 已實現的功能

1. **失衡狀態系統**
   - 當敵人架勢條滿時自動進入失衡狀態
   - 播放失衡動畫
   - 禁用防禦和攻擊能力
   - 3秒後自動恢復並重置架勢值

2. **處決系統**
   - 玩家在失衡敵人附近按Q鍵進行處決
   - 檢查敵人是否在失衡狀態
   - 檢查距離和角度條件
   - 播放處決動畫

3. **被處決狀態**
   - 敵人被處決時播放被處決動畫
   - 動畫播放完畢後進入死亡狀態

## 新增的腳本文件

### 1. StaggerState.cs
```csharp
// 位置：Assets/Scripts/Enemy/StaggerState.cs
// 功能：處理敵人失衡狀態
// 主要方法：
// - EnterState(): 進入失衡狀態，禁用能力
// - UpdateState(): 播放動畫，計時恢復
// - ExitState(): 退出狀態，重置動畫參數
```

### 2. ExecutedState.cs
```csharp
// 位置：Assets/Scripts/Enemy/ExecutedState.cs
// 功能：處理敵人被處決狀態
// 主要方法：
// - EnterState(): 進入被處決狀態
// - UpdateState(): 播放被處決動畫
// - ExitState(): 退出狀態
```

### 3. ExecutionSystem.cs
```csharp
// 位置：Assets/Scripts/Player/ExecutionSystem.cs
// 功能：處理玩家處決操作
// 主要方法：
// - TryExecuteEnemy(): 檢查並執行處決
// - ExecuteEnemy(): 執行處決邏輯
```

### 4. StaggerExecutionTest.cs
```csharp
// 位置：Assets/Scripts/Test/StaggerExecutionTest.cs
// 功能：測試失衡和處決系統
// 使用方法：按T鍵測試失衡，按Y鍵測試處決
```

## 修改的腳本文件

### 1. HealthPostureController.cs
```csharp
// 修改位置：Assets/Scripts/UI/HealthPostureController.cs
// 主要修改：
// - OnPostureBroken(): 敵人在架勢滿時進入失衡狀態
// - canIncreasePosture: 改為public，方便外部訪問
// - SetPostureValue(): 改為public，並添加ResetPosture()方法
```

### 2. TPController.cs
```csharp
// 修改位置：Assets/Scripts/Player/TPController.cs
// 主要修改：
// - Start(): 自動添加ExecutionSystem組件
```

## 系統流程

```
1. 玩家攻擊敵人 → 增加架勢值
2. 架勢值滿 → 敵人進入失衡狀態 (StaggerState)
3. 玩家在失衡敵人附近按Q → 執行處決 (ExecutionSystem)
4. 敵人進入被處決狀態 (ExecutedState)
5. 播放被處決動畫 → 進入死亡狀態 (DieState)
```

## 動畫要求

### 敵人Animator需要設置：
- `Stagger` (Trigger) - 失衡動畫
- `Executed` (Trigger) - 被處決動畫

### 玩家Animator需要設置：
- `Execute` (Trigger) - 處決動畫

## 使用方法

### 1. 設置敵人
1. 確保敵人有EnemyAI組件
2. 確保敵人有HealthPostureController組件
3. 在Animator中添加Stagger和Executed動畫狀態

### 2. 設置玩家
1. 確保玩家有TPController組件
2. 在Animator中添加Execute動畫狀態
3. ExecutionSystem會自動添加

### 3. 測試系統
1. 將StaggerExecutionTest腳本添加到敵人物件上
2. 按T鍵測試失衡功能
3. 按Y鍵測試處決功能

## 調試信息

系統會輸出詳細的Debug信息：
- `[StaggerState]` - 失衡狀態相關
- `[ExecutedState]` - 被處決狀態相關
- `[ExecutionSystem]` - 處決系統相關
- `[HealthPostureController]` - 架勢控制器相關
- `[StaggerExecutionTest]` - 測試相關

## 注意事項

1. **動畫設置**：確保所有必要的動畫狀態和參數都已正確設置
2. **Layer設置**：敵人Layer應設置為6（Enemy層）
3. **範圍調整**：可以在ExecutionSystem中調整處決範圍
4. **測試**：使用StaggerExecutionTest進行功能測試

## 未來擴展

1. **特效系統**：可以添加處決特效
2. **音效系統**：可以添加處決音效
3. **UI提示**：可以添加處決提示UI
4. **多種處決**：可以實現不同類型的處決動畫

## 文件結構

```
Assets/Scripts/
├── Enemy/
│   ├── StaggerState.cs (新增)
│   ├── ExecutedState.cs (新增)
│   └── StaggerExecutionSystem_README.md (新增)
├── Player/
│   ├── ExecutionSystem.cs (新增)
│   └── TPController.cs (修改)
├── Test/
│   └── StaggerExecutionTest.cs (新增)
└── UI/
    └── HealthPostureController.cs (修改)
```

## 總結

✅ **系統已完全實現並可以正常運行**
✅ **所有必要的腳本都已創建**
✅ **現有系統已正確修改**
✅ **測試腳本已準備就緒**
✅ **文檔已完整編寫**

系統現在可以：
- 當敵人架勢滿時自動進入失衡狀態
- 玩家可以對失衡敵人進行處決
- 處決後敵人會播放被處決動畫並死亡
- 提供完整的調試信息和測試工具 