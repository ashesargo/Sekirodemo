# Analyze enemy state scripts for Sekiro
_Exported on 2025/7/25 at 17:32:00 [GMT+8] from Cursor (1.2.4)_

---

**User**

@d:\Sekirodemo\Assets\Scripts\Enemy\HitState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\IEnemyState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\IdleState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\RangedEnemy.cs @d:\Sekirodemo\Assets\Scripts\Enemy\Projectile.cs @d:\Sekirodemo\Assets\Scripts\Enemy\ReloadState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\RetreatState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\ShootState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\AimState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\AlertState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\AttackState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\BossAI.cs @d:\Sekirodemo\Assets\Scripts\Enemy\BossChaseState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\BossComboState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\BossIdleState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\BossIntroComboState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\BossJumpAttackState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\BossRangedAttack.cs @d:\Sekirodemo\Assets\Scripts\Enemy\BossRangedAttackState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\BossRushState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\ChaseState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\CloseCombatState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\DieState.cs @d:\Sekirodemo\Assets\Scripts\Enemy\EnemyAI.cs 我是正在模仿隻狼的遊戲開發者 請幫我分析這些腳本

---

**Cursor**

好的，以下是針對你提供的所有腳本的結構與功能分析，並會以「隻狼」AI設計的角度，說明各腳本的用途與設計思路。  
（如需針對某一腳本細節深入討論，請再指定）

---

## 1. 狀態機核心設計

### `BaseEnemyState`（IEnemyState.cs）
- **說明**：所有敵人狀態的基底類別，定義了進入、更新、離開狀態的方法，並可決定是否啟用Root Motion。
- **設計重點**：  
  - 讓每個狀態都能覆寫進入/更新/離開時的行為。
  - Root Motion 控制，方便動畫驅動或程式驅動移動切換。

### `EnemyAI`
- **說明**：敵人AI的主控腳本，負責狀態切換、移動、避障、與玩家互動等。
- **設計重點**：  
  - 狀態機模式（State Pattern），每個狀態為一個類別。
  - 支援追擊、避障、撤退、近戰、遠程等多種行為。
  - 內建物理避障（立方體/球體偵測）、敵人間避障。
  - 支援Root Motion與程式移動切換。
  - 方便擴充Boss專屬AI。

---

## 2. 一般敵人狀態

### `IdleState`
- **說明**：待機狀態，偵測玩家進入視野後切換到警戒。
- **設計重點**：  
  - 進入時播放Idle動畫，啟用自動攻擊。
  - 距離與視野判斷，觸發警戒。

### `AlertState`
- **說明**：警戒狀態，短暫觀察後決定進攻或追擊。
- **設計重點**：  
  - 播放Alert動畫，短暫等待。
  - 判斷是否進入攻擊範圍，遠程兵種進Aim，近戰進Attack。

### `ChaseState`
- **說明**：追擊狀態，靠近玩家並避障。
- **設計重點**：  
  - 使用Seek、ObstacleAvoid、EnemyAvoid組合力移動。
  - 進入攻擊範圍自動切換狀態。

### `AttackState`
- **說明**：近戰攻擊狀態，播放攻擊動畫，結束後撤退。
- **設計重點**：  
  - 進入時觸發Attack動畫。
  - 動畫結束自動切換到撤退。

### `RetreatState`
- **說明**：撤退狀態，敵人會背向玩家移動並避障。
- **設計重點**：  
  - 使用背向玩家的方向與專用避障。
  - 撤退結束後切換回追擊或Boss專屬狀態。

### `HitState`
- **說明**：受傷狀態，播放受擊或Parry動畫，動畫結束後根據血量切換狀態。
- **設計重點**：  
  - Boss有隨機Parry動畫。
  - 受傷結束後自動判斷死亡、追擊或待機。

### `DieState`
- **說明**：死亡狀態，播放死亡動畫，等待回收。

---

## 3. 遠程敵人專屬

### `RangedEnemy`
- **說明**：遠程敵人專用腳本，負責發射子彈、音效、特效。
- **設計重點**：  
  - 可指定射擊點、目標、子彈速度/傷害。
  - 支援音效與槍口火花。

### `Projectile`
- **說明**：子彈腳本，負責移動、碰撞、傷害、命中特效。
- **設計重點**：  
  - 支援存活時間、命中目標層、命中後銷毀。
  - 可對玩家或敵人造成傷害。

### `AimState`
- **說明**：瞄準狀態，敵人面向玩家，瞄準一段時間後射擊。
- **設計重點**：  
  - 支援玩家脫離攻擊範圍時自動追擊。
  - 玩家太近時切換近戰。

### `ShootState`
- **說明**：射擊狀態，播放射擊動畫並發射子彈，動畫結束後裝彈或近戰。
- **設計重點**：  
  - 動畫事件觸發射擊。
  - 玩家太近時自動切換近戰。

### `ReloadState`
- **說明**：裝彈狀態，播放裝彈動畫，裝彈完成後根據距離決定行動。
- **設計重點**：  
  - 玩家太近時優先近戰。
  - 裝彈完成後可繼續射擊或追擊。

### `CloseCombatState`
- **說明**：遠程敵人近戰狀態，播放攻擊動畫，結束後撤退。

---

## 4. Boss專屬AI

### `BossAI`
- **說明**：Boss專用AI，管理Combo、遠程、突進等特殊行為。
- **設計重點**：  
  - 支援多段Combo、開場Combo、遠程/突進冷卻、跳躍攻擊等。
  - Combo動畫與持續時間可自訂。

### `BossIdleState`
- **說明**：Boss待機狀態，偵測玩家後進入追擊或開場Combo。

### `BossChaseState`
- **說明**：Boss追擊狀態，根據距離決定是否進入Combo、遠程、突進等。
- **設計重點**：  
  - 支援開場Combo流程。
  - 距離遠時自動切換遠程或突進。

### `BossIntroComboState`
- **說明**：Boss開場Combo狀態，依序播放Combo1~3，未完成時可中斷追擊。

### `BossComboState`
- **說明**：Boss一般Combo狀態，播放Combo動畫，結束後隨機決定下一步（撤退、追擊、跳躍攻擊）。

### `BossJumpAttackState`
- **說明**：Boss跳躍攻擊狀態，隨機選擇跳躍攻擊動畫，結束後撤退。

### `BossRangedAttack` / `BossRangedAttackState`
- **說明**：Boss遠程攻擊腳本與狀態，負責發射Boss專屬子彈，動畫結束後回追擊。

### `BossRushState`
- **說明**：Boss突進狀態，朝玩家方向突進，動畫結束後回追擊。

---

## 5. 狀態切換邏輯總結

- **敵人AI** 以「狀態機」為核心，每個狀態獨立負責進入、更新、離開時的行為。
- **近戰/遠程/Boss** 皆有專屬狀態，並可根據距離、血量、動畫進度自動切換。
- **Boss** 有更複雜的行為樹，支援多段Combo、遠程、突進、跳躍攻擊等。
- **避障系統** 以物理偵測為主，支援前方/後方立方體偵測與敵人間避障。

---

## 6. 特色與優缺點

### 優點
- 結構清晰，易於擴充與維護。
- 支援動畫事件、Root Motion、物理移動混用。
- Boss行為豐富，易於調整難度與花樣。
- 狀態切換條件明確，方便調試。

### 可優化建議
- 若需更複雜AI，可考慮行為樹（Behavior Tree）或GOAP。
- 狀態切換可加入冷卻、隨機性，提升不可預測性。
- 避障可進一步優化（如A*路徑規劃）。
- Boss可加入更多特殊攻擊、階段變化。

---

如需針對某一狀態、某一敵人類型、或Boss AI細節進行優化建議或深入解說，請隨時告知！

---

**User**

幫我詳細分析現在 敵人與boss何時啟用root motion何時不啟用

---

**Cursor**

好的，以下詳細分析你目前專案中「敵人」與「Boss」在各種狀態下何時啟用 Root Motion，何時不啟用，並說明其設計邏輯與原因。

---

## 1. Root Motion 控制機制

### 控制方式
- 每個狀態類別都可覆寫 `ShouldUseRootMotion()` 方法，回傳 `true` 或 `false`。
- 進入狀態時（`EnterState`），會呼叫 `enemy.SetRootMotion(ShouldUseRootMotion())`，決定該狀態是否啟用 Root Motion。
- 離開狀態時（`ExitState`），預設會關閉 Root Motion（`enemy.SetRootMotion(false)`）。

---

## 2. 各狀態 Root Motion 啟用情況

### 2.1 一般敵人（RangedEnemy、近戰Enemy）

| 狀態                | Root Motion | 說明                                                         |
|---------------------|-------------|--------------------------------------------------------------|
| IdleState           | **啟用**    | 站立待機，動畫推動角色（Idle動畫通常有Root Motion）           |
| AlertState          | **啟用**    | 警戒動畫通常有Root Motion                                     |
| ChaseState          | **不啟用**  | 追擊時用程式移動（AI計算方向、避障），動畫僅作為視覺效果       |
| AttackState         | **啟用**    | 近戰攻擊時由動畫推動角色（攻擊動畫有Root Motion）             |
| RetreatState        | **不啟用**  | 撤退時用程式移動（AI計算撤退方向、避障），動畫僅作為視覺效果   |
| HitState            | **啟用**    | 受傷/Parry動畫由動畫推動角色                                  |
| DieState            | **啟用**    | 死亡動畫由動畫推動角色                                        |
| AimState            | **啟用**    | 瞄準時通常站立不動，動畫推動                                  |
| ShootState          | **啟用**    | 射擊動畫由動畫推動                                            |
| ReloadState         | **啟用**    | 裝彈動畫由動畫推動                                            |
| CloseCombatState    | **啟用**    | 近戰攻擊動畫由動畫推動                                        |

