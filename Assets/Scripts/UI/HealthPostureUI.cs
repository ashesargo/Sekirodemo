using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 控制生命值與架勢條 UI 顯示
public class HealthPostureUI : MonoBehaviour
{
    private HealthPostureSystem healthPostureSystem; // 引用 HealthPostureSystem 資料

    private const float postureBarWidth = 490f; // 架勢條全滿時的寬度
    private const float healthBarDamagedFadeTimerMax = 1f; // 受傷後，延遲血條開始縮短的等待時間
    private const float healthBarDamagedShrinkSpeed = 2f; // 延遲血條的縮短速度
    private float healthBarDamagedFadeTimer; // 受損血條延遲的計時器
    private float previousPostureNormalized; // 記錄前一次的架勢值

    // 架勢條自動隱藏相關
    private const float postureBarHideDelay = 5f; // 架勢條隱藏延遲時間
    private Coroutine hidePostureBarCoroutine; // 隱藏架勢條的協程
    private bool isPostureIncreasing = false; // 架勢是否正在增加

    [Header("UI 組件")]
    [SerializeField] private Image healthBarImage; // 血條 Image
    [SerializeField] private Image healthBarDamagedImage; // 顯示延遲效果的受損血條 Image
    [SerializeField] private Image postureBarImage; // 架勢條 Image
    [SerializeField] private GameObject postureBarHighlightGameObject; // 架勢條滿了之後的高亮特效
    [SerializeField] private RectTransform postureBarRectTransform; // 架勢條的 RectTransform
    [SerializeField] private GameObject postureBarGameObject;   // 架勢條 GameObject

    private void Update()
    {
        // 架勢自動恢復
        if (healthPostureSystem != null)
        {
            healthPostureSystem.HandlePostureRecovery();
        }

        // 更新延遲血條的縮短邏輯
        healthBarDamagedFadeTimer -= Time.deltaTime;
        if (healthBarDamagedFadeTimer < 0)
        {
            if (healthBarImage.fillAmount < healthBarDamagedImage.fillAmount)
            {
                // 如果延遲血條比實際血條長，則使其緩慢縮短
                healthBarDamagedImage.fillAmount -= healthBarDamagedShrinkSpeed * Time.deltaTime;
            }
        }
    }

    // 設定 HealthPostureSystem 資料來源
    public void SetHealthPostureSystem(HealthPostureSystem healthPostureSystem)
    {
        this.healthPostureSystem = healthPostureSystem;

        // 初始化時隱藏架勢條
        if (postureBarGameObject != null)
        {
            postureBarGameObject.SetActive(false);
        }

        // 初始化 UI
        UpdateHealthBar();
        UpdatePostureBar();

        // 定義事件，當觸發時更新 UI
        if (healthPostureSystem != null)
        {
            healthPostureSystem.OnHealthChanged += HealthPostureSystem_OnHealthChanged;
            healthPostureSystem.OnPostureChanged += HealthPostureSystem_OnPostureChanged;
            healthPostureSystem.OnDead += HealthPostureSystem_OnDead;
            healthPostureSystem.OnPostureBroken += HealthPostureSystem_OnPostureBroken;
        }
    }

    // 當架勢條被擊破時的處理
    private void HealthPostureSystem_OnPostureBroken(object sender, System.EventArgs e)
    {
        // 架勢被打破時顯示架勢條
        ShowPostureBar();
    }

    // 當角色死亡時
    private void HealthPostureSystem_OnDead(object sender, System.EventArgs e)
    {
    }

    // 當架勢值改變時
    private void HealthPostureSystem_OnPostureChanged(object sender, System.EventArgs e)
    {
        UpdatePostureBar();
    }

    // 當生命值改變時
    private void HealthPostureSystem_OnHealthChanged(object sender, System.EventArgs e)
    {
        UpdateHealthBar();
    }

    // 更新生命條
    private void UpdateHealthBar()
    {
        float healthNormalized = healthPostureSystem.GetHealthNormalized();
        
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = healthNormalized;
        }

        // 如果是受傷（目前血量小於延遲血條），重置計時器
        if (healthBarDamagedImage != null && healthBarDamagedImage.fillAmount > healthBarImage.fillAmount)
        {
            healthBarDamagedFadeTimer = healthBarDamagedFadeTimerMax;
        }
        else if (healthBarDamagedImage != null)
        {
            // 如果是回血，延遲血條直接跟上
            healthBarDamagedImage.fillAmount = healthBarImage.fillAmount;
        }
    }

    // 更新架勢條
    private void UpdatePostureBar()
    {
        float postureNormalized = healthPostureSystem.GetPostureNormalized();

        // 檢查架勢條是否在增加
        bool isPostureIncreasing = postureNormalized > previousPostureNormalized;

        // 如果架勢值增加，顯示架勢條並重置隱藏計時器
        if (isPostureIncreasing && postureNormalized > 0f)
        {
            ShowPostureBar();
            StartPostureBarHideTimer();
        }

        // 根據架勢值的比例，設定架勢條的寬度
        postureBarRectTransform.sizeDelta = new Vector2(postureNormalized * postureBarWidth, postureBarRectTransform.sizeDelta.y);

        // 根據架勢條的比例，改變顏色（從黃色到紅色）
        Color postureBarColor = new Color(1, 1 - postureNormalized, 0);
        postureBarImage.color = postureBarColor;

        // 如果架勢條未滿，隱藏高光
        if (postureNormalized < 0.5f) {
            postureBarHighlightGameObject.SetActive(false);
        }

        // 當架式條大於一半，架式條增加時，架式條高光半透明顯示
        if (postureNormalized > 0.5f && postureNormalized < 1f && isPostureIncreasing)
        {
            postureBarHighlightGameObject.SetActive(true);

            // 只顯示0.2秒
            StartCoroutine(HidePostureBarHighlightAfterDelay(0.2f));
        }

        // 當架式條滿了，架式條高光不透明顯示
        if (postureNormalized == 1f)
        {
            postureBarHighlightGameObject.SetActive(true);
        }

        // 架勢條減少時隱藏高亮特效
        if (!isPostureIncreasing)
        {
            postureBarHighlightGameObject.SetActive(false);
        }

        // 更新前一次的架勢值
        previousPostureNormalized = postureNormalized;
    }

    // 顯示架勢條
    private void ShowPostureBar()
    {
        if (postureBarGameObject != null)
        {
            postureBarGameObject.SetActive(true);
        }
    }

    // 隱藏架勢條
    private void HidePostureBar()
    {
        if (postureBarGameObject != null)
        {
            postureBarGameObject.SetActive(false);
        }
    }

    // 開始架勢條隱藏計時器
    private void StartPostureBarHideTimer()
    {
        // 停止之前的隱藏協程
        if (hidePostureBarCoroutine != null)
        {
            StopCoroutine(hidePostureBarCoroutine);
        }

        // 開始新的隱藏協程
        hidePostureBarCoroutine = StartCoroutine(HidePostureBarAfterDelay(postureBarHideDelay));
    }

    // 延遲隱藏架勢條的協程
    private IEnumerator HidePostureBarAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePostureBar();
        hidePostureBarCoroutine = null;
    }

    // 延遲隱藏架勢條高光
    private IEnumerator HidePostureBarHighlightAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        postureBarHighlightGameObject.SetActive(false);
    }
}

