# 敵人徑向模糊效果系統使用說明

## 概述

這個系統為敵人提供了與主角類似的徑向模糊後處理效果，但模糊強度比主角少50%，創造出更平衡的視覺效果。

## 主要功能

### 1. 敵人模糊效果 (EnemyPointBlur.cs)
- **模糊強度**: 比主角少50%
  - 預設模糊強度: 0.5f (主角為1.0f)
  - Parry模糊強度: 0.25f (主角為0.5f)
  - 防禦模糊強度: 0.15f (主角為0.3f)
  - 受傷模糊強度: 0.2f (主角為0.4f)

### 2. 事件系統
敵人AI現在包含四個新的事件：
- `OnEnemyParrySuccess`: 當敵人被玩家成功Parry時觸發
- `OnEnemyGuardSuccess`: 當敵人成功防禦時觸發
- `OnEnemyHitOccurred`: 當敵人受傷時觸發
- `OnEnemyAttackSuccess`: 當敵人攻擊成功時觸發

## 設置步驟

### 1. 添加敵人模糊效果組件
1. 在相機物件上添加 `EnemyPointBlur` 腳本
2. 設置以下參數：
   - **PointBlurShader**: 使用與主角相同的徑向模糊Shader
   - **curve**: 模糊強度動畫曲線
   - **EnemySpark**: 敵人格擋成功特效Prefab
   - **EnemyGuardSpark**: 敵人防禦特效Prefab
   - **EnemyHitSpark**: 敵人受傷特效Prefab

### 2. 配置敵人AI
1. 確保敵人物件有 `EnemyAI` 組件
2. 設置 `enemyWeaponCollider`: 敵人的武器碰撞器
3. 設置 `detectionLayer`: 碰撞檢測層級

### 3. 特效Prefab設置
為敵人創建專門的特效Prefab：
- **EnemySpark**: 敵人格擋成功時的火花特效
- **EnemyGuardSpark**: 敵人防禦時的火花特效
- **EnemyHitSpark**: 敵人受傷時的火花特效

## 事件觸發時機

### 敵人受傷事件 (OnEnemyHitOccurred)
- **觸發時機**: 當敵人被玩家攻擊且未成功防禦時
- **觸發位置**: 敵人身前2單位處
- **模糊效果**: 使用 `HitBlurStrength` (0.2f)

### 敵人防禦事件 (OnEnemyGuardSuccess)
- **觸發時機**: 當敵人成功防禦玩家攻擊時
- **觸發位置**: 敵人身前2單位處
- **模糊效果**: 使用 `GuardBlurStrength` (0.15f)

### 敵人Parry事件 (OnEnemyParrySuccess)
- **觸發時機**: 當玩家成功Parry敵人攻擊時
- **觸發位置**: 敵人位置
- **模糊效果**: 使用 `ParryBlurStrength` (0.25f)

### 敵人攻擊成功事件 (OnEnemyAttackSuccess)
- **觸發時機**: 當敵人成功攻擊玩家時
- **觸發位置**: 敵人位置前方1.5單位處
- **特效**: 在敵人位置產生EnemyHitSpark特效
- **模糊效果**: 使用 `HitBlurStrength` (0.2f)

## 性能優化

### 降採樣設置
- **downSampleFactor**: 設為2，提高渲染性能
- **模糊範圍**: 可調整 `BlurRange` 參數
- **模糊速度**: 可調整 `BlurSpeed` 參數

### 材質管理
- 自動創建和管理後處理材質
- 自動釋放臨時渲染紋理
- 避免內存洩漏

## 調試功能

### Debug日誌
系統會輸出詳細的Debug日誌：
- 事件觸發確認
- 碰撞點檢測結果
- 特效生成狀態

### 視覺調試
- 可在Scene視圖中查看碰撞檢測範圍
- 可調整模糊參數即時預覽效果

## 注意事項

1. **Shader兼容性**: 確保使用與主角相同的徑向模糊Shader
2. **特效Prefab**: 建議為敵人創建專門的特效，與主角特效區分
3. **性能考慮**: 在大量敵人同時存在時，注意模糊效果的性能影響
4. **事件訂閱**: 系統會自動處理事件的訂閱和取消訂閱

## 自定義設置

### 調整模糊強度
```csharp
// 在EnemyPointBlur組件中調整
BlurStrength = 0.5f;        // 預設模糊強度
ParryBlurStrength = 0.25f;  // Parry模糊強度
GuardBlurStrength = 0.15f;  // 防禦模糊強度
HitBlurStrength = 0.2f;     // 受傷模糊強度
```

### 調整特效位置
```csharp
// 在事件處理中調整特效生成位置
Vector3 spawnPosition = enemyPosition + enemyForward * 2f;
```

## 故障排除

### 常見問題
1. **模糊效果不顯示**: 檢查Shader設置和材質初始化
2. **特效不生成**: 確認特效Prefab已正確設置
3. **事件不觸發**: 檢查敵人AI組件和事件訂閱狀態

### 調試步驟
1. 檢查Console中的Debug日誌
2. 確認EnemyPointBlur組件已正確初始化
3. 驗證敵人AI事件是否正常觸發
4. 檢查特效Prefab是否有效

## 版本更新

- **v1.0**: 初始版本，支持基本的敵人模糊效果
- 模糊強度比主角少50%
- 支持三種事件類型：受傷、防禦、Parry
- 完整的性能優化和調試功能 