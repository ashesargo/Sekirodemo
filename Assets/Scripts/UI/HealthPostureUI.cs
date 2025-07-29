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
    
    // 生命球UI
    [Header("生命球UI")]
    [SerializeField] private Transform lifeBallContainer; // 生命球的父物件
    [SerializeField] private GameObject lifeBallPrefab;   // 生命球Prefab（Image元件）
    [SerializeField] private GameObject lifeBallCrossPrefab;  // 打叉生命球Prefab
    [SerializeField] private float lifeBallSpacing = 50f; // 生命球之間的間距
    private List<GameObject> lifeBallObjects = new List<GameObject>();

    private void Update()
    {
        // 檢查是否為玩家（玩家UI在GUI父物件底下，需要特殊判斷）
        bool isPlayer = IsPlayerUI();
        
        // 架勢自動恢復（所有角色都正常自動恢復）
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

        // 檢查是否為玩家（玩家UI在GUI父物件底下，需要特殊判斷）
        bool isPlayer = IsPlayerUI();
        
        // 初始化時隱藏架勢條（除非是玩家且架勢值大於50%）
        if (postureBarGameObject != null)
        {
            if (isPlayer && healthPostureSystem != null)
            {
                float postureNormalized = healthPostureSystem.GetPostureNormalized();
                if (postureNormalized > 0.5f)
                {
                    postureBarGameObject.SetActive(true);
                }
                else
                {
                    postureBarGameObject.SetActive(false);
                }
            }
            else
            {
                postureBarGameObject.SetActive(false);
            }
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

        // 檢查是否為玩家（玩家UI在GUI父物件底下，需要特殊判斷）
        bool isPlayer = IsPlayerUI();

        // 玩家架勢值大於50%時常駐顯示
        if (isPlayer && postureNormalized > 0.5f)
        {
            ShowPostureBar();
            // 停止隱藏協程
            if (hidePostureBarCoroutine != null)
            {
                StopCoroutine(hidePostureBarCoroutine);
                hidePostureBarCoroutine = null;
            }
        }
        // 如果架勢值增加，顯示架勢條並重置隱藏計時器
        else if (isPostureIncreasing && postureNormalized > 0f)
        {
            ShowPostureBar();
            StartPostureBarHideTimer();
        }
        // 如果架勢值減少且玩家架勢值大於50%，確保架勢條仍然顯示
        else if (isPlayer && postureNormalized > 0.5f && !isPostureIncreasing)
        {
            ShowPostureBar();
            // 停止隱藏協程
            if (hidePostureBarCoroutine != null)
            {
                StopCoroutine(hidePostureBarCoroutine);
                hidePostureBarCoroutine = null;
            }
        }
        // 玩家架勢值小於等於50%且架勢值減少時，讓隱藏計時器處理
        else if (isPlayer && postureNormalized <= 0.5f && !isPostureIncreasing)
        {
            // 不顯示架勢條，讓隱藏計時器處理
            // 如果沒有隱藏計時器在運行，則隱藏架勢條
            if (hidePostureBarCoroutine == null)
            {
                HidePostureBar();
            }
        }
        // 其他情況（非玩家或架勢值為0）
        else
        {
            // 不顯示架勢條，讓隱藏計時器處理
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

        // 架勢條減少時隱藏高亮特效（但玩家架勢值大於50%時不隱藏）
        if (!isPostureIncreasing && (!isPlayer || postureNormalized <= 0.5f))
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
        // 檢查是否為玩家（玩家UI在GUI父物件底下，需要特殊判斷）
        bool isPlayer = IsPlayerUI();
        float postureNormalized = healthPostureSystem.GetPostureNormalized();
        if (isPlayer && postureNormalized > 0.5f)
        {
            return;
        }
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

    // 更新生命球顯示
    public void UpdateLifeBalls(int healthAmount, int healthAmountMax)
    {
        // 先清空舊的
        foreach (var obj in lifeBallObjects)
            Destroy(obj);
        lifeBallObjects.Clear();

        // 如果最大生命值為0，不顯示任何球
        if (healthAmountMax <= 0)
            return;

        // 計算總寬度，讓生命球居中
        float totalWidth = (healthAmountMax - 1) * lifeBallSpacing;
        float startX = -totalWidth / 2f;

        // 根據最大生命值生成對應數量的球
        for (int i = 0; i < healthAmountMax; i++)
        {
            GameObject go;
            
            if (i < healthAmount)
            {
                // 正常生命球使用原本的 Prefab
                go = Instantiate(lifeBallPrefab, lifeBallContainer);
            }
            else
            {
                // 減命時使用打叉 Prefab 替換
                go = Instantiate(lifeBallCrossPrefab, lifeBallContainer);
            }

            // 設定生命球位置，讓它們並排顯示
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(startX + i * lifeBallSpacing, 0f);
            }

            lifeBallObjects.Add(go);
        }
    }

    // 判斷是否為玩家UI
    private bool IsPlayerUI()
    {
        // 方法1：檢查是否有PlayerStatus組件（如果UI在玩家底下）
        bool hasPlayerStatus = GetComponentInParent<HealthPostureController>()?.GetComponent<PlayerStatus>() != null;
        if (hasPlayerStatus) return true;
        
        // 方法2：檢查物件名稱或標籤（如果UI在GUI父物件底下）
        // 你可以根據你的UI命名規則來判斷
        if (gameObject.name.Contains("Player") || gameObject.name.Contains("player"))
            return true;
        
        // 方法3：檢查父物件名稱
        Transform parent = transform.parent;
        while (parent != null)
        {
            if (parent.name.Contains("Player") || parent.name.Contains("player"))
                return true;
            parent = parent.parent;
        }
        
        // 方法4：檢查是否有特定的組件或標記
        // 你可以在玩家UI上掛一個特殊的組件來標記
        if (GetComponent<PlayerUIMarker>() != null)
            return true;
        
        return false;
    }
}

