using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

/// <summary>
/// 血色暗角控制器
/// 當角色血量低於50%時啟用vignette效果
/// 支援Built-in Render Pipeline的Post Processing Stack v2
/// </summary>
public class BloodVignetteController : MonoBehaviour
{
    [Header("血量設定")]
    [SerializeField] private float healthThreshold = 0.5f; // 血量閾值 (50%)
    [SerializeField] private PlayerStatus playerStatus; // 玩家狀態引用
    
    [Header("Vignette 設定")]
    [SerializeField] private PostProcessVolume postProcessVolume; // 後處理體積
    [SerializeField] private float vignetteIntensity = 0.5f; // vignette強度
    [SerializeField] private Color vignetteColor = Color.red; // vignette顏色
    [SerializeField] private float smoothness = 0.2f; // 平滑度
    
    [Header("動畫設定")]
    [SerializeField] private float fadeInDuration = 0.5f; // 淡入時間
    [SerializeField] private float fadeOutDuration = 1.0f; // 淡出時間
    
#if UNITY_POST_PROCESSING_STACK_V2
    private Vignette vignette; // vignette組件
#endif
    private bool isVignetteActive = false; // vignette是否啟用
    private float currentIntensity = 0f; // 當前強度
    private float targetIntensity = 0f; // 目標強度
    
    private void Start()
    {
        // 如果沒有指定玩家狀態，自動尋找
        if (playerStatus == null)
        {
            playerStatus = FindObjectOfType<PlayerStatus>();
        }
        
        // 如果沒有指定後處理體積，自動尋找
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<PostProcessVolume>();
        }
        
#if UNITY_POST_PROCESSING_STACK_V2
        // 獲取vignette組件
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGetSettings(out vignette);
            
            // 如果沒有vignette組件，創建一個
            if (vignette == null)
            {
                CreateVignetteSettings();
            }
        }
        
        // 初始化vignette設定
        if (vignette != null)
        {
            InitializeVignette();
        }
#endif
    }
    
    private void Update()
    {
        if (playerStatus == null) return;
        
        // 獲取當前血量百分比
        float healthPercentage = playerStatus.GetHealthPercentage();
        
        // 檢查是否需要啟用vignette
        bool shouldActivate = healthPercentage <= healthThreshold;
        
        // 更新vignette狀態
        UpdateVignetteState(shouldActivate);
        
        // 更新vignette強度
        UpdateVignetteIntensity();
    }
    
#if UNITY_POST_PROCESSING_STACK_V2
    /// <summary>
    /// 創建vignette設定
    /// </summary>
    private void CreateVignetteSettings()
    {
        if (postProcessVolume.profile == null) return;
        
        // 創建新的vignette設定
        vignette = postProcessVolume.profile.AddSettings<Vignette>();
        vignette.enabled.Override(true);
        vignette.active = true;
    }
    
    /// <summary>
    /// 初始化vignette設定
    /// </summary>
    private void InitializeVignette()
    {
        vignette.enabled.Override(true);
        vignette.active = true;
        vignette.mode.Override(VignetteMode.Classic);
        vignette.color.Override(vignetteColor);
        vignette.center.Override(new Vector2(0.5f, 0.5f));
        vignette.intensity.Override(0f);
        vignette.smoothness.Override(smoothness);
        vignette.roundness.Override(1f);
        vignette.rounded.Override(false);
    }
#endif
    
    /// <summary>
    /// 更新vignette狀態
    /// </summary>
    private void UpdateVignetteState(bool shouldActivate)
    {
        if (isVignetteActive != shouldActivate)
        {
            isVignetteActive = shouldActivate;
            targetIntensity = shouldActivate ? vignetteIntensity : 0f;
        }
    }
    
    /// <summary>
    /// 更新vignette強度
    /// </summary>
    private void UpdateVignetteIntensity()
    {
        if (Mathf.Abs(currentIntensity - targetIntensity) > 0.001f)
        {
            float duration = isVignetteActive ? fadeInDuration : fadeOutDuration;
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime / duration);
            
#if UNITY_POST_PROCESSING_STACK_V2
            if (vignette != null)
            {
                vignette.intensity.Override(currentIntensity);
            }
#endif
        }
    }
    
    /// <summary>
    /// 設定血量閾值
    /// </summary>
    public void SetHealthThreshold(float threshold)
    {
        healthThreshold = Mathf.Clamp01(threshold);
    }
    
    /// <summary>
    /// 設定vignette強度
    /// </summary>
    public void SetVignetteIntensity(float intensity)
    {
        vignetteIntensity = Mathf.Clamp01(intensity);
        if (isVignetteActive)
        {
            targetIntensity = vignetteIntensity;
        }
    }
    
    /// <summary>
    /// 設定vignette顏色
    /// </summary>
    public void SetVignetteColor(Color color)
    {
        vignetteColor = color;
#if UNITY_POST_PROCESSING_STACK_V2
        if (vignette != null)
        {
            vignette.color.Override(vignetteColor);
        }
#endif
    }
    
    /// <summary>
    /// 手動啟用vignette效果
    /// </summary>
    public void EnableVignette()
    {
        isVignetteActive = true;
        targetIntensity = vignetteIntensity;
    }
    
    /// <summary>
    /// 手動停用vignette效果
    /// </summary>
    public void DisableVignette()
    {
        isVignetteActive = false;
        targetIntensity = 0f;
    }
    
    /// <summary>
    /// 立即設定vignette強度（無動畫）
    /// </summary>
    public void SetVignetteIntensityImmediate(float intensity)
    {
        currentIntensity = Mathf.Clamp01(intensity);
        targetIntensity = currentIntensity;
#if UNITY_POST_PROCESSING_STACK_V2
        if (vignette != null)
        {
            vignette.intensity.Override(currentIntensity);
        }
#endif
    }
    
    /// <summary>
    /// 獲取當前vignette強度
    /// </summary>
    public float GetCurrentIntensity()
    {
        return currentIntensity;
    }
    
    /// <summary>
    /// 獲取vignette是否啟用
    /// </summary>
    public bool IsVignetteActive()
    {
        return isVignetteActive;
    }
} 