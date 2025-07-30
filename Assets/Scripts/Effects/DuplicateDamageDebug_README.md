# 重複傷害問題追蹤指南

## 問題描述
玩家攻擊敵人時，敵人會判定兩次傷害，導致架勢值異常增加。

**已解決**: 問題根源在於EnemyTest.cs的TakeDamage方法中重複調用架勢值增加。

**新發現**: 敵人可能有多個碰撞體，導致同一個敵人在一次攻擊中被重複檢測。

## 可能的原因

### 1. 動畫事件重複調用
- **Weapon.cs**: `PerformFanAttack()` 可能被動畫事件多次調用
- **檢查方法**: 觀察 `[Weapon] PerformFanAttack 被調用` 的LOG頻率

### 2. 多個攻擊系統衝突
- **TPController.cs**: 處理Parry邏輯
- **Weapon.cs**: 處理一般攻擊邏輯
- **WeaponEffect.cs**: 處理攻擊特效

### 3. 動畫控制器設置問題
- 動畫事件可能在同一個動畫中設置了多次
- 不同動畫狀態可能都調用了同一個事件

### 4. 敵人多個碰撞體問題 ⭐ 新發現
- 敵人可能有多個碰撞體（主碰撞體、武器碰撞體等）
- 同一個敵人的不同碰撞體被分別檢測
- 導致同一個敵人在一次攻擊中被重複處理

## 追蹤方法

### 1. 觀察Weapon.cs的LOG
```
[Weapon] PerformFanAttack 被調用 - 時間: [時間戳]
[Weapon] 當前狀態 - isAttacking: [布林值], 距離上次攻擊: [秒數]秒
[Weapon] 檢測到 [數量] 個碰撞體
[Weapon] 碰撞體 [索引]: [碰撞體名稱] (GameObject: [遊戲物件名稱])
[Weapon] 敵人狀態: [狀態名稱], HP: [血量]
[Weapon] 對敵人 [敵人名稱] 造成 [傷害] 點傷害 (角度: [角度]°)
```

### 2. 觀察EnemyTest.cs的LOG
```
[EnemyTest] [敵人名稱] 接收到傷害: [傷害值] - 時間: [時間戳]
[EnemyTest] 當前狀態 - HP: [血量], isDead: [布林值], state: [狀態]
[EnemyTest] 調用堆疊: [堆疊追蹤]
```

### 3. 觀察架勢值增加的LOG
```
[HealthPostureController] [物件名稱] 架勢值增加開始
[HealthPostureController] 增加前架勢值: [當前值]/[最大值] ([百分比])
[HealthPostureController] 增加數值: [數值], 是否為Parry: [布林值]
[HealthPostureController] 調用來源: [堆疊追蹤]
```

## 調試步驟

### 步驟1: 確認攻擊調用頻率
1. 執行遊戲並進行一次攻擊
2. 觀察Console中 `[Weapon] PerformFanAttack 被調用` 的出現次數
3. 如果出現多次，說明動畫事件被重複調用

### 步驟2: 確認碰撞體檢測
1. 觀察 `[Weapon] 檢測到 [數量] 個碰撞體` 的LOG
2. 檢查 `[Weapon] 碰撞體 [索引]: [名稱]` 的詳細信息
3. 確認是否有多個碰撞體屬於同一個敵人

### 步驟3: 確認傷害接收頻率
1. 觀察 `[EnemyTest] 接收到傷害` 的LOG
2. 檢查同一個敵人在短時間內是否接收到多次傷害
3. 記錄每次傷害的時間戳和調用堆疊

### 步驟4: 確認架勢值增加來源
1. 觀察 `[HealthPostureController] 架勢值增加開始` 的LOG
2. 檢查架勢值增加的調用來源
3. 確認是否有重複的架勢值增加

## 解決方案

### 已修正: EnemyTest.cs重複架勢值增加
**問題**: EnemyTest.cs的TakeDamage方法中同時調用了：
1. `healthController.TakeDamage(damage, PlayerStatus.HitState.Hit)` - 會增加架勢值
2. `healthController.AddPosture(additionalPostureIncrease, false)` - 又增加架勢值

**修正**: 移除重複的`AddPosture`調用，只保留`TakeDamage`的架勢值增加。

### 新修正: 敵人多個碰撞體問題
**問題**: 敵人可能有多個碰撞體，導致同一個敵人在一次攻擊中被重複檢測。

**修正**: 
1. 使用GameObject而不是Collider來追蹤已攻擊的目標
2. 添加詳細的碰撞體檢測LOG
3. 檢查敵人狀態變化

### 方案1: 修正動畫事件
如果發現動畫事件被重複調用：
1. 檢查動畫控制器中的事件設置
2. 確保每個動畫狀態只調用一次攻擊事件
3. 移除重複的動畫事件

### 方案2: 加強防重複攻擊機制
如果Weapon.cs的防重複機制不夠強：
1. 增加攻擊冷卻時間
2. 添加更嚴格的狀態檢查
3. 使用更精確的時間控制

### 方案3: 統一攻擊系統
如果有多個攻擊系統衝突：
1. 統一使用Weapon.cs處理所有攻擊
2. 移除TPController中的攻擊檢測邏輯
3. 確保只有一個系統負責傷害處理

## 預期結果

正常情況下，一次攻擊應該產生以下LOG序列：
1. `[Weapon] PerformFanAttack 被調用` (1次)
2. `[Weapon] 檢測到 [數量] 個碰撞體` (顯示碰撞體詳情)
3. `[Weapon] 對敵人 [名稱] 造成 [傷害] 點傷害` (每個敵人1次)
4. `[EnemyTest] [敵人名稱] 接收到傷害` (每個敵人1次)
5. `[HealthPostureController] 架勢值增加開始` (每個敵人1次)

如果發現重複的LOG，則需要進一步調查和修正。

## 注意事項

- 所有LOG都包含時間戳，方便追蹤時序
- 調用堆疊信息可以幫助定位問題源頭
- 建議在測試時使用單個敵人，避免干擾
- 記錄所有相關的LOG，以便後續分析
- 注意敵人碰撞體的設置，避免多個碰撞體導致重複檢測 