#### **總結**
- **啟用Root Motion**：站立、警戒、攻擊、受傷、死亡、瞄準、射擊、裝彈、近戰。
- **不啟用Root Motion**：追擊、撤退（這兩種需要AI計算移動方向與避障，不能單靠動畫）。

---

### 2.2 Boss 專屬狀態

| 狀態                    | Root Motion | 說明                                                         |
|-------------------------|-------------|--------------------------------------------------------------|
| BossIdleState           | **啟用**    | Boss待機，動畫推動角色                                       |
| BossChaseState          | **不啟用**  | Boss追擊時用程式移動（AI計算方向、避障）                     |
| BossIntroComboState     | **啟用**    | 開場Combo動畫由動畫推動                                      |
| BossComboState          | **啟用**    | 一般Combo動畫由動畫推動                                      |
| BossJumpAttackState     | **啟用**    | 跳躍攻擊動畫由動畫推動                                      |
| BossRangedAttackState   | **啟用**    | 遠程攻擊動畫由動畫推動                                      |
| BossRushState           | **啟用**    | 突進動畫由動畫推動                                          |

#### **總結**
- **啟用Root Motion**：Boss所有攻擊、待機、遠程、突進、跳躍攻擊等「動畫主導」的狀態。
- **不啟用Root Motion**：Boss追擊（BossChaseState）——這時需要AI計算移動路徑、避障，不能單靠動畫。

---

## 3. 設計邏輯與原因

### 啟用 Root Motion 的情境
- **動畫主導的狀態**：如攻擊、受傷、死亡、裝彈、射擊、Combo、跳躍攻擊等，這些行為需要角色精確跟隨動畫移動（如攻擊步伐、受傷後退、死亡倒地等）。
- **站立/待機/警戒**：角色不移動或只做小幅度動作，Root Motion可確保動畫與物理一致。

### 不啟用 Root Motion 的情境
- **AI主導的移動**：如追擊、撤退，這些狀態下角色需要根據AI計算的方向、速度、避障等進行移動，不能單靠動畫的移動軌跡。
- **需要複雜路徑規劃或避障時**：Root Motion 只會根據動畫移動，無法避開障礙物或其他敵人，必須用程式控制。

---

## 4. 具體程式依據

- 每個狀態類別最後都有 `public override bool ShouldUseRootMotion() => true/false;` 明確指定。
- 進入狀態時會呼叫 `enemy.SetRootMotion(ShouldUseRootMotion())`，即時切換。
- 例如：
  - `ChaseState`、`RetreatState`、`BossChaseState`：`ShouldUseRootMotion() => false`
  - 其他狀態（Idle、Attack、Hit、Die、Aim、Shoot、Reload、BossCombo、BossJumpAttack、BossRangedAttack、BossRush等）：`ShouldUseRootMotion() => true`

---

## 5. 總結表格

| 狀態類型         | Root Motion | 代表狀態（含Boss）                  | 原因/用途                         |
|------------------|-------------|-------------------------------------|------------------------------------|
| 待機/警戒/瞄準   | 啟用        | Idle, Alert, Aim, BossIdle          | 動畫主導，角色不移動或小動作      |
| 攻擊/受傷/死亡   | 啟用        | Attack, Hit, Die, Shoot, Reload, CloseCombat, BossCombo, BossJumpAttack, BossRangedAttack, BossRush | 動畫主導，需精確對齊動畫移動      |
| 追擊/撤退        | 不啟用      | Chase, Retreat, BossChase           | AI主導，需程式計算移動與避障      |

---

如需針對某一狀態的Root Motion細節、動畫設計建議，或想了解如何優化Root Motion與AI移動的切換，歡迎再詢問！

---

**User**

有辦法在不管root motion啟用與否 都限制敵人在與玩家距離10時停止移動並撥放後續動畫或是狀態嗎

---

**Cursor**

可以！  
你可以在**不論 Root Motion 是否啟用**的情況下，強制讓敵人在與玩家距離小於10時停止移動，並切換到你想要的動畫或狀態。  
這種做法在Unity的AI設計中很常見，通常在每個狀態的 `UpdateState` 內加一段距離判斷即可。

---

## 1. 原理說明

- **Root Motion 啟用時**：角色移動由動畫推動，但你仍可在 `UpdateState` 內偵測距離，並在條件成立時切換動畫或狀態（例如直接 `animator.Play("Idle")` 或 `SwitchState(new XXState())`）。
- **Root Motion 未啟用時**：角色移動由程式控制（如 `Move()`），你同樣可在距離判斷時強制 `velocity = Vector3.zero` 或 `Stop()`，並切換動畫或狀態。

---

## 2. 範例程式片段

假設你要在所有狀態下都能這樣判斷，可以在 `EnemyAI` 的 `Update()` 或每個狀態的 `UpdateState` 內加上：

```csharp
// 以EnemyAI.Update()為例
void Update()
{
    if (player == null) return;

    // 距離判斷（不論Root Motion）
    float dist = Vector3.Distance(transform.position, player.position);
    if (dist < 10f)
    {
        Stop(); // velocity = Vector3.zero
        animator.Play("Idle"); // 或你想要的動畫
        // 或切換狀態
        // SwitchState(new SomeState());
        return;
    }

    currentState?.UpdateState(this);
}
```

### 或在每個狀態的 `UpdateState` 內加：

```csharp
public override void UpdateState(EnemyAI enemy)
{
    float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);
    if (dist < 10f)
    {
        enemy.Stop();
        enemy.animator.Play("Idle"); // 或切換狀態
        return;
    }
    // ...原本的狀態邏輯...
}
```

---

## 3. 注意事項

- **Root Motion 狀態下**，`Stop()` 只會清空 velocity，但動畫仍會推動角色。  
  若要完全靜止，需切換到「靜止」動畫（如Idle），或切換狀態，讓Root Motion動畫本身不再推動角色。
- **建議**：  
  - 在距離判斷時，直接切換到IdleState或自訂的「等待」狀態，這樣Root Motion與程式移動都會被正確管理。
  - 若只想暫停移動但不切換狀態，則需確保當前動畫是靜止的（如Idle），否則Root Motion動畫仍會推動角色。

---

## 4. 實務建議

- **最佳做法**：  
  - 在所有會移動的狀態（Chase、Retreat、BossChase等）的 `UpdateState` 內，最前面加上距離判斷與狀態切換。
  - 例如：
    ```csharp
    if (Vector3.Distance(enemy.transform.position, enemy.player.position) < 10f)
    {
        enemy.SwitchState(new IdleState()); // 或自訂狀態
        return;
    }
    ```
- 這樣不論Root Motion是否啟用，敵人都會在距離小於10時停下並進入你想要的行為。

---

## 5. 結論

**完全可以實現！**  
只要在狀態邏輯裡加上距離判斷與狀態/動畫切換，就能不受Root Motion影響，強制敵人在距離10時停下並執行後續行為。

如需範例程式碼整合到你現有的狀態，或想要自訂一個「距離觸發」的共用邏輯，歡迎再指定！

```csharp
// 以EnemyAI.Update()為例
void Update()
{
    if (player == null) return;

    // 距離判斷（不論Root Motion）
    float dist = Vector3.Distance(transform.position, player.position);
    if (dist < 10f)
    {
        Stop(); // velocity = Vector3.zero
        animator.Play("Idle"); // 或你想要的動畫
        // 或切換狀態
        // SwitchState(new SomeState());
        return;
    }

    currentState?.UpdateState(this);
}
```

```csharp
public override void UpdateState(EnemyAI enemy)
{
    float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);
    if (dist < 10f)
    {
        enemy.Stop();
        enemy.animator.Play("Idle"); // 或切換狀態
        return;
    }
    // ...原本的狀態邏輯...
}
```

```csharp
    if (Vector3.Distance(enemy.transform.position, enemy.player.position) < 10f)
    {
        enemy.SwitchState(new IdleState()); // 或自訂狀態
        return;
    }
```

---

**User**

如果動畫全部更改成沒有root motion 但enemy與boss會一直跟著player 但設定一個最小距離 讓enemy與boss永遠與player隔著這段距離 這樣就不會有重疊及碰撞問題 但動畫與狀態還是又能夠正常切換 有辦法做到嗎

---

**Cursor**

完全可以做到！  
這種設計在動作遊戲AI中很常見，稱為「保持最小距離追蹤」或「安全距離追擊」。  
你只要**不用Root Motion**，全部用程式控制移動，並在移動邏輯中加上「最小距離判斷」，就能讓敵人/Boss永遠不會貼臉、不會重疊，且動畫與狀態切換完全不受影響。

---

## 1. 實作原理

