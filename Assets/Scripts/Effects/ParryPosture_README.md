# Parry架勢值增加功能說明

## 概述

當玩家Parry敵人時，被Parry的敵人架勢條增加20；當敵人Parry玩家時，玩家架勢條增加20。這個功能增強了Parry系統的戰略性，讓Parry成為雙向的架勢值管理機制。

## 功能實現

### 1. 玩家Parry敵人
- **觸發位置**: `TPController.cs` 中的Parry成功邏輯
- **架勢值增加**: 被Parry的敵人架勢值 +20，玩家不增加架勢值
- **實現方式**: 通過 `HealthPostureController.AddPosture(20, false)` 實現

### 2. 敵人Parry玩家
- **觸發位置**: `PlayerStatus.cs` 中的TakeDamage方法
- **架勢值增加**: 玩家架勢值 +20，敵人不增加架勢值
- **特效**: 在玩家前方產生HitSpark特效
- **實現方式**: 當玩家處於Parry狀態時，自動增加架勢值並觸發特效

## 代碼修改

### TPController.cs
```csharp
// 在Parry成功時增加敵人架勢值
HealthPostureController enemyHealthController = hit.GetComponent<HealthPostureController>();
if (enemyHealthController != null)
{
    enemyHealthController.AddPosture(20, false);
    Debug.Log("[TPController] 被Parry的敵人架勢值增加20");
}
```

### PlayerStatus.cs
```csharp
// 當玩家被敵人Parry時增加架勢值
if (stateInfo.IsTag("Parry"))
{
    currentHitState = HitState.Parry; damage = 0;
    
    // 當玩家被敵人Parry時，玩家架勢值增加20
    if (healthController != null)
    {
        healthController.AddPosture(20, false);
        Debug.Log("[PlayerStatus] 玩家被敵人Parry，架勢值增加20");
    }
}
```

## 測試功能

### ParryPostureTest.cs
- **P鍵**: 測試玩家Parry敵人（敵人架勢值+20）
- **O鍵**: 測試敵人Parry玩家（玩家架勢值+20）

### 測試步驟
1. 在場景中添加 `ParryPostureTest` 組件
2. 按P鍵測試玩家Parry敵人
3. 按O鍵測試敵人Parry玩家
4. 查看Console中的Debug日誌確認架勢值變化

## 遊戲平衡性

### 架勢值增加量
- **標準增加量**: 20點
- **可調整性**: 可在代碼中輕鬆修改數值
- **平衡考量**: 20點是一個適中的數值，既能體現Parry的重要性，又不會過於懲罰

### 戰略意義
1. **鼓勵精準Parry**: 玩家需要更精確的時機來Parry敵人
2. **風險管理**: 錯誤的Parry時機會讓敵人架勢值增加
3. **雙向互動**: 敵人的Parry也會影響玩家的架勢值

## 調試功能

### Debug日誌
- `[TPController] 被Parry的敵人架勢值增加20`
- `[PlayerStatus] 玩家被敵人Parry，架勢值增加20`
- `[ParryPostureTest] 玩家Parry敵人測試 - 敵人架勢值: X.XX → Y.YY (+20)`
- `[ParryPostureTest] 敵人Parry玩家測試 - 玩家架勢值: X.XX → Y.YY (+20)`

### 視覺反饋
- 架勢條會即時更新顯示新的架勢值
- 血條UI會顯示架勢值的變化

## 自定義設置

### 調整架勢值增加量
```csharp
// 在TPController.cs中修改
enemyHealthController.AddPosture(30, false); // 改為30點

// 在PlayerStatus.cs中修改
healthController.AddPosture(30, false); // 改為30點
```

### 添加條件判斷
```csharp
// 可以添加額外的條件判斷
if (enemyHealthController != null && someCondition)
{
    enemyHealthController.AddPosture(20, false);
}
```

## 注意事項

1. **架勢值上限**: 架勢值不會超過最大值（100）
2. **性能考慮**: 架勢值增加是即時計算，性能影響很小
3. **兼容性**: 與現有的架勢值系統完全兼容
4. **調試**: 建議在開發階段使用測試腳本驗證功能

## 版本更新

- **v1.0**: 初始版本，支持基本的Parry架勢值增加功能
- 玩家Parry敵人時，敵人架勢值+20
- 敵人Parry玩家時，玩家架勢值+20
- 完整的測試和調試功能 