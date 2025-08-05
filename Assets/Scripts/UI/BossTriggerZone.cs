using UnityEngine;
using System.Collections;

public class BossTriggerZone : MonoBehaviour
{
    [Header("Boss設定")]
    [SerializeField] public GameObject bossObject;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioSource backgroundMusicSource;
    
    [Header("觸發設定")]
    [SerializeField] private float triggerRadius = 10f;
    [SerializeField] private float checkInterval = 0.2f; // 增加間隔以節省性能
    
    [Header("音樂設定")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float bossMusicVolume = 1f;
    
    // 快取變數 - 避免重複分配
    private bool isPlayerInZone = false;
    private HealthPostureController bossHealthController;
    private AudioClip originalMusic;
    private Coroutine musicFadeCoroutine;
    private GameObject player;
    private float nextCheckTime;
    private float sqrTriggerRadius; // 使用平方距離避免開方運算
    private Vector3 lastPlayerPosition; // 快取玩家位置
    private bool isInitialized = false;
    
    // 靜態快取 - 避免重複查找
    private static GameObject cachedPlayer;
    private static float lastPlayerCacheTime;
    private const float PLAYER_CACHE_DURATION = 1f;
    
    // 常量
    private const string PLAYER_TAG = "Player";
    private const string CLONE_SUFFIX = "(Clone)";
    
    #region Unity Lifecycle
    
    void Start()
    {
        InitializeComponents();
        sqrTriggerRadius = triggerRadius * triggerRadius; // 預計算平方距離
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // 使用時間間隔檢查
        if (Time.time >= nextCheckTime)
        {
            CheckPlayerInZoneOptimized();
            nextCheckTime = Time.time + checkInterval;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeComponents()
    {
        InitializeBossController();
        InitializeAudioSource();
        SaveOriginalMusic();
        CachePlayerReference();
        isInitialized = true;
    }
    
    private void InitializeBossController()
    {
        if (bossObject == null) return;
        
        bossHealthController = bossObject.GetComponent<HealthPostureController>();
        if (bossHealthController == null) return;
        
        FixHealthPostureUI();
    }
    
    private void FixHealthPostureUI()
    {
        if (bossHealthController?.healthPostureUI != null) return;
        
        var healthUIs = bossObject.GetComponentsInChildren<HealthPostureUI>(true);
        if (healthUIs.Length > 0)
        {
            bossHealthController.healthPostureUI = healthUIs[0];
        }
    }
    
    private void InitializeAudioSource()
    {
        if (backgroundMusicSource == null)
        {
            backgroundMusicSource = FindObjectOfType<AudioSource>();
        }
    }
    
    private void SaveOriginalMusic()
    {
        if (backgroundMusicSource != null)
        {
            originalMusic = backgroundMusicSource.clip;
        }
    }
    
    private void CachePlayerReference()
    {
        player = GetCachedPlayer();
        if (player != null)
        {
            lastPlayerPosition = player.transform.position;
        }
    }
    
    // 靜態快取玩家物件
    private static GameObject GetCachedPlayer()
    {
        if (cachedPlayer != null && Time.time - lastPlayerCacheTime < PLAYER_CACHE_DURATION)
        {
            return cachedPlayer;
        }
        
        cachedPlayer = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        lastPlayerCacheTime = Time.time;
        return cachedPlayer;
    }
    
    #endregion
    
    #region Optimized Player Detection
    
    private void CheckPlayerInZoneOptimized()
    {
        // 使用靜態快取
        if (player == null)
        {
            player = GetCachedPlayer();
            if (player == null) return;
        }
        
        // 檢查玩家是否移動 - 如果沒有移動，跳過距離計算
        Vector3 currentPlayerPosition = player.transform.position;
        if (currentPlayerPosition == lastPlayerPosition)
        {
            return; // 玩家沒有移動，跳過檢查
        }
        
        lastPlayerPosition = currentPlayerPosition;
        
        // 使用平方距離避免開方運算
        float sqrDistance = (transform.position - currentPlayerPosition).sqrMagnitude;
        
        if (sqrDistance <= sqrTriggerRadius && !isPlayerInZone)
        {
            OnPlayerEnterBossZone();
        }
        else if (sqrDistance > sqrTriggerRadius && isPlayerInZone)
        {
            OnPlayerExitBossZone();
        }
    }
    
    #endregion
    
    #region Optimized Boss Management
    
    private HealthPostureController FindActualBossInstance()
    {
        // 優先使用快取的控制器
        if (bossHealthController != null && bossHealthController.gameObject.activeInHierarchy)
        {
            return bossHealthController;
        }
        
        if (bossObject == null) return null;
        
        // 使用更高效的查找方式
        var bossName = bossObject.name.Replace(CLONE_SUFFIX, "");
        
        // 直接查找同名的 HealthPostureController
        var controllers = FindObjectsOfType<HealthPostureController>();
        int controllerCount = controllers.Length;
        
        for (int i = 0; i < controllerCount; i++)
        {
            var controller = controllers[i];
            if (!controller.IsBoss()) continue;
            
            var controllerName = controller.gameObject.name.Replace(CLONE_SUFFIX, "");
            if (controllerName == bossName)
            {
                return controller;
            }
        }
        
        // 備用查找
        for (int i = 0; i < controllerCount; i++)
        {
            var controller = controllers[i];
            if (controller.IsBoss())
            {
                return controller;
            }
        }
        
        return null;
    }
    
    private void EnsureGameObjectHierarchyActive(GameObject targetObject)
    {
        if (targetObject == null) return;
        
        Transform current = targetObject.transform;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }
            current = current.parent;
        }
    }
    
    #endregion
    
    #region Zone Events
    
    private void OnPlayerEnterBossZone()
    {
        if (isPlayerInZone) return;
        
        isPlayerInZone = true;
        HandleBossHealthBar(true);
        StartBossMusic();
    }
    
    private void OnPlayerExitBossZone()
    {
        isPlayerInZone = false;
        
        var currentBossController = FindActualBossInstance();
        if (currentBossController != null && IsBossDead())
        {
            currentBossController.HideHealthBar();
        }
        
        StartOriginalMusic();
    }
    
    private void HandleBossHealthBar(bool show)
    {
        var actualBossController = FindActualBossInstance();
        if (actualBossController == null) return;
        
        EnsureGameObjectHierarchyActive(actualBossController.gameObject);
        
        // 修復血條UI引用
        if (actualBossController.healthPostureUI == null)
        {
            var healthUIs = actualBossController.gameObject.GetComponentsInChildren<HealthPostureUI>(true);
            if (healthUIs.Length > 0)
            {
                actualBossController.healthPostureUI = healthUIs[0];
            }
        }
        
        if (show)
        {
            actualBossController.ForceShowHealthBar();
        }
        else
        {
            actualBossController.HideHealthBar();
        }
    }
    
    #endregion
    
    #region Optimized Music Management
    
    private void StartBossMusic()
    {
        if (bossMusic == null || backgroundMusicSource == null) return;
        
        StopMusicCoroutine();
        musicFadeCoroutine = StartCoroutine(FadeMusicOptimized(bossMusic, bossMusicVolume));
    }
    
    private void StartOriginalMusic()
    {
        if (originalMusic == null || backgroundMusicSource == null) return;
        
        StopMusicCoroutine();
        musicFadeCoroutine = StartCoroutine(FadeMusicOptimized(originalMusic, 1f));
    }
    
    private void StopMusicCoroutine()
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }
    }
    
    // 優化的音樂淡入淡出
    private IEnumerator FadeMusicOptimized(AudioClip targetClip, float targetVolume)
    {
        if (backgroundMusicSource == null) yield break;
        
        float startVolume = backgroundMusicSource.volume;
        float halfDuration = fadeDuration * 0.5f;
        
        // 淡出當前音樂
        yield return StartCoroutine(FadeVolumeOptimized(startVolume, 0f, halfDuration));
        
        // 切換音樂
        backgroundMusicSource.clip = targetClip;
        backgroundMusicSource.Play();
        
        // 淡入新音樂
        yield return StartCoroutine(FadeVolumeOptimized(0f, targetVolume, halfDuration));
    }
    
    // 優化的音量淡入淡出
    private IEnumerator FadeVolumeOptimized(float startVolume, float endVolume, float duration)
    {
        float elapsedTime = 0f;
        float inverseDuration = 1f / duration; // 預計算倒數
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime; // 使用 unscaledDeltaTime 避免時間縮放影響
            float progress = elapsedTime * inverseDuration;
            backgroundMusicSource.volume = Mathf.Lerp(startVolume, endVolume, progress);
            yield return null;
        }
        
        backgroundMusicSource.volume = endVolume;
    }
    
    #endregion
    
    #region Boss Status
    
    public bool IsBossDead()
    {
        var actualBossController = FindActualBossInstance();
        if (actualBossController == null)
        {
            if (bossHealthController == null) return true;
            actualBossController = bossHealthController;
        }
        
        float healthPercentage = actualBossController.GetHealthPercentage();
        return healthPercentage <= 0f && actualBossController.live <= 0;
    }
    
    public void OnBossDeath()
    {
        HandleBossHealthBar(false);
        StartOriginalMusic();
    }
    
    #endregion
    
    #region Public API
    
    public bool IsPlayerInZone()
    {
        return isPlayerInZone;
    }
    
    #endregion
} 