- **所有移動都用程式控制**（如 `Move()`），動畫只做視覺效果（不推動角色）。
- **每次移動前都判斷與玩家的距離**，小於最小距離時就不再靠近，只播放Idle或攻擊等動畫。
- **狀態切換**（如進攻、待機、撤退等）依然根據距離、視野等條件判斷，不受Root Motion影響。

---

## 2. 具體程式範例

假設你要讓敵人/Boss與玩家永遠保持 `minDistanceToPlayer`（例如2.5f）：

### 在 EnemyAI.cs 的 Move 方法內：

```csharp
public void Move(Vector3 force, bool lookAtPlayer = false)
{
    if (player != null)
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer < minDistanceToPlayer)
        {
            velocity = Vector3.zero; // 停止移動
            // 可選：播放Idle動畫
            // animator.Play("Idle");
            return;
        }
    }
    // ...原本的移動邏輯...
}
```

### 狀態切換依然正常

例如在ChaseState、BossChaseState等狀態的 `UpdateState` 內：

```csharp
public override void UpdateState(EnemyAI enemy)
{
    float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);
    if (dist < enemy.minDistanceToPlayer)
    {
        // 進入攻擊、Idle或其他狀態
        enemy.SwitchState(new AttackState());
        return;
    }
    // ...追擊邏輯...
}
```

---

## 3. 動畫與狀態切換

- **動畫**：只要你用 `animator.Play()` 或 `SetTrigger()` 控制動畫，動畫會正常切換，不會因為不用Root Motion而失效。
- **狀態**：狀態機切換完全不受Root Motion影響，依然可以根據距離、血量、視野等條件切換。

---

## 4. 優點

- **不會重疊**：敵人/Boss永遠不會貼臉或卡在玩家身上。
- **不會推擠**：完全程式控制移動，碰撞問題大幅減少。
- **動畫自由**：動畫只做視覺，不影響物理與AI。
- **狀態機彈性**：所有狀態切換、攻擊、撤退等都能正常運作。

---

## 5. 注意事項

- **攻擊判定**：近戰攻擊時，請用「距離判斷」或「碰撞盒」來決定是否命中，而不是靠Root Motion推進。
- **多敵人時**：如需避免敵人之間重疊，可加「敵人避障」邏輯（你現有AI已經有這功能）。
- **動畫設計**：攻擊、受傷等動畫不需Root Motion，建議設計為「原地動作」。

---

## 6. 結論

**這種設計完全可行，且更容易控制AI行為與碰撞！**  
只要在移動邏輯加上最小距離判斷，敵人/Boss就能永遠與玩家保持安全距離，動畫與狀態切換也會完全正常。

如需範例程式碼整合、或想要多敵人圍攻時的「分散站位」設計，歡迎再詢問！

```csharp
public void Move(Vector3 force, bool lookAtPlayer = false)
{
    if (player != null)
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer < minDistanceToPlayer)
        {
            velocity = Vector3.zero; // 停止移動
            // 可選：播放Idle動畫
            // animator.Play("Idle");
            return;
        }
    }
    // ...原本的移動邏輯...
}
```

```csharp
public override void UpdateState(EnemyAI enemy)
{
    float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);
    if (dist < enemy.minDistanceToPlayer)
    {
        // 進入攻擊、Idle或其他狀態
        enemy.SwitchState(new AttackState());
        return;
    }
    // ...追擊邏輯...
}
```

---

**User**

但是boss各combo中的位移該如何處理

---

**Cursor**

這是一個很關鍵的問題！  
當你**完全不用Root Motion**時，Boss的Combo攻擊（例如連擊中的衝刺、橫掃等）如果動畫本身有位移，這些「攻擊位移」就**不會自動推動角色**，你必須用程式來同步推動Boss的位置，讓視覺與實際碰撞一致。

---

## 1. 常見做法

### 方法一：**動畫事件（Animation Event）+ 程式推動**
- 在Combo動畫的關鍵幀（如出拳、衝刺起始）加上Animation Event。
- 在事件觸發時，呼叫Boss腳本中的「推進」方法，讓Boss在這一瞬間或一段時間內向前移動。
- 這樣可以精確控制每一段Combo的位移量與時機。

#### 範例
```csharp
// BossComboState.cs
public void OnComboMove(float moveDistance)
{
    // 以Boss面朝方向推進
    Vector3 move = transform.forward * moveDistance;
    transform.position += move;
}
```
- 在動畫編輯器中加一個事件，呼叫 `OnComboMove(2.5f)`，Boss就會在那一幀往前推進2.5單位。

---

### 方法二：**狀態機內持續推進（如衝刺段）**
- 在Combo狀態的 `UpdateState` 內，根據動畫進度（`normalizedTime`）決定是否推進Boss。
- 例如：在Combo動畫的0.2~0.5秒之間，每幀都往前推一點。

#### 範例
```csharp
public override void UpdateState(EnemyAI enemy)
{
    AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
    if (stateInfo.IsName("Combo1"))
    {
        // 只在動畫0.2~0.5區間推進
        if (stateInfo.normalizedTime > 0.2f && stateInfo.normalizedTime < 0.5f)
        {
            float moveSpeed = 8f; // 可依動畫調整
            enemy.transform.position += enemy.transform.forward * moveSpeed * Time.deltaTime;
        }
    }
    // ...其他Combo段落...
}
```

---

### 方法三：**混合方式**
- 有些Combo段只需瞬間位移（用事件），有些需持續滑步（用UpdateState判斷動畫進度持續推進）。
- 這樣可以最大程度還原動畫的動感。

---

## 2. 注意事項

- **位移量要和動畫設計師協調**，確保推進距離與動畫腳步一致。
- **攻擊判定**也要根據推進時機同步（如出拳瞬間才判定傷害）。
- **碰撞偵測**：推進時要考慮障礙物、玩家、地形，避免穿牆或卡住。
- **多段Combo**：每段可設不同推進方式與距離。

---

## 3. 實務建議

- **推薦用法**：  
  - Combo動畫有「大幅位移」時，請動畫師在關鍵幀加Animation Event，程式收到事件後推進Boss。
  - Combo動畫有「滑步」時，在UpdateState根據動畫進度持續推進。
- **測試時**，可在場景中顯示Boss的實際移動軌跡，方便調整推進距離與時機。

---

## 4. 總結

- **不用Root Motion時，Boss Combo的位移必須用程式推動**，常用Animation Event或動畫進度判斷來實現。
- 這樣可以讓Boss的攻擊動作、位移、碰撞、狀態切換都精確同步，且完全不會有重疊或推擠問題。

---

如需具體範例（如Combo1/Combo2/Combo3的推進程式碼），或想要自動化推進管理工具，歡迎再指定！

```csharp
// BossComboState.cs
public void OnComboMove(float moveDistance)
{
    // 以Boss面朝方向推進
    Vector3 move = transform.forward * moveDistance;
    transform.position += move;
}
```

```csharp
public override void UpdateState(EnemyAI enemy)
{
    AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
    if (stateInfo.IsName("Combo1"))
    {
        // 只在動畫0.2~0.5區間推進
        if (stateInfo.normalizedTime > 0.2f && stateInfo.normalizedTime < 0.5f)
        {
            float moveSpeed = 8f; // 可依動畫調整
            enemy.transform.position += enemy.transform.forward * moveSpeed * Time.deltaTime;
        }
    }
    // ...其他Combo段落...
}
```

---

**User**

現在想把撤退狀態改成隨機左右移動而不是後退移動 移動時enemy與boss也要會自動避障 請你幫我改寫

---

**Cursor**

```csharp
// ... existing code ...
    private float timer = 0f;
    private Vector3 strafeDir; // 橫移方向
    private string strafeAnim = "Strafe";
    private int strafeSign = 1; // 1=右, -1=左
// ... existing code ...
    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("RetreatState: 進入橫移狀態");
        // 隨機決定橫移方向
        strafeSign = Random.value < 0.5f ? 1 : -1;
        strafeAnim = strafeSign == 1 ? "StrafeR" : "StrafeL";
        if (enemy.animator.HasState(0, Animator.StringToHash(strafeAnim)))
        {
            enemy.animator.Play(strafeAnim);
        }
        else
        {
            enemy.animator.Play("Idle");
        }
        timer = 0f;
        // 以玩家為基準，取得橫向（右手邊）方向
        Vector3 toPlayer = (enemy.player.position - enemy.transform.position).normalized;
        toPlayer.y = 0f;
        Vector3 right = Vector3.Cross(Vector3.up, toPlayer).normalized;
        strafeDir = right * strafeSign; // 右或左
        enemy.canAutoAttack = false; // 橫移時禁用自動攻擊
    }
// ... existing code ...
    public override void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        // 以玩家為基準，動態取得橫向方向
        Vector3 toPlayer = (enemy.player.position - enemy.transform.position).normalized;
        toPlayer.y = 0f;
        Vector3 right = Vector3.Cross(Vector3.up, toPlayer).normalized;
        strafeDir = right * strafeSign;
        // 計算橫移力
        Vector3 strafeForce = strafeDir * enemy.maxSpeed * 0.5f;
        // 使用前方避障（橫移方向的前方）
        Vector3 boxCenter = enemy.transform.position + strafeDir * enemy.detectionBoxOffset + Vector3.up * enemy.detectionBoxYOffset;
        Quaternion boxRotation = Quaternion.LookRotation(strafeDir);
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, enemy.detectionBoxSize * 0.5f, boxRotation, enemy.obstacleMask);
        Vector3 avoid = Vector3.zero;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == enemy.gameObject) continue;
            Vector3 directionToObstacle = hitCollider.transform.position - enemy.transform.position;
            directionToObstacle.y = 0f;
            if (directionToObstacle.magnitude > 0.1f)
            {
                Vector3 awayFromObstacle = -directionToObstacle.normalized;
                float distance = directionToObstacle.magnitude;
                float distanceRatio = Mathf.Clamp01(1f - (distance / enemy.detectionBoxSize.z));
                float adjustedStrength = enemy.avoidStrength * distanceRatio;
                avoid += awayFromObstacle * adjustedStrength;
            }
        }
        if (avoid.magnitude > enemy.maxSpeed)
            avoid = avoid.normalized * enemy.maxSpeed;
        // 加上敵人避障
        avoid += enemy.EnemyAvoid();
        Vector3 totalForce = strafeForce + avoid;
        // 橫移時面向玩家
        enemy.Move(totalForce, true);
        if (timer >= enemy.retreatTime)
        {
            BossAI bossAI = enemy.GetComponent<BossAI>();
            if (bossAI != null)
            {
                Debug.Log("RetreatState: Boss橫移結束，切換到 BossChaseState");
                enemy.canAutoAttack = true;
                enemy.SwitchState(new BossChaseState());
            }
            else
            {
                Debug.Log("RetreatState: 橫移結束，切換到 ChaseState");
                enemy.canAutoAttack = true;
                enemy.SwitchState(new ChaseState());
            }
        }
    }
// ... existing code ...
```

