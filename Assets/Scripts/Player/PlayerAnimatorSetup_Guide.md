# 玩家Animator設置詳細指南

## 前置準備

### 1. 確認動畫文件
確保你有以下動畫文件：
- 處決動畫 (Execute Animation)

### 2. 動畫文件命名建議
- `Player_Execute.anim` - 處決動畫

## Animator Controller設置

### 步驟1：創建Animator Controller
1. 在Project視窗中右鍵
2. 選擇 **Create > Animator Controller**
3. 命名為 `PlayerAnimator`

### 步驟2：添加參數
在Animator視窗中：
1. 點擊 **Parameters** 標籤
2. 添加以下參數：

| 參數名稱 | 類型 | 說明 |
|---------|------|------|
| Execute | Trigger | 處決動畫觸發器 |

### 步驟3：創建動畫狀態
1. 將處決動畫拖拽到Animator視窗
2. 重命名狀態：
   - 處決動畫狀態命名為 `Execute`

### 步驟4：設置狀態轉換
1. **Execute轉換**：
   - 右鍵點擊 **Any State**
   - 選擇 **Make Transition**
   - 連接到 **Execute** 狀態
   - 在轉換上設置條件：`Execute` 參數為true

### 步驟5：設置狀態屬性
1. **Execute狀態**：
   - 設置 **Exit Time** 為 0
   - 設置 **Has Exit Time** 為 false
   - 設置 **Transition Duration** 為 0.1

## 動畫狀態機結構

```
Any State
└── Execute (Trigger: Execute)
```

## 動畫設置建議

### 1. 處決動畫 (Execute)
- **持續時間**：建議1-2秒
- **動畫效果**：玩家揮劍、刺擊要害
- **音效**：可以添加處決音效
- **特效**：可以添加處決特效

## 測試方法

### 1. 在Animator中測試
1. 選擇Animator Controller
2. 在Inspector中點擊 **Preview** 標籤
3. 點擊參數測試動畫

### 2. 在遊戲中測試
1. 將Animator Controller分配給玩家
2. 使用 `PlayerAnimatorTestHelper` 腳本測試
3. 按E鍵測試處決動畫

## 常見問題

### 問題1：動畫不播放
**解決方案**：
- 檢查參數名稱是否正確
- 檢查轉換條件是否設置正確
- 檢查動畫文件是否正確導入

### 問題2：動畫播放後不停止
**解決方案**：
- 檢查動畫是否設置為Loop
- 在動畫結束後添加轉換回到Idle狀態

### 問題3：動畫轉換不順暢
**解決方案**：
- 調整Transition Duration
- 設置適當的Exit Time
- 使用Blend Tree優化轉換

## 高級設置

### 1. 使用Blend Tree
對於複雜的動畫系統，可以使用Blend Tree：
1. 創建Blend Tree
2. 添加多個動畫片段
3. 使用參數控制混合

### 2. 添加Layer
對於不同身體部位的動畫：
1. 創建新的Layer
2. 設置Layer Weight
3. 使用Avatar Mask控制影響範圍

### 3. 使用Override Controller
對於不同玩家類型：
1. 創建Override Controller
2. 替換特定動畫片段
3. 保持相同的參數結構

## 性能優化

### 1. 動畫壓縮
- 設置適當的動畫壓縮
- 減少關鍵幀數量
- 使用動畫曲線優化

### 2. 動畫事件
- 使用動畫事件觸發音效
- 使用動畫事件觸發特效
- 使用動畫事件控制遊戲邏輯

## 完整設置檢查清單

- [ ] 創建Animator Controller
- [ ] 添加Execute參數 (Trigger)
- [ ] 導入處決動畫
- [ ] 創建Execute狀態
- [ ] 設置Any State到Execute的轉換
- [ ] 設置轉換條件
- [ ] 調整轉換持續時間
- [ ] 測試動畫播放
- [ ] 分配給玩家物件

## 注意事項

1. **動畫文件格式**：確保動畫文件是Unity支持的格式
2. **動畫長度**：建議動畫長度適中，避免過長影響遊戲節奏
3. **動畫品質**：確保動畫品質符合遊戲風格
4. **性能考慮**：避免過於複雜的動畫狀態機
5. **備份工作**：定期備份Animator Controller設置

## 與現有動畫系統的整合

### 1. 檢查現有參數
確保不與現有參數衝突：
- `Horizontal` (Float)
- `Vertical` (Float)
- `isGrounded` (Bool)
- `isDashing` (Bool)
- `Lock` (Bool)
- `ComboStep` (Integer)
- `Parry1`, `Parry2` (Trigger)
- `Dash` (Trigger)

### 2. 動畫優先級
處決動畫應該有較高的優先級：
- 設置Execute狀態的優先級
- 確保處決動畫可以打斷其他動畫

### 3. 動畫混合
考慮與現有動畫的混合：
- 使用Blend Tree
- 設置適當的Transition Duration
- 考慮動畫的進入和退出時間 