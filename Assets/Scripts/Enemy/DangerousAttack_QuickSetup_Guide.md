# 危攻擊快速設置指南

## 概述
這個指南將教您如何在現有的攻擊動畫中直接插入事件來觸發危攻擊，無需創建新的動畫。

## 方法一：使用 DangerousAttackConverter（推薦）

### 步驟1：添加組件
1. 選擇敵人GameObject
2. 添加 `DangerousAttackConverter` 組件

### 步驟2：配置設定
在 `DangerousAttackConverter` 組件中設置：
- **Enable Dangerous Conversion**: 勾選以啟用危攻擊轉換
- **Dangerous Attack Damage**: 危攻擊傷害值 (建議: 30)
- **Dangerous Attack Range**: 危攻擊範圍 (建議: 2.5)
- **Player Layer**: 玩家層級 (Layer 7)
- **Dangerous Attack Effect**: 危攻擊特效 (可選)

### 步驟3：在現有動畫中插入事件
在Unity Animator中為現有的攻擊動畫添加事件：

#### 在攻擊動畫中添加以下事件：
1. **OnAttackStart()** - 在動畫開始時調用
2. **OnAttackDamage()** - 在傷害判定時調用
3. **OnAttackEnd()** - 在動畫結束時調用

#### 設置方法：
1. 打開Animator窗口
2. 選擇現有的攻擊動畫片段（如 "Attack", "Combo1", "Combo2" 等）
3. 在動畫時間軸上添加事件
4. 將事件函數設置為對應的方法名稱

### 步驟4：動態控制危攻擊
```csharp
// 在代碼中動態啟用/禁用危攻擊
DangerousAttackConverter converter = enemy.GetComponent<DangerousAttackConverter>();

// 啟用危攻擊轉換
converter.EnableDangerousConversion();

// 禁用危攻擊轉換
converter.DisableDangerousConversion();

// 設置危攻擊傷害
converter.SetDangerousAttackDamage(50f);
```

## 方法二：使用 AnimationEventHandler

### 步驟1：添加組件
1. 選擇敵人GameObject
2. 添加 `AnimationEventHandler` 組件

### 步驟2：在動畫中插入事件
在現有攻擊動畫中添加以下事件：

#### 危攻擊事件：
1. **OnDangerousAttackStart()** - 危攻擊開始
2. **OnDangerousAttackDamage()** - 危攻擊傷害判定
3. **OnDangerousAttackEnd()** - 危攻擊結束

#### 一般攻擊事件（可選）：
1. **OnAttackStart()** - 一般攻擊開始
2. **OnAttackDamage()** - 一般攻擊傷害判定
3. **OnAttackEnd()** - 一般攻擊結束

## 實際操作範例

### 範例1：將 Combo1 轉換為危攻擊

1. **選擇動畫片段**：
   - 打開Animator窗口
   - 找到 "Combo1" 動畫片段

2. **添加事件**：
   - 在動畫開始處添加 `OnDangerousAttackStart()` 事件
   - 在攻擊判定時刻添加 `OnDangerousAttackDamage()` 事件
   - 在動畫結束處添加 `OnDangerousAttackEnd()` 事件

3. **設置事件時間**：
   - 開始事件：0.0秒
   - 傷害事件：0.5秒（根據動畫調整）
   - 結束事件：1.0秒

### 範例2：動態切換攻擊類型

```csharp
// 在BossAI中動態控制危攻擊
public class BossAI : EnemyAI
{
    private DangerousAttackConverter converter;
    
    void Start()
    {
        converter = GetComponent<DangerousAttackConverter>();
    }
    
    // 在特定條件下啟用危攻擊
    public void EnableDangerousAttack()
    {
        if (converter != null)
        {
            converter.EnableDangerousConversion();
        }
    }
    
    // 在特定條件下禁用危攻擊
    public void DisableDangerousAttack()
    {
        if (converter != null)
        {
            converter.DisableDangerousConversion();
        }
    }
}
```

## 動畫事件時間建議

### 一般攻擊動畫事件時間：
- **開始事件**: 0.0秒
- **傷害事件**: 0.3-0.6秒（根據動畫長度調整）
- **結束事件**: 1.0秒

### 連擊動畫事件時間：
- **Combo1**: 傷害事件在 0.4秒
- **Combo2**: 傷害事件在 0.5秒
- **Combo3**: 傷害事件在 0.6秒

## 調試和測試

### 調試信息
系統會輸出詳細的Debug信息：
- 危攻擊開始/結束
- 傷害判定結果
- 轉換狀態變化

### 測試方法
1. **在Scene視圖中選擇敵人**可以看到攻擊範圍
2. **使用Context Menu**進行快速測試：
   - 右鍵點擊DangerousAttackConverter組件
   - 選擇 "啟用危攻擊轉換" 或 "禁用危攻擊轉換"
   - 選擇 "檢查危攻擊狀態" 查看當前狀態

### 常見問題解決

#### 問題1：事件不觸發
- 檢查動畫事件函數名稱是否正確
- 確認動畫片段是否正確選擇
- 驗證組件是否正確添加

#### 問題2：傷害不生效
- 確認玩家Layer設置正確
- 檢查攻擊範圍和角度
- 驗證PlayerStatus組件是否存在

#### 問題3：特效不顯示
- 確認特效Prefab是否正確設置
- 檢查特效的Layer設置
- 驗證特效的Particle System設置

## 進階技巧

### 1. 條件性危攻擊
```csharp
// 根據Boss血量決定是否啟用危攻擊
if (bossHealth < 50f)
{
    converter.EnableDangerousConversion();
}
```

### 2. 動態傷害調整
```csharp
// 根據難度調整危攻擊傷害
float damage = isHardMode ? 50f : 30f;
converter.SetDangerousAttackDamage(damage);
```

### 3. 多種攻擊類型
```csharp
// 為不同的攻擊動畫設置不同的危攻擊傷害
if (currentAnimation == "Combo1")
{
    converter.SetDangerousAttackDamage(30f);
}
else if (currentAnimation == "Combo2")
{
    converter.SetDangerousAttackDamage(40f);
}
```

這個方法讓您可以非常靈活地在現有動畫中插入危攻擊功能，無需創建新的動畫片段！ 