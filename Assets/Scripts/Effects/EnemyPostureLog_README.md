# 敵人架勢值增加LOG追蹤系統

## 概述
這個系統會詳細記錄敵人架勢值增加的所有來源，幫助您追蹤架勢值增加的具體原因。

## LOG格式說明

### 1. TPController.cs (玩家Parry敵人)
```
[TPController] 玩家Parry敵人成功！敵人: [敵人名稱], 位置: [座標]
[TPController] 敵人架勢值增加來源: TPController.cs (玩家Parry敵人)
[TPController] 增加數值: 20, 是否為Parry: false
[TPController] 被Parry的敵人架勢值增加20
```

### 2. EnemyTest.cs (敵人防禦)
```
[EnemyTest] [敵人類型] 防禦成功！架勢值增加來源: EnemyTest.cs (敵人防禦)
[EnemyTest] 增加數值: [數值], 是否為Parry: false
[敵人類型]防禦時增加架勢值 [數值] 點！
```

### 3. EnemyTest.cs (敵人受傷)
```
[EnemyTest] [敵人類型] 受傷！架勢值增加來源: EnemyTest.cs (敵人受傷)
[EnemyTest] 增加數值: [數值], 是否為Parry: false
[敵人類型]受傷時增加架勢值 [數值] 點！
```

### 4. HealthPostureController.cs (架勢值處理)
```
[HealthPostureController] [物件名稱] 架勢值增加開始
[HealthPostureController] 增加前架勢值: [當前值]/[最大值] ([百分比])
[HealthPostureController] 增加數值: [數值], 是否為Parry: [布林值]
[HealthPostureController] 調用來源: [堆疊追蹤]
[HealthPostureController] 增加後架勢值: [當前值]/[最大值] ([百分比])
[HealthPostureController] [物件名稱] 架勢值增加完成
```

### 5. HealthPostureSystem.cs (架勢值系統)
```
[HealthPostureSystem] 架勢值增加開始: [原值] → [新值]
[HealthPostureSystem] 增加數值: [數值], 是否為Parry: [布林值]
[HealthPostureSystem] 一般架勢: [原值] → [新值] (+[增加量])
[HealthPostureSystem] 架勢值增加完成: [原值] → [新值] (最終值: [當前值]/[最大值])
```

## 可能的架勢值增加來源

### 1. 玩家Parry敵人
- **觸發位置**: `TPController.cs`
- **增加數值**: 20
- **LOG標籤**: `[TPController]`

### 2. 敵人防禦
- **觸發位置**: `EnemyTest.cs`
- **增加數值**: 根據敵人類型決定 (defendPostureIncrease 或 bossDefendPostureIncrease)
- **LOG標籤**: `[EnemyTest]`

### 3. 敵人受傷
- **觸發位置**: `EnemyTest.cs`
- **增加數值**: 根據敵人類型決定 (hitPostureIncrease 或 bossHitPostureIncrease)
- **LOG標籤**: `[EnemyTest]`

## 使用方法

1. **開啟Unity Console**: 在Unity中開啟Console視窗
2. **執行遊戲**: 開始遊戲並進行戰鬥
3. **觀察LOG**: 在Console中觀察帶有 `[TPController]`, `[EnemyTest]`, `[HealthPostureController]`, `[HealthPostureSystem]` 標籤的LOG
4. **追蹤架勢值**: 根據LOG內容追蹤敵人架勢值增加的具體來源和數值

## 注意事項

- 所有LOG都會顯示完整的調用堆疊，幫助您追蹤架勢值增加的完整路徑
- 如果敵人沒有HealthPostureController組件，會顯示警告LOG
- 架勢值增加前後都會記錄，方便您確認數值變化
- Parry狀態的架勢值增加有特殊限制（最大值99%）

## 調試技巧

1. **篩選LOG**: 在Console中使用關鍵字篩選特定類型的LOG
2. **觀察時序**: 注意LOG的時間順序，了解架勢值增加的先後順序
3. **檢查數值**: 確認架勢值增加前後的數值是否正確
4. **追蹤來源**: 使用堆疊追蹤信息找到架勢值增加的具體程式碼位置 