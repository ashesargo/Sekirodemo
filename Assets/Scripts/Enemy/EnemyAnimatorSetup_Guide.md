# 敵人Animator設置詳細指南

## 前置準備

### 1. 確認動畫文件
確保你有以下動畫文件：
- 失衡動畫 (Stagger Animation)
- 被處決動畫 (Executed Animation)

### 2. 動畫文件命名建議
- `Enemy_Stagger.anim` - 失衡動畫
- `Enemy_Executed.anim` - 被處決動畫

## Animator Controller設置

### 步驟1：創建Animator Controller
1. 在Project視窗中右鍵
2. 選擇 **Create > Animator Controller**
3. 命名為 `EnemyAnimator`

### 步驟2：添加參數
在Animator視窗中：
1. 點擊 **Parameters** 標籤
2. 添加以下參數：

| 參數名稱 | 類型 | 說明 |
|---------|------|------|
| Stagger | Trigger | 失衡動畫觸發器 |
| Executed | Trigger | 被處決動畫觸發器 |

### 步驟3：創建動畫狀態
1. 將失衡動畫拖拽到Animator視窗
2. 將被處決動畫拖拽到Animator視窗
3. 重命名狀態：
   - 失衡動畫狀態命名為 `Stagger`
   - 被處決動畫狀態命名為 `Executed`

### 步驟4：設置狀態轉換
1. **Stagger轉換**：
   - 右鍵點擊 **Any State**
   - 選擇 **Make Transition**
   - 連接到 **Stagger** 狀態
   - 在轉換上設置條件：`Stagger` 參數為true

2. **Executed轉換**：
   - 右鍵點擊 **Any State**
   - 選擇 **Make Transition**
   - 連接到 **Executed** 狀態
   - 在轉換上設置條件：`Executed` 參數為true

### 步驟5：設置狀態屬性
1. **Stagger狀態**：
   - 設置 **Exit Time** 為 0
   - 設置 **Has Exit Time** 為 false
   - 設置 **Transition Duration** 為 0.1

2. **Executed狀態**：
   - 設置 **Exit Time** 為 0
   - 設置 **Has Exit Time** 為 false
   - 設置 **Transition Duration** 為 0.1

## 動畫狀態機結構

```
Any State
├── Stagger (Trigger: Stagger)
└── Executed (Trigger: Executed)
```

## 動畫設置建議

### 1. 失衡動畫 (Stagger)
- **持續時間**：建議2-3秒
- **動畫效果**：敵人搖晃、失去平衡
- **音效**：可以添加失衡音效

### 2. 被處決動畫 (Executed)
- **持續時間**：建議1-2秒
- **動畫效果**：敵人被擊中要害、倒下
- **音效**：可以添加處決音效

## 測試方法

### 1. 在Animator中測試
1. 選擇Animator Controller
2. 在Inspector中點擊 **Preview** 標籤
3. 點擊參數測試動畫

### 2. 在遊戲中測試
1. 將Animator Controller分配給敵人
2. 使用 `StaggerExecutionTest` 腳本測試
3. 按T鍵測試失衡動畫
4. 按Y鍵測試處決動畫

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
對於不同敵人類型：
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
- [ ] 添加Stagger參數 (Trigger)
- [ ] 添加Executed參數 (Trigger)
- [ ] 導入失衡動畫
- [ ] 導入被處決動畫
- [ ] 創建Stagger狀態
- [ ] 創建Executed狀態
- [ ] 設置Any State到Stagger的轉換
- [ ] 設置Any State到Executed的轉換
- [ ] 設置轉換條件
- [ ] 調整轉換持續時間
- [ ] 測試動畫播放
- [ ] 分配給敵人物件

## 注意事項

1. **動畫文件格式**：確保動畫文件是Unity支持的格式
2. **動畫長度**：建議動畫長度適中，避免過長影響遊戲節奏
3. **動畫品質**：確保動畫品質符合遊戲風格
4. **性能考慮**：避免過於複雜的動畫狀態機
5. **備份工作**：定期備份Animator Controller設置 