已經將撤退狀態（RetreatState）改寫為「隨機左右橫移」而非後退，並確保橫移時會自動避障：

- 進入狀態時，隨機決定向左或向右橫移（以玩家為基準的橫向）。
- 橫移時會在橫向前方做障礙物偵測，並自動避障（同時加上敵人避障）。
- 橫移時角色會持續面向玩家，並播放對應的橫移動畫（StrafeR/StrafeL）。
- 狀態結束時，Boss與一般敵人會切回原本的追擊狀態。

你可以直接在Unity測試，敵人與Boss在撤退時會隨機左右橫移且自動避障，完全不會再後退。

如需進一步優化（如多次隨機切換方向、橫移遇牆自動換邊等），或動畫命名需調整，請再告知！

---

**User**

現在敵人在任何狀態中都會與其他敵人互相重疊 請幫我修正

---

**Cursor**

你目前的AI其實已經有「敵人避障」的邏輯（`EnemyAvoid()`），但根據你的描述，**敵人在任何狀態下還是會重疊**，這通常有以下幾個原因：

1. **只有部分狀態有加上敵人避障力**（如Chase/Retreat才有，其他狀態沒加）。
2. **避障力太小或判斷距離太短**，導致推不開。
3. **移動時沒有把敵人避障力加進去**，或是只在某些狀態下加。

---

## 解決方案

### 1. **確保所有會移動的狀態都加上 `EnemyAvoid()`**

請檢查所有有移動的狀態（如ChaseState、RetreatState、BossChaseState、橫移等），在計算移動力時都要加上 `enemy.EnemyAvoid()`，例如：

```csharp
Vector3 seek = enemy.Seek(enemy.player.position);
Vector3 obstacleAvoid = enemy.ObstacleAvoid();
Vector3 enemyAvoid = enemy.EnemyAvoid();
Vector3 totalForce = seek + obstacleAvoid + enemyAvoid;
enemy.Move(totalForce, true);
```

橫移（橫向撤退）也要加上 `enemy.EnemyAvoid()`，你剛剛的RetreatState已經有加。

---

### 2. **調整敵人避障參數**

如果還是會重疊，請嘗試**調大這些參數**（在EnemyAI Inspector可調）：

- `enemyAvoidDistance`（偵測半徑，建議設2~3）
- `enemyAvoidStrength`（推力，建議設10~30）

---

### 3. **Idle/Alert等靜止狀態也要推開？**

如果你希望**即使在Idle/Alert等靜止狀態也不重疊**，可以在這些狀態的`UpdateState`裡加一段：

```csharp
// IdleState.cs
public override void UpdateState(EnemyAI enemy)
{
    // ...原本邏輯...
    // 即使靜止，也推開其他敵人
    Vector3 avoid = enemy.EnemyAvoid();
    if (avoid.magnitude > 0.01f)
        enemy.Move(avoid, false);
}
```
這樣即使不追擊也會自動分開。

---

### 4. **物理層級的碰撞（可選）**

如果你希望**完全物理推開**，可考慮：
- 給敵人加上Rigidbody（已有）+ Collider，並設為非Kinematic。
- 但這樣會有物理彈跳，通常AI遊戲只用「避障力」就夠了。

---

## 總結

1. **所有移動狀態都要加 `EnemyAvoid()`**。
2. **Idle/Alert等靜止狀態也可加推開邏輯**。
3. **調大避障參數**，讓推力夠大。
4. **如需物理推開，需調整Collider/Rigidbody設置**（通常不建議）。

---

### 你可以這樣修改 IdleState/AlertState：

```csharp
public override void UpdateState(EnemyAI enemy)
{
    // ...原本邏輯...
    Vector3 avoid = enemy.EnemyAvoid();
    if (avoid.magnitude > 0.01f)
        enemy.Move(avoid, false);
}
```

**只要這樣，敵人就會自動分開，不會重疊！**

如需我幫你直接改寫IdleState、AlertState等，請告知！

```csharp
Vector3 seek = enemy.Seek(enemy.player.position);
Vector3 obstacleAvoid = enemy.ObstacleAvoid();
Vector3 enemyAvoid = enemy.EnemyAvoid();
Vector3 totalForce = seek + obstacleAvoid + enemyAvoid;
enemy.Move(totalForce, true);
```

```csharp
// IdleState.cs
public override void UpdateState(EnemyAI enemy)
{
    // ...原本邏輯...
    // 即使靜止，也推開其他敵人
    Vector3 avoid = enemy.EnemyAvoid();
    if (avoid.magnitude > 0.01f)
        enemy.Move(avoid, false);
}
```

```csharp
public override void UpdateState(EnemyAI enemy)
{
    // ...原本邏輯...
    Vector3 avoid = enemy.EnemyAvoid();
    if (avoid.magnitude > 0.01f)
        enemy.Move(avoid, false);
}
```

---

**User**

現在敵人追擊時 就算已經進入攻擊範圍也不會馬上攻擊 導致會出現推擠玩家的現象 想讓敵人能夠一進攻擊距離馬上攻擊而不是繼續追擊

---

**Cursor**

你的 `ChaseState` 其實已經有「一進攻擊範圍就切換到攻擊狀態」的判斷邏輯：

```csharp
if (enemy.IsInAttackRange())
{
    // 遠程兵種切Aim，近戰切Attack
    ...
    enemy.SwitchState(new AttackState());
    return;
}
```

**但你還是遇到敵人進攻擊範圍後繼續推擠玩家，原因可能有：**

---

### 1. `attackRange` 設定太小

- `IsInAttackRange()` 只判斷 `Vector3.Distance(transform.position, player.position) < attackRange`。
- 如果 `attackRange` 太小，敵人會貼到玩家身上才切換狀態，導致推擠。

---

### 2. `minDistanceToPlayer` 沒有和 `attackRange` 協調

- `Move()` 內有 `minDistanceToPlayer` 判斷，若這個值比 `attackRange` 小，敵人會先貼臉才停下。
- 建議：`attackRange` ≥ `minDistanceToPlayer`，甚至 `attackRange` 再大一點。

---

### 3. 其他狀態（如Retreat、Idle等）也要協調距離

- 若攻擊結束後又馬上追擊，會重複推擠。
- 建議攻擊結束後有一個「冷卻」或「撤退」狀態。

---

## **最佳修正建議**

### 1. **調大 `attackRange`，讓敵人還沒貼臉就能攻擊**

例如：
- `attackRange = 2.5f`（或依你角色模型大小調整）
- `minDistanceToPlayer = 1.2f`

### 2. **可加一個 buffer，讓敵人更早進入攻擊狀態**

你可以這樣改 `IsInAttackRange()` 或在ChaseState加buffer：

```csharp
float attackBuffer = 0.5f;
if (Vector3.Distance(enemy.transform.position, enemy.player.position) < enemy.attackRange + attackBuffer)
{
    // 切換攻擊狀態
}
```

### 3. **確保攻擊狀態時敵人不再移動**

- `AttackState` 內不要再呼叫 `Move()`，只播放動畫。

---

## **總結**

- 你的ChaseState邏輯沒問題，**只要把`attackRange`設大一點**，敵人就會在還沒貼臉時就切換到攻擊狀態，不會推擠玩家。
- 若還有推擠，請檢查 `minDistanceToPlayer`、`attackRange` 這兩個參數的關係，並適當調整。

---

如需我幫你直接調整 `IsInAttackRange()` 或參數建議，請告知你目前的 `attackRange` 與 `minDistanceToPlayer` 設定值！

```csharp
if (enemy.IsInAttackRange())
{
    // 遠程兵種切Aim，近戰切Attack
    ...
    enemy.SwitchState(new AttackState());
    return;
}
```

