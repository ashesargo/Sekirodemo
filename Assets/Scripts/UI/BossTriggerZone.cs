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
        if (player == null) 
        {
            Debug.LogWarning($"[BossTriggerZone] 找不到標籤為 'Player' 的物件");
            return;
        }
        
        float distance = Vector3.Distance(transform.position, player.transform.position);
        
        if (distance <= triggerRadius && !isPlayerInZone)
        {
            // 玩家進入Boss區域
            Debug.Log($"[BossTriggerZone] 玩家進入 Boss 區域！距離: {distance:F2}");
            OnPlayerEnterBossZone();
        }
        else if (distance > triggerRadius && isPlayerInZone)
        {
            // 玩家離開Boss區域
            Debug.Log($"[BossTriggerZone] 玩家離開 Boss 區域！距離: {distance:F2}");
            OnPlayerExitBossZone();
        }
    }
    
    // 確保 GameObject 及其所有父物件都是啟用的
    private void EnsureGameObjectHierarchyActive(GameObject targetObject)
    {
        if (targetObject == null) 
        {
            Debug.LogWarning($"[BossTriggerZone] EnsureGameObjectHierarchyActive: targetObject 為 null");
            return;
        }
        
        Debug.Log($"[BossTriggerZone] 開始確保 {targetObject.name} 的層級啟用狀態");
        
        // 從目標物件開始，向上遍歷所有父物件，確保它們都是啟用的
        Transform current = targetObject.transform;
        int hierarchyLevel = 0;
        while (current != null)
        {
            Debug.Log($"[BossTriggerZone] 檢查層級 {hierarchyLevel}: {current.gameObject.name}, activeSelf: {current.gameObject.activeSelf}, activeInHierarchy: {current.gameObject.activeInHierarchy}");
            
            if (!current.gameObject.activeSelf)
            {
                Debug.Log($"[BossTriggerZone] 啟用父物件: {current.gameObject.name}");
                current.gameObject.SetActive(true);
            }
            current = current.parent;
            hierarchyLevel++;
        }
        
        Debug.Log($"[BossTriggerZone] 確保 {targetObject.name} 的整個層級都是啟用的，檢查了 {hierarchyLevel} 個層級");
    }
    
    private HealthPostureController FindActualBossInstance()
    {
        if (bossObject == null)
        {
            Debug.LogWarning($"[BossTriggerZone] FindActualBossInstance: bossObject 為 null，無法找到 Boss 實例");
            return null;
        }
        
        // 根據 bossObject 的名稱找到實際生成的實例
        string bossName = bossObject.name;
        Debug.Log($"[BossTriggerZone] 尋找 Boss 實例，名稱: {bossName}");
        
        // 方法1: 嘗試找到具有相同名稱的啟用物件
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        Debug.Log($"[BossTriggerZone] 場景中共有 {allObjects.Length} 個物件");
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == bossName && obj.activeInHierarchy)
            {
                HealthPostureController controller = obj.GetComponent<HealthPostureController>();
                if (controller != null)
                {
                    Debug.Log($"[BossTriggerZone] 找到 Boss 實例 (完全匹配): {obj.name}");
                    return controller;
                }
            }
        }
        
        // 方法2: 如果找不到完全相同的名稱，嘗試找到精確匹配的物件（去掉Clone後綴）
        foreach (GameObject obj in allObjects)
        {
            string objNameWithoutClone = obj.name.Replace("(Clone)", "");
            string bossNameWithoutClone = bossName.Replace("(Clone)", "");
            
            if (objNameWithoutClone == bossNameWithoutClone && obj.activeInHierarchy)
            {
                HealthPostureController controller = obj.GetComponent<HealthPostureController>();
                if (controller != null && controller.IsBoss())
                {
                    Debug.Log($"[BossTriggerZone] 找到精確匹配的 Boss 實例: {obj.name} (去掉Clone後綴後匹配)");
                    return controller;
                }
            }
        }
        
        // 方法3: 找到所有 Boss 類型的 HealthPostureController，但優先匹配正確的類型
        HealthPostureController[] allControllers = FindObjectsOfType<HealthPostureController>();
        Debug.Log($"[BossTriggerZone] 找到 {allControllers.Length} 個 HealthPostureController");
        
        // 首先嘗試找到與 bossObject 相同類型的 Boss
        foreach (HealthPostureController controller in allControllers)
        {
            string controllerNameWithoutClone = controller.gameObject.name.Replace("(Clone)", "");
            string bossNameWithoutClone = bossName.Replace("(Clone)", "");
            
            Debug.Log($"[BossTriggerZone] 檢查控制器: {controller.gameObject.name}, IsBoss: {controller.IsBoss()}, activeInHierarchy: {controller.gameObject.activeInHierarchy}");
            
            if (controller.IsBoss() && controller.gameObject.activeInHierarchy && controllerNameWithoutClone == bossNameWithoutClone)
            {
                Debug.Log($"[BossTriggerZone] 找到正確類型的 Boss 實例: {controller.gameObject.name}");
                return controller;
            }
        }
        
        // 如果找不到正確類型的 Boss，才考慮其他 Boss
        Debug.LogWarning($"[BossTriggerZone] 找不到與 {bossName} 相同類型的 Boss，這可能表示配置錯誤");
        foreach (HealthPostureController controller in allControllers)
        {
            if (controller.IsBoss() && controller.gameObject.activeInHierarchy)
            {
                Debug.Log($"[BossTriggerZone] 找到其他類型的 Boss 實例 (備用): {controller.gameObject.name}");
                return controller;
            }
        }
        
        Debug.LogWarning($"[BossTriggerZone] 無法找到 Boss 實例，名稱: {bossName}");
        return null;
    }

    private void OnPlayerEnterBossZone()
    {
        if (isPlayerInZone) 
        {
            Debug.Log($"[BossTriggerZone] OnPlayerEnterBossZone: 玩家已經在區域內，忽略重複調用");
            return;
        }
        
        isPlayerInZone = true;
        Debug.Log($"[BossTriggerZone] ===== 玩家進入 Boss 區域開始 ===== {gameObject.name}");
        
        // 動態找到實際生成的 Boss 實例
        HealthPostureController actualBossController = FindActualBossInstance();
        
        if (actualBossController != null)
        {
            Debug.Log($"[BossTriggerZone] 找到實際 Boss 實例: {actualBossController.gameObject.name}");
            Debug.Log($"[BossTriggerZone] Boss 血量: {actualBossController.GetHealthPercentage() * 100:F1}%");
            Debug.Log($"[BossTriggerZone] Boss 架勢: {actualBossController.GetPosturePercentage() * 100:F1}%");
            
            // 確保 Boss 的整個層級都是啟用的
            EnsureGameObjectHierarchyActive(actualBossController.gameObject);
            
            // 檢查並修復物件池生成的Boss的血條UI引用
            if (actualBossController.healthPostureUI == null)
            {
                Debug.LogWarning($"[BossTriggerZone] Boss {actualBossController.gameObject.name} 的 healthPostureUI 為 null，嘗試修復");
                
                // 嘗試在Boss物件下找到血條UI
                HealthPostureUI[] healthUIs = actualBossController.gameObject.GetComponentsInChildren<HealthPostureUI>(true);
                Debug.Log($"[BossTriggerZone] 在 Boss 下找到 {healthUIs.Length} 個 HealthPostureUI 組件");
                
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
            else
            {
                Debug.Log($"[BossTriggerZone] Boss 血條UI已存在: {actualBossController.healthPostureUI.gameObject.name}");
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
                
                // 檢查血條UI的詳細狀態
                Debug.Log($"[BossTriggerZone] 血條UI GameObject 狀態: activeInHierarchy={actualBossController.healthPostureUI.gameObject.activeInHierarchy}");
                
                // 檢查血條UI的子物件狀態
                Transform[] childTransforms = actualBossController.healthPostureUI.gameObject.GetComponentsInChildren<Transform>(true);
                Debug.Log($"[BossTriggerZone] 血條UI共有 {childTransforms.Length} 個子物件");
                foreach (Transform child in childTransforms)
                {
                    if (child.name.Contains("Health") || child.name.Contains("Posture") || child.name.Contains("Bar"))
                    {
                        Debug.Log($"[BossTriggerZone] 血條相關子物件: {child.name}, activeInHierarchy: {child.gameObject.activeInHierarchy}");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"[BossTriggerZone] 無法找到實際的 Boss 實例");
        }
        
        // 只為當前 BossTriggerZone 對應的 Boss 顯示血條
        // 移除為其他 Boss 顯示血條的邏輯，避免多個 Boss UI 同時顯示
        Debug.Log($"[BossTriggerZone] 只為當前 BossTriggerZone 顯示血條，不處理其他 Boss");
        
        // 開始淡入 Boss 音樂
        if (bossMusic != null)
        {
            Debug.Log($"[BossTriggerZone] 開始淡入 Boss 音樂: {bossMusic.name}");
            StartCoroutine(FadeToBossMusic());
        }
        else
        {
            Debug.LogWarning($"[BossTriggerZone] bossMusic 為 null，跳過音樂切換");
        }
        
        Debug.Log($"[BossTriggerZone] ===== 玩家進入 Boss 區域完成 ===== {gameObject.name}");
    }
    
    void OnPlayerExitBossZone()
    {
        Debug.Log($"[BossTriggerZone] ===== 玩家離開 Boss 區域開始 ===== {gameObject.name}");
        
        isPlayerInZone = false;
        
        Debug.Log($"[BossTriggerZone] 玩家離開 Boss 區域");
        
        // 只檢查當前 BossTriggerZone 對應的 Boss 狀態
        HealthPostureController currentBossController = FindActualBossInstance();
        if (currentBossController != null)
        {
            bool isDead = IsBossDead();
            Debug.Log($"[BossTriggerZone] 當前 Boss {currentBossController.gameObject.name} 死亡狀態: {isDead}");
            
            if (isDead)
            {
                // 當前 Boss 已死亡，隱藏其血條
                Debug.Log($"[BossTriggerZone] 當前 Boss 已死亡，隱藏血條: {currentBossController.gameObject.name}");
                currentBossController.HideHealthBar();
            }
            else
            {
                // 當前 Boss 還活著，保持血條顯示
                Debug.Log($"[BossTriggerZone] 當前 Boss 還活著，保持血條顯示: {currentBossController.gameObject.name}");
                return;
            }
        }
        else
        {
            Debug.LogWarning($"[BossTriggerZone] 無法找到當前 Boss 實例");
        }
        
        // 淡出回到原始音樂
        if (backgroundMusicSource != null && originalMusic != null)
        {
            Debug.Log($"[BossTriggerZone] 開始淡出回到原始音樂");
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
                Debug.Log($"[BossTriggerZone] 停止之前的音樂淡入淡出協程");
            }
            if (gameObject.activeInHierarchy)
            {
                musicFadeCoroutine = StartCoroutine(FadeToOriginalMusic());
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
        
        Debug.Log("所有Boss都已死亡，隱藏Boss血條並淡出音樂");
        Debug.Log($"[BossTriggerZone] ===== 玩家離開 Boss 區域完成 ===== {gameObject.name}");
    }
    
    // 淡入Boss音樂
    IEnumerator FadeToBossMusic()
    {
        Debug.Log($"[BossTriggerZone] 開始淡入 Boss 音樂");
        Debug.Log($"[BossTriggerZone] 當前音量: {backgroundMusicSource.volume}");
        Debug.Log($"[BossTriggerZone] 當前音樂: {(backgroundMusicSource.clip != null ? backgroundMusicSource.clip.name : "null")}");
        Debug.Log($"[BossTriggerZone] 目標音樂: {bossMusic.name}");
        
        float startVolume = backgroundMusicSource.volume;
        float targetVolume = bossMusicVolume;
        
        // 淡出當前音樂
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
        
        // 切換到Boss音樂
        backgroundMusicSource.clip = bossMusic;
        backgroundMusicSource.Play();
        
        Debug.Log($"[BossTriggerZone] 已切換到 Boss 音樂: {bossMusic.name}");
        
        // 淡入Boss音樂
        elapsedTime = 0f;
        Debug.Log($"[BossTriggerZone] 開始淡入階段，持續時間: {fadeDuration / 2f} 秒");
        while (elapsedTime < fadeDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (fadeDuration / 2f);
            backgroundMusicSource.volume = Mathf.Lerp(0f, targetVolume, progress);
            yield return null;
        }
        
        backgroundMusicSource.volume = targetVolume;
        Debug.Log($"[BossTriggerZone] Boss 音樂淡入完成，最終音量: {backgroundMusicSource.volume}");
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
            Debug.LogWarning($"[BossTriggerZone] IsBossDead: bossHealthController 為 null");
            return true;
        }
        
        // 檢查Boss的血量是否為0
        float healthPercentage = bossHealthController.GetHealthPercentage();
        bool isDead = healthPercentage <= 0f;
        Debug.Log($"[BossTriggerZone] Boss {bossHealthController.gameObject.name} 血量檢查: {healthPercentage * 100:F1}%, 是否死亡: {isDead}");
        return isDead;
    }

    // Boss死亡時調用
    public void OnBossDeath()
    {
        Debug.Log($"[BossTriggerZone] ===== OnBossDeath 被調用開始 ===== {gameObject.name}");
        Debug.Log($"[BossTriggerZone] GameObject 狀態: activeInHierarchy={gameObject.activeInHierarchy}, activeSelf={gameObject.activeSelf}");
        Debug.Log($"[BossTriggerZone] backgroundMusicSource: {(backgroundMusicSource != null ? backgroundMusicSource.name : "null")}");
        Debug.Log($"[BossTriggerZone] originalMusic: {(originalMusic != null ? originalMusic.name : "null")}");
        Debug.Log($"[BossTriggerZone] 當前音樂: {(backgroundMusicSource != null && backgroundMusicSource.clip != null ? backgroundMusicSource.clip.name : "null")}");
        Debug.Log($"[BossTriggerZone] 當前音量: {(backgroundMusicSource != null ? backgroundMusicSource.volume.ToString() : "N/A")}");
        
        // 隱藏Boss血條
        if (bossHealthController != null)
        {
            Debug.Log($"[BossTriggerZone] 隱藏 Boss 血條: {bossHealthController.gameObject.name}");
            bossHealthController.HideHealthBar();
            Debug.Log($"[BossTriggerZone] 已隱藏 Boss 血條");
        }
        else
        {
            Debug.LogWarning($"[BossTriggerZone] OnBossDeath: bossHealthController 為 null，無法隱藏血條");
        }
        
        // 淡出回到原始音樂
        if (backgroundMusicSource != null && originalMusic != null)
        {
            Debug.Log($"[BossTriggerZone] 開始處理音樂恢復");
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
        Debug.Log($"[BossTriggerZone] ===== OnBossDeath 被調用完成 ===== {gameObject.name}");
    }
    
    // 檢查玩家是否在Boss區域內
    public bool IsPlayerInZone()
    {
        Debug.Log($"[BossTriggerZone] IsPlayerInZone 查詢: {isPlayerInZone}");
        return isPlayerInZone;
    }
    
    // 在Scene視圖中顯示觸發範圍
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
} 