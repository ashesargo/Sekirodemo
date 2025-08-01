using UnityEngine;
using System.Collections;

public class BossTriggerZone : MonoBehaviour
{
    [Header("Boss設定")]
    public GameObject bossObject; // Boss物件
    public AudioClip bossMusic; // Boss音樂
    public AudioSource backgroundMusicSource; // 背景音樂AudioSource
    
    [Header("觸發設定")]
    public float triggerRadius = 10f; // 觸發範圍
    
    [Header("音樂設定")]
    public float fadeDuration = 2f; // 音樂淡入淡出時間
    public float bossMusicVolume = 1f; // Boss音樂音量
    
    private bool isPlayerInZone = false;
    private HealthPostureController bossHealthController;
    private AudioSource originalAudioSource;
    private AudioClip originalMusic; // 保存原始音樂
    private Coroutine musicFadeCoroutine; // 音樂淡入淡出協程
    
    void Start()
    {
        Debug.Log($"[BossTriggerZone] {gameObject.name} 初始化開始");
        
        // 獲取Boss的血條控制器
        if (bossObject != null)
        {
            bossHealthController = bossObject.GetComponent<HealthPostureController>();
            if (bossHealthController == null)
            {
                Debug.LogWarning($"[BossTriggerZone] 無法在 bossObject ({bossObject.name}) 上找到 HealthPostureController 組件");
            }
            else
            {
                Debug.Log($"[BossTriggerZone] 成功找到 Boss 的 HealthPostureController: {bossObject.name}");
                
                // 檢查Boss的血條UI設置
                if (bossHealthController.healthPostureUI == null)
                {
                    Debug.LogWarning($"[BossTriggerZone] Boss {bossObject.name} 的 healthPostureUI 為 null");
                    
                    // 嘗試在Boss物件下找到血條UI
                    HealthPostureUI[] healthUIs = bossObject.GetComponentsInChildren<HealthPostureUI>(true);
                    if (healthUIs.Length > 0)
                    {
                        bossHealthController.healthPostureUI = healthUIs[0];
                        Debug.Log($"[BossTriggerZone] 已修復 Boss 血條UI引用: {healthUIs[0].gameObject.name}");
                    }
                    else
                    {
                        Debug.LogError($"[BossTriggerZone] 無法在 Boss {bossObject.name} 下找到血條UI");
                    }
                }
                else
                {
                    Debug.Log($"[BossTriggerZone] Boss 血條UI已正確設置: {bossHealthController.healthPostureUI.gameObject.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[BossTriggerZone] bossObject 未設置！請在 Inspector 中設置 Boss 物件");
        }
        
        // 獲取背景音樂AudioSource
        if (backgroundMusicSource == null)
        {
            backgroundMusicSource = FindObjectOfType<AudioSource>();
            if (backgroundMusicSource == null)
            {
                Debug.LogWarning($"[BossTriggerZone] 找不到 AudioSource 組件，請確保場景中有背景音樂的 AudioSource");
            }
            else
            {
                Debug.Log($"[BossTriggerZone] 自動找到 AudioSource: {backgroundMusicSource.name}");
            }
        }
        else
        {
            Debug.Log($"[BossTriggerZone] 使用指定的 AudioSource: {backgroundMusicSource.name}");
        }
        
        // 保存原始音樂
        if (backgroundMusicSource != null)
        {
            originalAudioSource = backgroundMusicSource;
            originalMusic = backgroundMusicSource.clip;
            if (originalMusic == null)
            {
                Debug.LogWarning($"[BossTriggerZone] backgroundMusicSource 沒有設置音樂片段");
            }
            else
            {
                Debug.Log($"[BossTriggerZone] 保存原始音樂: {originalMusic.name}");
            }
        }
        
        // 檢查 Boss 音樂設置
        if (bossMusic == null)
        {
            Debug.LogWarning($"[BossTriggerZone] bossMusic 未設置！請在 Inspector 中設置 Boss 音樂");
        }
        else
        {
            Debug.Log($"[BossTriggerZone] Boss 音樂已設置: {bossMusic.name}");
        }
        
        Debug.Log($"[BossTriggerZone] {gameObject.name} 初始化完成");
    }
    
    void Update()
    {
        // 檢查玩家是否在觸發範圍內
        CheckPlayerInZone();
    }
    
    void CheckPlayerInZone()
    {
        // 找到玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.transform.position);
        
        if (distance <= triggerRadius && !isPlayerInZone)
        {
            // 玩家進入Boss區域
            OnPlayerEnterBossZone();
        }
        else if (distance > triggerRadius && isPlayerInZone)
        {
            // 玩家離開Boss區域
            OnPlayerExitBossZone();
        }
    }
    
    // 確保 GameObject 及其所有父物件都是啟用的
    private void EnsureGameObjectHierarchyActive(GameObject targetObject)
    {
        if (targetObject == null) return;
        
        // 從目標物件開始，向上遍歷所有父物件，確保它們都是啟用的
        Transform current = targetObject.transform;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                Debug.Log($"[BossTriggerZone] 啟用父物件: {current.gameObject.name}");
                current.gameObject.SetActive(true);
            }
            current = current.parent;
        }
        
        Debug.Log($"[BossTriggerZone] 確保 {targetObject.name} 的整個層級都是啟用的");
    }
    