```csharp
float attackBuffer = 0.5f;
if (Vector3.Distance(enemy.transform.position, enemy.player.position) < enemy.attackRange + attackBuffer)
{
    // 切換攻擊狀態
}
```

---

**User**

現在minDistanceToPlayer 10  attackRange 15

---

**Cursor**

你的設定如下：

- `minDistanceToPlayer = 10`
- `attackRange = 15`

這樣的設定**理論上是正確的**，因為敵人只要距離玩家小於15就會進入攻擊狀態，距離小於10時就會完全停止移動（`Move()` 內部會 return，不再推進）。

**但如果你還是遇到敵人貼臉推擠玩家，可能有以下原因：**

---

## 1. 狀態切換時機問題

- 如果敵人進入攻擊範圍（<15）時，`ChaseState` 會立刻切到 `AttackState`，但**攻擊動畫還沒開始前，敵人可能還在移動**（特別是Root Motion未啟用時）。
- 請確認 `AttackState` 內**不要再呼叫 `Move()`**，且進入攻擊狀態時有 `enemy.Stop()`。

---

## 2. 進入攻擊狀態後，敵人仍然有殘留速度

- 你可以在 `AttackState.EnterState()` 裡加上 `enemy.Stop();`，確保一進攻擊就完全靜止。

---

## 3. 多敵人同時進攻擊範圍

- 多隻敵人同時進攻擊範圍時，會同時切到攻擊狀態，但攻擊動畫結束後又馬上追擊，導致推擠。
- 建議攻擊後切到「撤退」或「等待」狀態，讓敵人不會馬上又貼上來。

---

## 4. 物理推擠問題

- 如果敵人有Rigidbody/Collider，請確保Rigidbody設為Kinematic，否則物理推擠會干擾AI移動。

---

## **修正建議**

### 1. 確認 `AttackState` 進入時有 `enemy.Stop();`

```csharp
public override void EnterState(EnemyAI enemy)
{
    base.EnterState(enemy);
    Debug.Log("AttackState: 進入攻擊狀態");
    enemy.Stop(); // 這行很重要！
    // ...其他攻擊動畫邏輯...
}
```

### 2. 攻擊結束後不要馬上回到ChaseState

- 可以先切到 `RetreatState` 或 `IdleState`，讓敵人稍微拉開距離再追擊。

---

## **總結**

- 你的參數設置沒問題，問題多半出在**攻擊狀態時沒有完全靜止**或**攻擊後馬上又追擊**。
- 請檢查 `AttackState` 是否有 `enemy.Stop()`，以及攻擊結束後是否有短暫撤退或等待。

---

如需我幫你直接改寫 `AttackState` 或設計攻擊後撤退/等待邏輯，請告訴我！

```csharp
public override void EnterState(EnemyAI enemy)
{
    base.EnterState(enemy);
    Debug.Log("AttackState: 進入攻擊狀態");
    enemy.Stop(); // 這行很重要！
    // ...其他攻擊動畫邏輯...
}
```

---

**User**

直接幫我改寫

---

**Cursor**

```csharp
// ... existing code ...
    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        Debug.Log("AttackState: 進入攻擊狀態");
        enemy.Stop(); // 進入攻擊時完全靜止
        enemy.animator.ResetTrigger("Attack"); // 先重置Trigger
        enemy.animator.SetTrigger("Attack");   // 再設置Trigger
        hasAttacked = false;
        moveTimer = 0f;
        enemy.canAutoAttack = false;
    }
// ... existing code ...
    public override void UpdateState(EnemyAI enemy)
    {
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        if (!hasAttacked && stateInfo.IsName("Attack"))
        {
            hasAttacked = true;
            Debug.Log("AttackState: 攻擊動畫開始播放");
            enemy.Stop(); // 再次保險，攻擊動畫開始時也停止
        }
        // 等待攻擊動畫結束
        if (hasAttacked && stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 1.0f)
        {
            Debug.Log("AttackState: 攻擊動畫結束，切換到 RetreatState");
            enemy.SwitchState(new RetreatState()); // 攻擊後自動撤退
        }
    }
// ... existing code ...
```

已經幫你強化並改寫 `AttackState`：

- 進入攻擊狀態時會**完全停止移動**（`enemy.Stop()`）。
- 攻擊動畫開始時再次保險停止移動。
- **攻擊動畫結束後自動切換到 `RetreatState`**，讓敵人攻擊後會撤退，不會馬上又追擊推擠玩家。

這樣可以大幅減少敵人貼臉推擠玩家的現象，並讓戰鬥節奏更自然。

如需進一步優化（如攻擊後等待、隨機撤退距離等），請再告知！

---

**User**

boss有些combo會切換遠程武器並且進行遠程攻擊 該如何處理

---

**Cursor**

這是動作遊戲Boss常見的設計需求！  
你可以讓Boss在Combo流程中**切換武器**並**執行遠程攻擊**，通常有以下幾種做法，這裡給你一個結構化、易於擴充的方案：

---

## 1. **Combo動畫中插入遠程攻擊的時機**

- **動畫事件（Animation Event）**：在Combo動畫的特定幀加事件，通知程式「現在切換武器」或「現在發射遠程攻擊」。
- **狀態機流程**：在BossComboState的`UpdateState`根據動畫進度或Combo步驟，切換到遠程攻擊狀態。

---

## 2. **武器切換的實作方式**

### A. **動畫驅動切換（推薦）**
- 讓動畫師在Combo動畫中設計「收刀→拔弓」或「切換武器」的動作。
- 在動畫事件中呼叫Boss腳本的`SwitchWeapon("ranged")`或`SwitchWeapon("melee")`方法，切換武器模型顯示。

### B. **程式控制切換**
- Boss腳本中有`SetWeaponActive(string weaponName, bool active)`，根據事件啟用/關閉近戰或遠程武器的GameObject。

---

## 3. **遠程攻擊的觸發**

- 在動畫事件或Combo流程中呼叫Boss的遠程攻擊方法（如`FireBossProjectile()`）。
- 你已經有`BossRangedAttack`腳本，直接呼叫即可。

---

## 4. **範例流程（動畫事件法）**

1. **Combo動畫進行到遠程攻擊段時**，動畫事件呼叫：
   - `SwitchWeapon("ranged")`（顯示遠程武器，隱藏近戰武器）
   - `FireBossProjectile()`（發射遠程攻擊）
2. **Combo結束時**，動畫事件呼叫：
   - `SwitchWeapon("melee")`（切回近戰武器）

---

## 5. **程式範例**

### BossAI.cs
```csharp
public GameObject meleeWeapon;
public GameObject rangedWeapon;

public void SwitchWeapon(string type)
{
    if (type == "melee")
    {
        meleeWeapon.SetActive(true);
        rangedWeapon.SetActive(false);
    }
    else if (type == "ranged")
    {
        meleeWeapon.SetActive(false);
        rangedWeapon.SetActive(true);
    }
}

// 動畫事件呼叫
public void FireBossProjectile()
{
    var ranged = GetComponent<BossRangedAttack>();
    if (ranged != null)
        ranged.FireBossProjectile();
}
```

### 動畫事件設置
- 在Combo動畫的「切換武器」幀加事件：`SwitchWeapon("ranged")`
- 在「發射」幀加事件：`FireBossProjectile()`
- Combo結束時加事件：`SwitchWeapon("melee")`

---

## 6. **如果用狀態機切換（進階）**

- 在`BossComboState`流程中，當Combo步驟需要遠程攻擊時，`enemy.SwitchState(new BossRangedAttackState())`，遠程攻擊結束後再回Combo或Chase。

---

## 7. **總結**

- **推薦用法**：用動畫事件精確控制武器切換與遠程攻擊時機，程式只需提供切換與發射方法。
- **狀態機法**：如需更複雜AI，可在Combo流程中插入遠程攻擊狀態。

---

如需具體程式碼整合到你現有BossAI，或需要動畫事件設置教學，請告訴我你想用哪種方式（動畫事件 or 狀態機流程），我可以直接幫你寫好！

```csharp
public GameObject meleeWeapon;
public GameObject rangedWeapon;

public void SwitchWeapon(string type)
{
    if (type == "melee")
    {
        meleeWeapon.SetActive(true);
        rangedWeapon.SetActive(false);
    }
    else if (type == "ranged")
    {
        meleeWeapon.SetActive(false);
        rangedWeapon.SetActive(true);
    }
}

// 動畫事件呼叫
public void FireBossProjectile()
{
    var ranged = GetComponent<BossRangedAttack>();
    if (ranged != null)
        ranged.FireBossProjectile();
}
```

---

**User**

使用動畫事件方式 並且整合到現有BossAI 也需要動畫事件設置教學

---

**Cursor**

沒問題！以下會**完整教你如何用動畫事件方式**，讓Boss在Combo中切換武器並發射遠程攻擊，並**整合到你現有的BossAI**，最後還會附上Unity動畫事件設置教學。

---

# 1. BossAI 腳本整合

### 1.1 BossAI.cs 新增武器切換與遠程攻擊方法

請在 `BossAI.cs` 內加上如下欄位與方法：

