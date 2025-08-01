# 危攻擊系統使用指南

## 概述
危攻擊系統是一個模仿《隻狼》的機制，允許Boss執行無視玩家防禦的特殊攻擊。這些攻擊會直接對玩家造成傷害，無法被防禦或Parry。

## 系統組件

### 1. 核心腳本
- `DangerousAttackState.cs` - 一般敵人的危攻擊狀態
- `BossDangerousAttackState.cs` - Boss專用的危攻擊狀態
- `DangerousAttackHandler.cs` - 危攻擊傷害處理器
- `AnimationEventHandler.cs` - 動畫事件處理器

### 2. 修改的腳本
- `PlayerStatus.cs` - 新增 `TakeDangerousDamage()` 方法
- `BossAI.cs` - 新增危攻擊相關設定和方法
- `BossComboState.cs` - 整合危攻擊觸發邏輯

## 設置步驟

### 步驟1：為敵人添加組件
1. 選擇敵人GameObject
2. 添加以下組件：
   - `DangerousAttackHandler` (用於處理危攻擊傷害)
   - `AnimationEventHandler` (用於處理動畫事件)

### 步驟2：配置危攻擊設定
在 `DangerousAttackHandler` 組件中設置：
- **Dangerous Attack Damage**: 危攻擊傷害值 (建議: 30)
- **Dangerous Attack Range**: 危攻擊範圍 (建議: 2.5)
- **Player Layer**: 玩家層級 (Layer 7)
- **Attack Point**: 攻擊點位置 (可選)
- **Dangerous Attack Effect**: 危攻擊特效 (可選)

### 步驟3：配置BossAI設定
在 `BossAI` 組件中設置：
- **Dangerous Attack Animations**: 危攻擊動畫名稱陣列
- **Dangerous Attack Chance**: 危攻擊觸發機率 (建議: 0.2)
- **Dangerous Attack Cooldown**: 危攻擊冷卻時間 (建議: 10秒)

### 步驟4：設置動畫事件
在Unity Animator中為危攻擊動畫添加事件：

#### 危攻擊動畫事件：
1. **OnDangerousAttackStart()** - 在動畫開始時調用
2. **OnDangerousAttackDamage()** - 在傷害判定時調用
3. **OnDangerousAttackEnd()** - 在動畫結束時調用

#### 設置方法：
1. 打開Animator窗口
2. 選擇危攻擊動畫片段
3. 在動畫時間軸上添加事件
4. 將事件函數設置為對應的方法名稱

### 步驟5：創建危攻擊動畫
1. 在Animator中創建新的動畫狀態
2. 命名為 "DangerousAttack" 或自定義名稱
3. 設置適當的動畫片段
4. 添加動畫事件

## 使用範例

### 範例1：基本危攻擊設置
```csharp
// 在BossAI中觸發危攻擊
if (bossAI.ShouldTriggerDangerousAttack())
{
    bossAI.PerformDangerousAttack();
}
```

### 範例2：自定義危攻擊傷害
```csharp
// 在DangerousAttackHandler中修改傷害值
public float dangerousAttackDamage = 50f; // 更高的傷害
```

### 範例3：添加危攻擊特效
```csharp
// 在AnimationEventHandler中播放特效
if (dangerousAttackEffect != null)
{
    Vector3 effectPosition = attackPoint.position + attackPoint.forward * 2f;
    Instantiate(dangerousAttackEffect, effectPosition, attackPoint.rotation);
}
```

## 動畫事件詳細說明

### OnDangerousAttackStart()
- **時機**: 危攻擊動畫開始時
- **功能**: 
  - 設置危攻擊狀態為活躍
  - 播放危攻擊特效
  - 記錄Debug信息

### OnDangerousAttackDamage()
- **時機**: 危攻擊傷害判定時
- **功能**:
  - 檢測範圍內的玩家
  - 對玩家造成無視防禦的傷害
  - 觸發受傷事件和特效

### OnDangerousAttackEnd()
- **時機**: 危攻擊動畫結束時
- **功能**:
  - 設置危攻擊狀態為非活躍
  - 清理相關狀態

## 調試和測試

### 調試信息
系統會輸出詳細的Debug信息：
- 危攻擊開始/結束
- 傷害判定結果
- 玩家受傷情況

### 視覺化調試
- 在Scene視圖中選擇敵人可以看到攻擊範圍
- 紅色圓圈表示攻擊範圍
- 黃色射線表示攻擊角度

### 測試建議
1. 確保玩家在正確的Layer上 (Layer 7)
2. 測試防禦狀態下是否仍會受傷
3. 檢查動畫事件是否正確觸發
4. 驗證傷害值是否正確

## 進階功能

### 自定義危攻擊類型
可以創建不同類型的危攻擊：
- 突刺攻擊
- 橫掃攻擊
- 跳躍攻擊
- 遠程攻擊

### 危攻擊連擊
可以設置危攻擊的後續行為：
- 繼續一般攻擊
- 撤退
- 觸發其他特殊攻擊

### 危攻擊預警
可以添加視覺或音效預警：
- 紅色警告標誌
- 特殊音效
- 粒子特效

## 注意事項

1. **動畫事件必須正確設置** - 確保動畫事件在正確的時間點觸發
2. **Layer設置** - 確保玩家和敵人在正確的Layer上
3. **性能考慮** - 危攻擊特效不要過於複雜，避免影響性能
4. **平衡性** - 危攻擊傷害和冷卻時間需要平衡遊戲難度

## 故障排除

### 常見問題
1. **危攻擊不觸發**
   - 檢查BossAI的觸發機率設置
   - 確認冷卻時間是否已過
   - 檢查動畫事件是否正確設置

2. **傷害不生效**
   - 確認玩家Layer設置正確
   - 檢查攻擊範圍和角度
   - 驗證PlayerStatus組件是否存在

3. **特效不顯示**
   - 確認特效Prefab是否正確設置
   - 檢查特效的Layer設置
   - 驗證特效的Particle System設置

### 調試命令
```csharp
// 強制觸發危攻擊
bossAI.PerformDangerousAttack();

// 檢查危攻擊狀態
Debug.Log($"Can perform dangerous attack: {bossAI.CanPerformDangerousAttack()}");
``` 