    private HealthPostureController FindActualBossInstance()
    {
        if (bossObject == null)
        {
            Debug.LogWarning($"[BossTriggerZone] bossObject 為 null，無法找到 Boss 實例");
            return null;
        }
        
        // 根據 bossObject 的名稱找到實際生成的實例
        string bossName = bossObject.name;
        Debug.Log($"[BossTriggerZone] 尋找 Boss 實例，名稱: {bossName}");
        
        // 方法1: 嘗試找到具有相同名稱的啟用物件
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == bossName && obj.activeInHierarchy)
            {
                HealthPostureController controller = obj.GetComponent<HealthPostureController>();
                if (controller != null)
                {
                    Debug.Log($"[BossTriggerZone] 找到 Boss 實例: {obj.name}");
                    return controller;
                }
            }
        }
        
        // 方法2: 如果找不到完全相同的名稱，嘗試找到包含該名稱的物件
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(bossName) && obj.activeInHierarchy)
            {
                HealthPostureController controller = obj.GetComponent<HealthPostureController>();
                if (controller != null && controller.IsBoss())
                {
                    Debug.Log($"[BossTriggerZone] 找到包含名稱的 Boss 實例: {obj.name}");
                    return controller;
                }
            }
        }
        
        // 方法3: 找到所有 Boss 類型的 HealthPostureController
        HealthPostureController[] allControllers = FindObjectsOfType<HealthPostureController>();
        foreach (HealthPostureController controller in allControllers)
        {
            if (controller.IsBoss() && controller.gameObject.activeInHierarchy)
            {
                Debug.Log($"[BossTriggerZone] 找到 Boss 實例 (通過類型檢查): {controller.gameObject.name}");
                return controller;
            }
        }
        
        Debug.LogWarning($"[BossTriggerZone] 無法找到 Boss 實例，名稱: {bossName}");
        return null;
    }

    private void OnPlayerEnterBossZone()
    {
        if (isPlayerInZone) return;
        
        isPlayerInZone = true;
        Debug.Log($"[BossTriggerZone] 玩家進入 Boss 區域: {gameObject.name}");
        
        // 動態找到實際生成的 Boss 實例
        HealthPostureController actualBossController = FindActualBossInstance();
        
        if (actualBossController != null)
        {
            Debug.Log($"[BossTriggerZone] 找到實際 Boss 實例: {actualBossController.gameObject.name}");
            
            // 確保 Boss 的整個層級都是啟用的
            EnsureGameObjectHierarchyActive(actualBossController.gameObject);
            
            // 檢查並修復物件池生成的Boss的血條UI引用
            if (actualBossController.healthPostureUI == null)
            {
                Debug.LogWarning($"[BossTriggerZone] Boss {actualBossController.gameObject.name} 的 healthPostureUI 為 null，嘗試修復");
                
                // 嘗試在Boss物件下找到血條UI
                HealthPostureUI[] healthUIs = actualBossController.gameObject.GetComponentsInChildren<HealthPostureUI>(true);
                if (healthUIs.Length > 0)
                {
                    actualBossController.healthPostureUI = healthUIs[0];
                    Debug.Log($"[BossTriggerZone] 已修復 Boss 血條UI引用: {healthUIs[0].gameObject.name}");
                }
                else
                {
                    Debug.LogError($"[BossTriggerZone] 無法在 Boss {actualBossController.gameObject.name} 下找到血條UI");
                }
            }
            
            Debug.Log($"[BossTriggerZone] 嘗試顯示 Boss 血條: {actualBossController.gameObject.name}");
            Debug.Log($"[BossTriggerZone] Boss GameObject 狀態: activeInHierarchy={actualBossController.gameObject.activeInHierarchy}, activeSelf={actualBossController.gameObject.activeSelf}");
            
            // 直接調用 ForceShowHealthBar()，讓該方法自己處理激活邏輯
            actualBossController.ForceShowHealthBar();
                Debug.Log($"[BossTriggerZone] 已調用 ForceShowHealthBar()");
                
            if (actualBossController.healthPostureUI != null)
                {
                bool isActive = actualBossController.healthPostureUI.gameObject.activeInHierarchy;
                    Debug.Log($"[BossTriggerZone] Boss 血條狀態: {(isActive ? "啟用" : "禁用")}");
                }
            }
            else
            {
            Debug.LogWarning($"[BossTriggerZone] 無法找到實際的 Boss 實例");
        }
        
        // 找到所有 BossTriggerZone 實例
        BossTriggerZone[] allBossZones = FindObjectsOfType<BossTriggerZone>();
        Debug.Log($"[BossTriggerZone] 找到 {allBossZones.Length} 個 BossTriggerZone 實例");
        
        // 為其他 Boss 也顯示血條
        foreach (BossTriggerZone zone in allBossZones)
        {
            if (zone != this)
            {
                HealthPostureController otherBossController = zone.FindActualBossInstance();
                if (otherBossController != null)
                {
                    Debug.Log($"[BossTriggerZone] 處理其他 Boss: {otherBossController.gameObject.name}");
                
                // 確保其他 Boss 的整個層級都是啟用的
                    EnsureGameObjectHierarchyActive(otherBossController.gameObject);
                    
                    // 檢查並修復其他Boss的血條UI引用
                    if (otherBossController.healthPostureUI == null)
                    {
                        Debug.LogWarning($"[BossTriggerZone] 其他 Boss {otherBossController.gameObject.name} 的 healthPostureUI 為 null，嘗試修復");
                        
                        // 嘗試在其他Boss物件下找到血條UI
                        HealthPostureUI[] healthUIs = otherBossController.gameObject.GetComponentsInChildren<HealthPostureUI>(true);
                        if (healthUIs.Length > 0)
                        {
                            otherBossController.healthPostureUI = healthUIs[0];
                            Debug.Log($"[BossTriggerZone] 已修復其他 Boss 血條UI引用: {healthUIs[0].gameObject.name}");
                        }
                        else
                        {
                            Debug.LogError($"[BossTriggerZone] 無法在其他 Boss {otherBossController.gameObject.name} 下找到血條UI");
                        }
                    }
                    
                    Debug.Log($"[BossTriggerZone] 嘗試顯示其他 Boss 血條: {otherBossController.gameObject.name}");
                    Debug.Log($"[BossTriggerZone] 其他 Boss GameObject 狀態: activeInHierarchy={otherBossController.gameObject.activeInHierarchy}, activeSelf={otherBossController.gameObject.activeSelf}");
                
                    // 直接調用 ForceShowHealthBar()，讓該方法自己處理激活邏輯
                    otherBossController.ForceShowHealthBar();
                    Debug.Log($"[BossTriggerZone] 已調用其他 Boss 的 ForceShowHealthBar()");
                }
            }
        }
        
        // 開始淡入 Boss 音樂
        if (bossMusic != null)
        {
            StartCoroutine(FadeToBossMusic());
        }
    }
    
    void OnPlayerExitBossZone()
    {
        isPlayerInZone = false;
        
        Debug.Log($"[BossTriggerZone] 玩家離開 Boss 區域");
        
        // 檢查所有Boss是否都已死亡
        bool allBossesDead = true;
        BossTriggerZone[] allBossZones = FindObjectsOfType<BossTriggerZone>();
        
        foreach (BossTriggerZone zone in allBossZones)
        {
            if (zone.bossHealthController != null && !zone.IsBossDead())
            {
                allBossesDead = false;
                Debug.Log($"[BossTriggerZone] Boss 還活著: {zone.bossHealthController.gameObject.name}");
                break;
            }
        }
        
        if (!allBossesDead)
        {
            // 還有Boss活著，保持血條顯示
            Debug.Log("玩家離開Boss區域，但還有Boss活著，保持血條顯示");
            return;
        }
        
        // 所有Boss都已死亡，隱藏所有血條並恢復音樂
        foreach (BossTriggerZone zone in allBossZones)
        {
            if (zone.bossHealthController != null)
            {
                zone.bossHealthController.HideHealthBar();
                Debug.Log($"[BossTriggerZone] 隱藏 Boss 血條: {zone.bossHealthController.gameObject.name}");
            }
        }
        
        // 淡出回到原始音樂
        if (backgroundMusicSource != null && originalMusic != null)
        {
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            if (gameObject.activeInHierarchy)
            {
                musicFadeCoroutine = StartCoroutine(FadeToOriginalMusic());
            }
        }
        
        Debug.Log("所有Boss都已死亡，隱藏Boss血條並淡出音樂");
    }
    
    // 淡入Boss音樂
    IEnumerator FadeToBossMusic()
    {
        Debug.Log($"[BossTriggerZone] 開始淡入 Boss 音樂");
        
        float startVolume = backgroundMusicSource.volume;
        float targetVolume = bossMusicVolume;
        
        // 淡出當前音樂
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (fadeDuration / 2f);
            backgroundMusicSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }
        
        // 切換到Boss音樂
        backgroundMusicSource.clip = bossMusic;
        backgroundMusicSource.Play();
        
        Debug.Log($"[BossTriggerZone] 已切換到 Boss 音樂: {bossMusic.name}");
        
        // 淡入Boss音樂
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (fadeDuration / 2f);
            backgroundMusicSource.volume = Mathf.Lerp(0f, targetVolume, progress);
            yield return null;
        }
        
        backgroundMusicSource.volume = targetVolume;
        Debug.Log($"[BossTriggerZone] Boss 音樂淡入完成");
    }
    
    // 淡出回到原始音樂
    IEnumerator FadeToOriginalMusic()
    {
        Debug.Log($"[BossTriggerZone] 開始淡出回到原始音樂");
        Debug.Log($"[BossTriggerZone] 開始音量: {backgroundMusicSource.volume}");
        Debug.Log($"[BossTriggerZone] 當前音樂: {(backgroundMusicSource.clip != null ? backgroundMusicSource.clip.name : "null")}");
        Debug.Log($"[BossTriggerZone] 目標音樂: {(originalMusic != null ? originalMusic.name : "null")}");
        
        float startVolume = backgroundMusicSource.volume;
        
        // 淡出Boss音樂
        float elapsedTime = 0f;
        Debug.Log($"[BossTriggerZone] 開始淡出階段，持續時間: {fadeDuration / 2f} 秒");
        while (elapsedTime < fadeDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (fadeDuration / 2f);
            backgroundMusicSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }
        
        Debug.Log($"[BossTriggerZone] 淡出完成，音量: {backgroundMusicSource.volume}");
        
        // 切換回原始音樂
        backgroundMusicSource.clip = originalMusic;
        backgroundMusicSource.Play();
        
        Debug.Log($"[BossTriggerZone] 已切換回原始音樂: {originalMusic.name}");
        
        // 淡入原始音樂
        elapsedTime = 0f;
        Debug.Log($"[BossTriggerZone] 開始淡入階段，持續時間: {fadeDuration / 2f} 秒");
        while (elapsedTime < fadeDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (fadeDuration / 2f);
            backgroundMusicSource.volume = Mathf.Lerp(0f, startVolume, progress);
            yield return null;
        }
        
        backgroundMusicSource.volume = startVolume;
        Debug.Log($"[BossTriggerZone] 原始音樂淡入完成，最終音量: {backgroundMusicSource.volume}");
    }
    
    // 檢查Boss是否已死亡
    public bool IsBossDead()
    {
        if (bossHealthController == null) 
        {
            Debug.LogWarning($"[BossTriggerZone] bossHealthController 為 null");
            return true;
        }
        
        // 檢查Boss的血量是否為0
        float healthPercentage = bossHealthController.GetHealthPercentage();
        bool isDead = healthPercentage <= 0f;
        Debug.Log($"[BossTriggerZone] Boss 血量檢查: {healthPercentage * 100:F1}%, 是否死亡: {isDead}");
        return isDead;
    }

    // Boss死亡時調用
    public void OnBossDeath()
    {
        Debug.Log($"[BossTriggerZone] OnBossDeath 被調用！");
        Debug.Log($"[BossTriggerZone] GameObject 狀態: activeInHierarchy={gameObject.activeInHierarchy}, activeSelf={gameObject.activeSelf}");
        Debug.Log($"[BossTriggerZone] backgroundMusicSource: {(backgroundMusicSource != null ? backgroundMusicSource.name : "null")}");
        Debug.Log($"[BossTriggerZone] originalMusic: {(originalMusic != null ? originalMusic.name : "null")}");
        Debug.Log($"[BossTriggerZone] 當前音樂: {(backgroundMusicSource != null && backgroundMusicSource.clip != null ? backgroundMusicSource.clip.name : "null")}");
        Debug.Log($"[BossTriggerZone] 當前音量: {(backgroundMusicSource != null ? backgroundMusicSource.volume.ToString() : "N/A")}");
        
        // 隱藏Boss血條
        if (bossHealthController != null)
        {
            bossHealthController.HideHealthBar();
            Debug.Log($"[BossTriggerZone] 已隱藏 Boss 血條");
        }
        else
        {
            Debug.LogWarning($"[BossTriggerZone] bossHealthController 為 null，無法隱藏血條");
        }
        
        // 淡出回到原始音樂
        if (backgroundMusicSource != null && originalMusic != null)
        {
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
                Debug.Log($"[BossTriggerZone] 停止之前的音樂淡入淡出協程");
            }
            if (gameObject.activeInHierarchy)
            {
                musicFadeCoroutine = StartCoroutine(FadeToOriginalMusic());
                Debug.Log($"[BossTriggerZone] 開始淡出回到原始音樂");
            }
            else
            {
                Debug.LogWarning($"[BossTriggerZone] GameObject 未啟用，無法開始音樂淡入淡出");
            }
        }
        else
        {
            Debug.LogWarning($"[BossTriggerZone] 無法恢復原始音樂: backgroundMusicSource={backgroundMusicSource != null}, originalMusic={originalMusic != null}");
        }
        
        Debug.Log("Boss已死亡，隱藏血條並恢復音樂");
    }
    
    // 檢查玩家是否在Boss區域內
    public bool IsPlayerInZone()
    {
        return isPlayerInZone;
    }
    
    // 在Scene視圖中顯示觸發範圍
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
} 