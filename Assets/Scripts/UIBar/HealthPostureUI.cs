using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using CodeMonkey;

/// <summary>
/// 控制生命值與架勢條 UI 顯示的 MonoBehaviour。
/// 負責根據 HealthPostureSystem 的資料更新 UI 畫面。
/// </summary>
public class HealthPostureUI : MonoBehaviour {

    // -- 常數設定 --
    private const float POSTURE_BAR_WIDTH = 560f; // 架勢條全滿時的寬度
    private const float DAMAGED_HEALTH_FADE_TIMER_MAX = 1f; // 受傷後，延遲血條開始縮短的等待時間
    private const float DAMAGED_HEALTH_SHRINK_SPEED = 2f; // 延遲血條的縮短速度

    // -- UI 元件引用 --
    // 建議在 Unity Editor 中將對應的 UI 元件拖曳到此處
    [Header("UI 元件")]
    [SerializeField] private Image healthBarImage; // 主要血條 Image
    [SerializeField] private Image healthBarDamagedImage; // 用於顯示延遲效果的受損血條 Image
    [SerializeField] private RectTransform postureBarRectTransform; // 架勢條的 RectTransform，用於改變寬度
    [SerializeField] private Image postureBarImage; // 架勢條 Image，用於改變顏色
    [SerializeField] private GameObject postureBarHighlightGameObject; // 架勢條滿了之後的高亮特效物件

    // -- 私有變數 --
    private HealthPostureSystem healthPostureSystem; // 資料來源
    private float healthBarDamagedFadeTimer; // 受損血條延遲縮短的計時器

    private void Update() {
        // 更新延遲血條的縮短邏輯
        healthBarDamagedFadeTimer -= Time.deltaTime;
        if (healthBarDamagedFadeTimer < 0) {
            if (healthBarImage.fillAmount < healthBarDamagedImage.fillAmount) {
                // 如果延遲血條比實際血條長，則使其緩慢縮短
                healthBarDamagedImage.fillAmount -= DAMAGED_HEALTH_SHRINK_SPEED * Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// 設定此 UI 對應的 HealthPostureSystem 資料來源。
    /// </summary>
    /// <param name="healthPostureSystem">要顯示資料的系統</param>
    public void SetHealthPostureSystem(HealthPostureSystem healthPostureSystem) {
        // 儲存資料系統的引用
        this.healthPostureSystem = healthPostureSystem;

        // 初始化 UI 顯示
        UpdateHealthBar();
        UpdatePostureBar();

        // 訂閱相關事件，當資料變動時自動更新 UI
        healthPostureSystem.OnHealthChanged += HealthPostureSystem_OnHealthChanged;
        healthPostureSystem.OnPostureChanged += HealthPostureSystem_OnPostureChanged;
        healthPostureSystem.OnDead += HealthPostureSystem_OnDead;
        healthPostureSystem.OnPostureBroken += HealthPostureSystem_OnPostureBroken;
    }

    // 當架勢條被擊破時的處理
    private void HealthPostureSystem_OnPostureBroken(object sender, System.EventArgs e) {
        // 在滑鼠位置顯示文字提示 (此為範例，可替換為更複雜的 UI 動畫)
        // CMDebug.TextPopupMouse("Posture Broken!"); 
        // 顯示架勢條高光
        postureBarHighlightGameObject.SetActive(true);
    }

    // 當角色死亡時的處理
    private void HealthPostureSystem_OnDead(object sender, System.EventArgs e) {
        // 在滑鼠位置顯示文字提示 (此為範例)
        // CMDebug.TextPopupMouse("Dead!");
    }

    // 當架勢值改變時，由事件觸發此方法
    private void HealthPostureSystem_OnPostureChanged(object sender, System.EventArgs e) {
        UpdatePostureBar();
    }

    // 當生命值改變時，由事件觸發此方法
    private void HealthPostureSystem_OnHealthChanged(object sender, System.EventArgs e) {
        UpdateHealthBar();
    }

    // 更新生命條的顯示
    private void UpdateHealthBar() {
        float healthNormalized = healthPostureSystem.GetHealthNormalized();
        healthBarImage.fillAmount = healthNormalized;

        // 如果是受傷（目前血量小於延遲血條），則重置計時器
        if (healthBarDamagedImage.fillAmount > healthBarImage.fillAmount) {
            healthBarDamagedFadeTimer = DAMAGED_HEALTH_FADE_TIMER_MAX;
        } else {
            // 如果是治療，則讓延遲血條直接跟上
            healthBarDamagedImage.fillAmount = healthBarImage.fillAmount;
        }
    }

    // 更新架勢條的顯示
    private void UpdatePostureBar() {
        float postureNormalized = healthPostureSystem.GetPostureNormalized();
        // 根據架勢值的比例，設定架勢條的寬度
        postureBarRectTransform.sizeDelta = new Vector2(postureNormalized * POSTURE_BAR_WIDTH, postureBarRectTransform.sizeDelta.y);
        // 根據架勢值的比例，改變顏色（從黃色到紅色）
        Color postureBarColor = new Color(1, 1 - postureNormalized, 0);
        postureBarImage.color = postureBarColor;

        // 如果架勢值沒有滿，則隱藏高光
        if (postureNormalized < 1f) {
            postureBarHighlightGameObject.SetActive(false);
        }
    }
}