```csharp
// BossAI.cs

public GameObject meleeWeapon;   // 指到近戰武器 GameObject
public GameObject rangedWeapon;  // 指到遠程武器 GameObject

// 動畫事件：切換武器
public void SwitchWeapon(string type)
{
    if (type == "melee")
    {
        if (meleeWeapon != null) meleeWeapon.SetActive(true);
        if (rangedWeapon != null) rangedWeapon.SetActive(false);
    }
    else if (type == "ranged")
    {
        if (meleeWeapon != null) meleeWeapon.SetActive(false);
        if (rangedWeapon != null) rangedWeapon.SetActive(true);
    }
}

// 動畫事件：發射遠程攻擊
public void FireBossProjectile()
{
    var ranged = GetComponent<BossRangedAttack>();
    if (ranged != null)
        ranged.FireBossProjectile();
}
```

### 1.2 在 Inspector 指定武器物件

- 將Boss的近戰武器（如刀、劍）和遠程武器（如弓、槍）拖到 `meleeWeapon` 和 `rangedWeapon` 欄位。
- 預設只顯示近戰武器，遠程武器設為不啟用（Inactive）。

---

# 2. 動畫事件設置教學

### 2.1 打開動畫編輯器

1. 選擇Boss角色 → Window → Animation → Animation。
2. 選擇你要編輯的Combo動畫（如 Combo2）。

### 2.2 插入動畫事件

1. 在時間軸上**移動到你想切換武器的那一幀**（例如Boss收刀準備拔弓的瞬間）。
2. 點擊「Add Event」按鈕（小齒輪圖示）。
3. 在彈出的事件欄位輸入函數名稱：
   - 切換遠程武器：`SwitchWeapon`
   - 參數填 `"ranged"`
4. 在發射遠程攻擊的那一幀再加一個事件：
   - 函數名稱：`FireBossProjectile`
   - 無需參數
5. Combo結束或要切回近戰時，再加一個事件：
   - 函數名稱：`SwitchWeapon`
   - 參數填 `"melee"`

### 2.3 動畫事件參數設置

- `SwitchWeapon` 事件要點擊事件欄位，選擇 `string`，然後填入 `"ranged"` 或 `"melee"`。
- `FireBossProjectile` 事件不用填參數。

---

# 3. 動畫事件流程範例

| 幀 | 動畫內容         | 動畫事件              | 參數      |
|----|------------------|-----------------------|-----------|
| 10 | 收刀             | SwitchWeapon          | "ranged"  |
| 20 | 拉弓/舉槍        |                       |           |
| 30 | 發射             | FireBossProjectile    |           |
| 40 | 收弓/收槍        | SwitchWeapon          | "melee"   |

---

# 4. BossRangedAttack.cs 不需更動

你現有的 `BossRangedAttack` 腳本已經有 `FireBossProjectile()`，動畫事件會自動呼叫。

---

# 5. 測試與調整

- 進入遊戲，當Boss播放Combo動畫到指定幀時，會自動切換武器並發射遠程攻擊。
- 如需多次切換，只要在動畫對應幀加事件即可。

---

## 補充：如果動畫事件沒出現在下拉選單？

- 請確保 `SwitchWeapon(string type)` 和 `FireBossProjectile()` 是**public**，且在Boss掛載的腳本上。
- 參數型別要正確（string）。

---

## 總結

1. BossAI.cs 加入 `SwitchWeapon(string)` 和 `FireBossProjectile()`。
2. 在Inspector指定武器GameObject。
3. 在動畫對應幀加動畫事件，填入函數名與參數。
4. 測試即可。

---

如需範例圖、或遇到動畫事件找不到函數等Unity常見問題，歡迎再問！

```csharp
// BossAI.cs

public GameObject meleeWeapon;   // 指到近戰武器 GameObject
public GameObject rangedWeapon;  // 指到遠程武器 GameObject

// 動畫事件：切換武器
public void SwitchWeapon(string type)
{
    if (type == "melee")
    {
        if (meleeWeapon != null) meleeWeapon.SetActive(true);
        if (rangedWeapon != null) rangedWeapon.SetActive(false);
    }
    else if (type == "ranged")
    {
        if (meleeWeapon != null) meleeWeapon.SetActive(false);
        if (rangedWeapon != null) rangedWeapon.SetActive(true);
    }
}

// 動畫事件：發射遠程攻擊
public void FireBossProjectile()
{
    var ranged = GetComponent<BossRangedAttack>();
    if (ranged != null)
        ranged.FireBossProjectile();
}
```

---

**User**

幫我直接加入到對應腳本

---

**Cursor**

```csharp
// ... existing code ...
    // === 武器切換與遠程攻擊動畫事件用 ===
    public GameObject meleeWeapon;   // 指到近戰武器 GameObject
    public GameObject rangedWeapon;  // 指到遠程武器 GameObject

    // 動畫事件：切換武器
    public void SwitchWeapon(string type)
    {
        if (type == "melee")
        {
            if (meleeWeapon != null) meleeWeapon.SetActive(true);
            if (rangedWeapon != null) rangedWeapon.SetActive(false);
        }
        else if (type == "ranged")
        {
            if (meleeWeapon != null) meleeWeapon.SetActive(false);
            if (rangedWeapon != null) rangedWeapon.SetActive(true);
        }
    }

    // 動畫事件：發射遠程攻擊
    public void FireBossProjectile()
    {
        var ranged = GetComponent<BossRangedAttack>();
        if (ranged != null)
            ranged.FireBossProjectile();
    }
// ... existing code ...
```

已經幫你直接將動畫事件用的功能整合到 `BossAI.cs`：

- 新增了 `meleeWeapon` 和 `rangedWeapon` 兩個 GameObject 欄位（請在 Inspector 指定）。
- 新增 `public void SwitchWeapon(string type)` 方法，動畫事件可用 `"melee"` 或 `"ranged"` 參數切換武器顯示。
- 新增 `public void FireBossProjectile()` 方法，動畫事件可直接呼叫發射遠程攻擊。

你只要在動畫對應幀加上這兩個事件，Boss就能在Combo中切換武器並發射遠程攻擊！

如需動畫事件設置圖解或遇到Unity動畫事件找不到函數等問題，歡迎再問！

---

**User**

boss右手拿近戰武器左手拿弓 近戰時顯示近戰武器 遠距離攻擊時顯示弓 這樣可以嗎

---

**Cursor**

可以，這種設計完全沒問題，而且很常見！  
你可以讓Boss「右手一直拿近戰武器，左手只在遠程攻擊時顯示弓」，平時弓是隱藏的，遠程攻擊時才顯示弓，攻擊完再隱藏。

---

## 具體做法

1. **近戰狀態**  
   - 右手近戰武器（如刀、劍）**一直顯示**（`meleeWeapon.SetActive(true)`）。
   - 左手弓**隱藏**（`rangedWeapon.SetActive(false)`）。

2. **遠程攻擊狀態**  
   - 右手近戰武器**依然顯示**（或可選擇隱藏，視動畫需求）。
   - 左手弓**顯示**（`rangedWeapon.SetActive(true)`）。

3. **動畫事件控制**  
   - 在遠程攻擊動畫的「拉弓」幀加事件：`rangedWeapon.SetActive(true)`。
   - 在遠程攻擊動畫的「收弓」幀加事件：`rangedWeapon.SetActive(false)`。

---

## 你現有的SwitchWeapon方法可這樣調整（可選）

如果你想讓近戰武器一直顯示，只控制弓的顯示：

```csharp
public void ShowBow(bool show)
{
    if (rangedWeapon != null) rangedWeapon.SetActive(show);
}
```
- 動畫事件：拉弓時呼叫 `ShowBow(true)`，收弓時呼叫 `ShowBow(false)`。

---

## 動畫事件設置建議

- **近戰動畫**：不需加事件，弓預設隱藏。
- **遠程動畫**：
  - 拉弓幀：加事件 `ShowBow(true)`
  - 發射幀：加事件 `FireBossProjectile()`
  - 收弓幀：加事件 `ShowBow(false)`

---

## 總結

- 右手近戰武器可一直顯示，左手弓只在遠程攻擊時顯示，這種設計完全沒問題。
- 只需用動畫事件控制弓的顯示/隱藏即可，近戰武器不用特別隱藏。

如需我幫你直接加 `ShowBow` 方法，或動畫事件設置範例，請告訴我！

```csharp
public void ShowBow(bool show)
{
    if (rangedWeapon != null) rangedWeapon.SetActive(show);
}
```

---

**User**

近戰只顯示左手武器 遠程時只顯示右手武器

---

**Cursor**

明白！你要的是：

- **近戰攻擊時**：只顯示「左手武器」（右手武器隱藏）
- **遠程攻擊時**：只顯示「右手武器」（左手武器隱藏）

這種切換方式完全可以用動畫事件+簡單程式實現。

---

## 1. BossAI 腳本調整

請將 BossAI.cs 的武器欄位命名為 `leftWeapon` 和 `rightWeapon`，並新增一個動畫事件專用方法：

```csharp
public GameObject leftWeapon;   // 左手武器
public GameObject rightWeapon;  // 右手武器

// 動畫事件：切換武器顯示
public void ShowWeapon(string hand)
{
    if (hand == "left")
    {
        if (leftWeapon != null) leftWeapon.SetActive(true);
        if (rightWeapon != null) rightWeapon.SetActive(false);
    }
    else if (hand == "right")
    {
        if (leftWeapon != null) leftWeapon.SetActive(false);
        if (rightWeapon != null) rightWeapon.SetActive(true);
    }
}
```

