# 完整Animator設置指南

## 概述

這個指南涵蓋了失衡和處決系統所需的所有Animator設置，包括敵人和玩家的動畫配置。

## 敵人Animator設置

### 必要參數
| 參數名稱 | 類型 | 說明 |
|---------|------|------|
| Stagger | Trigger | 失衡動畫觸發器 |
| Executed | Trigger | 被處決動畫觸發器 |

### 動畫狀態
1. **Stagger** - 失衡動畫狀態
2. **Executed** - 被處決動畫狀態

### 轉換設置
- Any State → Stagger (條件: Stagger = true)
- Any State → Executed (條件: Executed = true)

## 玩家Animator設置

### 必要參數
| 參數名稱 | 類型 | 說明 |
|---------|------|------|
| Execute | Trigger | 處決動畫觸發器 |

### 動畫狀態
1. **Execute** - 處決動畫狀態

### 轉換設置
- Any State → Execute (條件: Execute = true)

## 快速設置步驟

### 敵人Animator設置

1. **創建Animator Controller**
   ```
   右鍵 > Create > Animator Controller
   命名: EnemyAnimator
   ```

2. **添加參數**
   ```
   Parameters標籤 > + > Trigger
   - Stagger
   - Executed
   ```

3. **創建動畫狀態**
   ```
   拖拽動畫文件到Animator視窗
   重命名為: Stagger, Executed
   ```

4. **設置轉換**
   ```
   右鍵Any State > Make Transition
   連接到Stagger和Executed狀態
   設置條件為對應的Trigger參數
   ```

### 玩家Animator設置

1. **創建Animator Controller**
   ```
   右鍵 > Create > Animator Controller
   命名: PlayerAnimator
   ```

2. **添加參數**
   ```
   Parameters標籤 > + > Trigger
   - Execute
   ```

3. **創建動畫狀態**
   ```
   拖拽處決動畫到Animator視窗
   重命名為: Execute
   ```

4. **設置轉換**
   ```
   右鍵Any State > Make Transition
   連接到Execute狀態
   設置條件為Execute參數
   ```

## 測試腳本

### 敵人測試
- **腳本**: `AnimatorTestHelper.cs`
- **按鍵**: 
  - `1` - 測試失衡動畫
  - `2` - 測試被處決動畫
  - `R` - 重置動畫

### 玩家測試
- **腳本**: `PlayerAnimatorTestHelper.cs`
- **按鍵**:
  - `E` - 測試處決動畫
  - `R` - 重置動畫

## 動畫建議

### 敵人動畫
1. **失衡動畫 (Stagger)**
   - 持續時間: 2-3秒
   - 效果: 搖晃、失去平衡
   - 音效: 失衡音效

2. **被處決動畫 (Executed)**
   - 持續時間: 1-2秒
   - 效果: 被擊中要害、倒下
   - 音效: 處決音效

### 玩家動畫
1. **處決動畫 (Execute)**
   - 持續時間: 1-2秒
   - 效果: 揮劍、刺擊要害
   - 音效: 處決音效
   - 特效: 處決特效

## 常見問題解決

### 問題1: 動畫不播放
**解決方案**:
- 檢查參數名稱是否正確
- 檢查轉換條件是否設置正確
- 檢查動畫文件是否正確導入

### 問題2: 動畫播放後不停止
**解決方案**:
- 檢查動畫是否設置為Loop
- 在動畫結束後添加轉換回到Idle狀態

### 問題3: 動畫轉換不順暢
**解決方案**:
- 調整Transition Duration
- 設置適當的Exit Time
- 使用Blend Tree優化轉換

## 性能優化

### 1. 動畫壓縮
- 設置適當的動畫壓縮
- 減少關鍵幀數量
- 使用動畫曲線優化

### 2. 動畫事件
- 使用動畫事件觸發音效
- 使用動畫事件觸發特效
- 使用動畫事件控制遊戲邏輯

## 完整檢查清單

### 敵人設置
- [ ] 創建EnemyAnimator Controller
- [ ] 添加Stagger參數 (Trigger)
- [ ] 添加Executed參數 (Trigger)
- [ ] 導入失衡動畫
- [ ] 導入被處決動畫
- [ ] 創建Stagger狀態
- [ ] 創建Executed狀態
- [ ] 設置Any State轉換
- [ ] 設置轉換條件
- [ ] 分配給敵人物件
- [ ] 測試動畫播放

### 玩家設置
- [ ] 創建PlayerAnimator Controller
- [ ] 添加Execute參數 (Trigger)
- [ ] 導入處決動畫
- [ ] 創建Execute狀態
- [ ] 設置Any State轉換
- [ ] 設置轉換條件
- [ ] 分配給玩家物件
- [ ] 測試動畫播放

## 與現有系統的整合

### 檢查現有參數
確保不與現有參數衝突：

**敵人現有參數**:
- 檢查是否有衝突的參數名稱

**玩家現有參數**:
- `Horizontal` (Float)
- `Vertical` (Float)
- `isGrounded` (Bool)
- `isDashing` (Bool)
- `Lock` (Bool)
- `ComboStep` (Integer)
- `Parry1`, `Parry2` (Trigger)
- `Dash` (Trigger)

### 動畫優先級
- 處決動畫應該有較高的優先級
- 確保處決動畫可以打斷其他動畫

## 注意事項

1. **動畫文件格式**: 確保動畫文件是Unity支持的格式
2. **動畫長度**: 建議動畫長度適中，避免過長影響遊戲節奏
3. **動畫品質**: 確保動畫品質符合遊戲風格
4. **性能考慮**: 避免過於複雜的動畫狀態機
5. **備份工作**: 定期備份Animator Controller設置

## 測試流程

1. **設置Animator Controller**
2. **添加必要參數**
3. **創建動畫狀態**
4. **設置狀態轉換**
5. **分配給物件**
6. **使用測試腳本驗證**
7. **在遊戲中測試完整流程**

## 調試信息

系統會輸出詳細的Debug信息：
- `[AnimatorTestHelper]` - 敵人動畫測試
- `[PlayerAnimatorTestHelper]` - 玩家動畫測試
- `[StaggerState]` - 失衡狀態相關
- `[ExecutedState]` - 被處決狀態相關
- `[ExecutionSystem]` - 處決系統相關 