---

## 2. 動畫事件設置

- **近戰動畫**：在進入攻擊幀加事件 `ShowWeapon("left")`
- **遠程動畫**：在進入攻擊幀加事件 `ShowWeapon("right")`
- 其他時候（如待機、移動）可依需求預設顯示一隻手的武器，或在Idle動畫加事件。

---

## 3. Inspector 設定

- 將Boss左手武器（如刀、劍）拖到 `leftWeapon`
- 將Boss右手武器（如弓、槍）拖到 `rightWeapon`
- 預設兩者都設為Active或只顯示一個，進入遊戲時會根據動畫事件自動切換

---

## 4. FireBossProjectile 不需更動

遠程攻擊時動畫事件照常呼叫 `FireBossProjectile()` 即可。

---

## 5. 範例流程

| 狀態   | 動畫事件             | 效果                  |
|--------|----------------------|-----------------------|
| 近戰   | ShowWeapon(\"left\") | 只顯示左手武器        |
| 遠程   | ShowWeapon(\"right\")| 只顯示右手武器        |

---

## 6. 直接幫你加到 BossAI.cs

如果你需要我直接幫你加進去，請回覆「請幫我加到BossAI.cs」即可！

---

如需動畫事件設置圖解或有其他武器顯示需求，歡迎再問！

```csharp
public GameObject leftWeapon;   // 左手武器
public GameObject rightWeapon;  // 右手武器

// 動畫事件：切換武器顯示
public void ShowWeapon(string hand)
{
    if (hand == "left")
    {
        if (leftWeapon != null) leftWeapon.SetActive(true);
        if (rightWeapon != null) rightWeapon.SetActive(false);
    }
    else if (hand == "right")
    {
        if (leftWeapon != null) leftWeapon.SetActive(false);
        if (rightWeapon != null) rightWeapon.SetActive(true);
    }
}
```

---

**User**

請幫我加到BossAI.cs

---

**Cursor**

```csharp
// ... existing code ...
    // === 左右手武器切換動畫事件用 ===
    public GameObject leftWeapon;   // 左手武器
    public GameObject rightWeapon;  // 右手武器

    // 動畫事件：切換武器顯示
    public void ShowWeapon(string hand)
    {
        if (hand == "left")
        {
            if (leftWeapon != null) leftWeapon.SetActive(true);
            if (rightWeapon != null) rightWeapon.SetActive(false);
        }
        else if (hand == "right")
        {
            if (leftWeapon != null) leftWeapon.SetActive(false);
            if (rightWeapon != null) rightWeapon.SetActive(true);
        }
    }
// ... existing code ...
```

已經幫你將以下內容加入到 `BossAI.cs`：

- 新增 `leftWeapon`（左手武器）與 `rightWeapon`（右手武器）兩個 GameObject 欄位。
- 新增 `ShowWeapon(string hand)` 方法，動畫事件可用 `"left"` 或 `"right"` 參數切換顯示左手或右手武器。

你只要在動畫對應幀加上動畫事件 `ShowWeapon("left")` 或 `ShowWeapon("right")`，Boss就會自動只顯示對應手的武器。

如需動畫事件設置圖解或有其他需求，歡迎再問！

---

**User**

讓事件變簡單一點 要使用遠程攻擊時隱藏右手武器 顯示左手武器 其他時間都顯示右手武器

---

**Cursor**

```csharp
// ... existing code ...
    // 動畫事件：簡化武器顯示切換
    public void ShowRangedWeapon(bool show)
    {
        if (show)
        {
            if (rightWeapon != null) rightWeapon.SetActive(false);
            if (leftWeapon != null) leftWeapon.SetActive(true);
        }
        else
        {
            if (rightWeapon != null) rightWeapon.SetActive(true);
            if (leftWeapon != null) leftWeapon.SetActive(false);
        }
    }
// ... existing code ...
```

已經幫你簡化事件切換方式，現在只需一個動畫事件方法：

```csharp
public void ShowRangedWeapon(bool show)
{
    if (show)
    {
        if (rightWeapon != null) rightWeapon.SetActive(false);
        if (leftWeapon != null) leftWeapon.SetActive(true);
    }
    else
    {
        if (rightWeapon != null) rightWeapon.SetActive(true);
        if (leftWeapon != null) leftWeapon.SetActive(false);
    }
}
```

---

## 使用方式

- **遠程攻擊動畫**：在「拉弓」幀加事件 `ShowRangedWeapon(true)`（只顯示左手武器，右手武器隱藏）
- **其他所有動畫**（近戰、待機、移動等）：在進入時加事件 `ShowRangedWeapon(false)`（只顯示右手武器，左手武器隱藏）

這樣你只要在需要遠程攻擊的動畫幀加一次事件，其他時間都用 `ShowRangedWeapon(false)`，事件設置最簡單！

如需動畫事件設置圖解或有其他需求，歡迎再問！

```csharp
public void ShowRangedWeapon(bool show)
{
    if (show)
    {
        if (rightWeapon != null) rightWeapon.SetActive(false);
        if (leftWeapon != null) leftWeapon.SetActive(true);
    }
    else
    {
        if (rightWeapon != null) rightWeapon.SetActive(true);
        if (leftWeapon != null) leftWeapon.SetActive(false);
    }
}
```

---

**User**

動畫事件設置圖解

---

**Cursor**

以下是Unity動畫事件設置的圖解與步驟說明，讓你能輕鬆將 `ShowRangedWeapon(bool)` 事件加到Boss的動畫上：

---

## 1. 開啟動畫編輯器

1. 選擇你的Boss角色（Hierarchy中點選Boss物件）。
2. 在Unity上方選單點選 `Window > Animation > Animation`，打開動畫視窗。
3. 在Animation視窗中，選擇你要編輯的動畫（如「遠程攻擊」動畫）。

---

## 2. 在動畫時間軸上插入事件

1. 在動畫時間軸上**拖曳滑桿到你想要加事件的那一幀**（例如拉弓的瞬間）。
2. 點擊動畫視窗下方的「Add Event」按鈕（如下圖紅框處，像一個小標記/齒輪）。

```
┌─────────────────────────────┐
│ Animation: 遠程攻擊         │
├─────────────────────────────┤
│ [■----------------------]   │ ← 時間軸（拖曳滑桿到目標幀）
│                             │
│   [Add Event]  ← 按這裡     │
└─────────────────────────────┘
```

---

## 3. 設定事件函數與參數

1. 點擊剛剛加上的事件標記（會出現在時間軸下方）。
2. 在Inspector右側會出現「Function」下拉選單，選擇 `ShowRangedWeapon`。
3. 參數型別選擇 `bool`，並勾選（true）或不勾選（false）：
   - **遠程攻擊動畫**：拉弓幀設 `true`
   - **近戰/待機/移動動畫**：進入時設 `false`

如下圖：

```
┌───────────── Inspector ─────────────┐
│ Event:                              │
│ Function: ShowRangedWeapon          │
│ Float:                              │
│ Int:                                │
│ String:                             │
│ Bool:   [✔]                         │ ← 勾選true（遠程攻擊時）
└─────────────────────────────────────┘
```

---

## 4. 動畫事件流程範例

| 動畫         | 幀位置 | 事件函數           | 參數      | 效果                  |
|--------------|--------|--------------------|-----------|-----------------------|
| 遠程攻擊     | 10     | ShowRangedWeapon   | true      | 顯示左手武器（弓）    |
| 近戰攻擊     | 0      | ShowRangedWeapon   | false     | 顯示右手武器（刀）    |
| 待機/移動    | 0      | ShowRangedWeapon   | false     | 顯示右手武器（刀）    |

---

## 5. 注意事項

- `ShowRangedWeapon` 必須是 Boss 掛載腳本的 public 方法，且參數型別正確（bool）。
- 如果事件沒出現在下拉選單，請先播放一次遊戲讓Unity刷新，或重新編譯腳本。

---

如需實際Unity介面截圖，請告訴我你的Unity版本（如2021/2022），我可以提供更精確的圖示說明！

```plaintext
┌─────────────────────────────┐
│ Animation: 遠程攻擊         │
├─────────────────────────────┤
│ [■----------------------]   │ ← 時間軸（拖曳滑桿到目標幀）
│                             │
│   [Add Event]  ← 按這裡     │
└─────────────────────────────┘
```

```plaintext
┌───────────── Inspector ─────────────┐
│ Event:                              │
│ Function: ShowRangedWeapon          │
│ Float:                              │
│ Int:                                │
│ String:                             │
│ Bool:   [✔]                         │ ← 勾選true（遠程攻擊時）
└─────────────────────────────────────┘
```

---

**User**

這樣拉弓時右手武器會隱藏嗎

---

**Cursor**

是的，**這樣設計拉弓時右手武器會自動隱藏**！

### 原理說明

你在動畫事件加上  
`ShowRangedWeapon(true)`  
時，會執行這段程式：

```csharp
public void ShowRangedWeapon(bool show)
{
    if (show)
    {
        if (rightWeapon != null) rightWeapon.SetActive(false); // 右手武器隱藏
        if (leftWeapon != null) leftWeapon.SetActive(true);    // 左手武器顯示
    }
    else
    {
        if (rightWeapon != null) rightWeapon.SetActive(true);  // 右手武器顯示
        if (leftWeapon != null) leftWeapon.SetActive(false);   // 左手武器隱藏
    }
}
```

---

## 實際效果

- **拉弓動畫時**（動畫事件設 `ShowRangedWeapon(true)`）：
  - 右手武器（如刀、劍）會自動隱藏
  - 左手武器（如弓）會自動顯示

- **其他動畫時**（動畫事件設 `ShowRangedWeapon(false)`）：
  - 右手武器顯示
  - 左手武器隱藏

---

## 小提醒

- 請確認你在Inspector有正確指定 rightWeapon（右手武器）和 leftWeapon（左手武器）。
- 動畫事件設置正確時，拉弓那一幀右手武器就會自動消失，弓會出現。

---

如有特殊需求（例如拉弓時右手還要握著刀，或弓和刀同時顯示/隱藏），也可以再調整！

```csharp
public void ShowRangedWeapon(bool show)
{
    if (show)
    {
        if (rightWeapon != null) rightWeapon.SetActive(false); // 右手武器隱藏
        if (leftWeapon != null) leftWeapon.SetActive(true);    // 左手武器顯示
    }
    else
    {
        if (rightWeapon != null) rightWeapon.SetActive(true);  // 右手武器顯示
        if (leftWeapon != null) leftWeapon.SetActive(false);   // 左手武器隱藏
    }
}
```

---

**User**

弓要出現不只一禎

---

**Cursor**

完全沒問題！  
**弓（左手武器）要出現不只一幀，只要在「拉弓開始」時加 `ShowRangedWeapon(true)`，在「拉弓結束或收弓」時加 `ShowRangedWeapon(false)`，弓就會在這段動畫期間一直顯示。**

---

## 操作步驟

1. **在拉弓動畫的「拉弓開始」幀**  
   加動畫事件：`ShowRangedWeapon(true)`  
   → 弓會從這一幀開始顯示，右手武器隱藏。

2. **在拉弓動畫的「收弓/結束」幀**  
   加動畫事件：`ShowRangedWeapon(false)`  
   → 弓會從這一幀開始隱藏，右手武器重新顯示。

---

## 圖解流程

| 幀 | 動畫內容         | 動畫事件                | 效果                  |
|----|------------------|-------------------------|-----------------------|
| 10 | 拉弓開始         | ShowRangedWeapon(true)  | 弓顯示，右手武器隱藏  |
| 11~39 | 拉弓/瞄準/發射 | 無                      | 弓持續顯示            |
| 40 | 收弓/結束        | ShowRangedWeapon(false) | 弓隱藏，右手武器顯示  |

---

## 小提醒

- 只要**ShowRangedWeapon(true)和ShowRangedWeapon(false)之間的所有動畫幀，弓都會持續顯示**。
- 不需要每一幀都加事件，只要在「開始」和「結束」各加一次即可。

---

如需更進階的控制（例如多段拉弓、特殊過場），也可以用同樣方式多次切換。  
有其他動畫需求也歡迎再問！

---

**User**

那其他的狀態或動畫該如何設置

---

**Cursor**

這是一個很好的問題！  
你只需要在**所有「非遠程攻擊」的動畫**（如待機、移動、近戰攻擊、受傷、死亡等）**的開頭幀**加上動畫事件：  
`ShowRangedWeapon(false)`  
這樣就能保證Boss在這些狀態下只顯示右手武器（近戰武器），弓會自動隱藏。

---

## 建議設置方式

### 1. **遠程攻擊動畫**
- **拉弓開始幀**：`ShowRangedWeapon(true)`
- **收弓/結束幀**：`ShowRangedWeapon(false)`

### 2. **近戰攻擊動畫**
- **開頭幀**：`ShowRangedWeapon(false)`

### 3. **待機/移動動畫**
- **開頭幀**：`ShowRangedWeapon(false)`

### 4. **受傷/死亡動畫**
- **開頭幀**：`ShowRangedWeapon(false)`

### 5. **特殊狀態（如撤退、警戒等）**
- **開頭幀**：`ShowRangedWeapon(false)`

---

## 圖解範例

| 動畫         | 幀位置 | 動畫事件                | 效果                  |
|--------------|--------|-------------------------|-----------------------|
| 遠程攻擊     | 10     | ShowRangedWeapon(true)  | 弓顯示，右手武器隱藏  |
| 遠程攻擊     | 40     | ShowRangedWeapon(false) | 弓隱藏，右手武器顯示  |
| 近戰攻擊     | 0      | ShowRangedWeapon(false) | 只顯示右手武器        |
| 待機         | 0      | ShowRangedWeapon(false) | 只顯示右手武器        |
| 移動         | 0      | ShowRangedWeapon(false) | 只顯示右手武器        |
| 受傷         | 0      | ShowRangedWeapon(false) | 只顯示右手武器        |
| 死亡         | 0      | ShowRangedWeapon(false) | 只顯示右手武器        |

---

## 小提醒

- **只要在每個動畫的開頭幀設一次即可**，不需要每一幀都設。
- 這樣即使Boss被打斷、狀態切換，也能保證武器顯示正確。
- 如果有特殊動畫（如Boss變身、特殊技能），也可依需求加上對應事件。

---

## 總結

- **遠程攻擊動畫**：ShowRangedWeapon(true) → ShowRangedWeapon(false)
- **其他所有動畫**：開頭幀加ShowRangedWeapon(false)

這樣就能保證Boss在所有狀態下武器顯示都正確、自然！

如需更細緻的設置建議或遇到特殊動畫狀況，歡迎再問！

---

**User**

動畫有多段 並且是由transition連接該如何設定

---

**Cursor**

這是Unity動畫系統常見的進階問題！  
**只要你的動畫是由多段（多個State）透過Transition連接，建議在「每個動畫State的開頭幀」都加上正確的動畫事件**，這樣不論從哪個Transition進入，都能保證武器顯示正確。

---

## 1. 原理說明

- **動畫事件只會在進入該動畫片段（State）時觸發**，不會自動延續前一個動畫的事件效果。
- 如果你有多個動畫片段（如「拉弓準備」→「拉弓瞄準」→「發射」→「收弓」），每個片段都應在**開頭幀**加上正確的事件。

---

## 2. 設定建議

### 例：Boss遠程攻擊動畫流程

假設你的Animator有這樣的狀態機：

```
Idle → [拉弓準備] → [拉弓瞄準] → [發射] → [收弓] → Idle
```

#### 設定方式：

| 動畫State      | 開頭幀動畫事件           | 效果                        |
|----------------|-------------------------|-----------------------------|
| 拉弓準備       | ShowRangedWeapon(true)  | 弓顯示，右手武器隱藏        |
| 拉弓瞄準       | ShowRangedWeapon(true)  | 弓持續顯示                  |
| 發射           | ShowRangedWeapon(true)  | 弓持續顯示                  |
| 收弓           | ShowRangedWeapon(false) | 弓隱藏，右手武器顯示        |
| Idle           | ShowRangedWeapon(false) | 弓隱藏，右手武器顯示        |

- **每個State的開頭幀都加事件**，即使是自動Transition進來也能正確切換武器。
- 如果某些State（如「拉弓瞄準」）很短，也建議加事件，保險起見。

---

## 3. Transition時機

- **Transition（過渡）時，進入新動畫State的第一幀就會觸發動畫事件**。
- 不論是自動過渡還是條件過渡，只要進入該State就會執行事件。

---

## 4. 圖解

```
[Idle] --(ShowRangedWeapon(false))-->
   |
   v
[拉弓準備] --(ShowRangedWeapon(true))-->
   |
   v
[拉弓瞄準] --(ShowRangedWeapon(true))-->
   |
   v
[發射] --(ShowRangedWeapon(true))-->
   |
   v
[收弓] --(ShowRangedWeapon(false))-->
   |
   v
[Idle] --(ShowRangedWeapon(false))-->
```

---

## 5. 小提醒

- **每個動畫State的開頭幀都要設事件**，不要只設在第一個State。
- 這樣不論從哪個Transition進入（如被打斷、重播、分支），武器顯示都不會錯亂。
- 如果有特殊分支（如Boss被打斷直接進Idle），Idle也要加ShowRangedWeapon(false)。

---

## 6. 實務經驗

- 這是專業遊戲開發常用的做法，確保動畫狀態機複雜時，武器/特效/表情等都能正確切換。
- 只要**每個State開頭幀都設好事件**，就不會出現武器顯示錯亂的情況。

---

如需針對你的Animator結構給具體建議，或需要圖示操作步驟，請提供你的動畫狀態機結構圖或描述！

```plaintext
Idle → [拉弓準備] → [拉弓瞄準] → [發射] → [收弓] → Idle
```

```plaintext
[Idle] --(ShowRangedWeapon(false))-->
   |
   v
[拉弓準備] --(ShowRangedWeapon(true))-->
   |
   v
[拉弓瞄準] --(ShowRangedWeapon(true))-->
   |
   v
[發射] --(ShowRangedWeapon(true))-->
   |
   v
[收弓] --(ShowRangedWeapon(false))-->
   |
   v
[Idle] --(ShowRangedWeapon(false))-->
```

