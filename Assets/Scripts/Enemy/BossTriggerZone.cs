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
        // 獲取Boss的血條控制器
        if (bossObject != null)
        {
            bossHealthController = bossObject.GetComponent<HealthPostureController>();
        }
        
        // 獲取背景音樂AudioSource
        if (backgroundMusicSource == null)
        {
            backgroundMusicSource = FindObjectOfType<AudioSource>();
        }
        
        // 保存原始音樂
        if (backgroundMusicSource != null)
        {
            originalAudioSource = backgroundMusicSource;
            originalMusic = backgroundMusicSource.clip;
        }
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
    
    void OnPlayerEnterBossZone()
    {
        isPlayerInZone = true;
        
        // 強制顯示Boss血條（常駐顯示）
        if (bossHealthController != null)
        {
            bossHealthController.ForceShowHealthBar();
        }
        
        // 淡入Boss音樂
        if (backgroundMusicSource != null && bossMusic != null)
        {
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            musicFadeCoroutine = StartCoroutine(FadeToBossMusic());
        }
        
        Debug.Log("玩家進入Boss區域，顯示Boss血條並淡入Boss音樂");
    }
    
    void OnPlayerExitBossZone()
    {
        isPlayerInZone = false;
        
        // 檢查Boss是否還活著，如果活著則保持血條顯示
        if (bossHealthController != null && !IsBossDead())
        {
            // Boss還活著，保持血條顯示
            Debug.Log("玩家離開Boss區域，但Boss還活著，保持血條顯示");
            return;
        }
        
        // Boss已死亡，隱藏血條並恢復音樂
        if (bossHealthController != null)
        {
            bossHealthController.HideHealthBar();
        }
        
        // 淡出回到原始音樂
        if (backgroundMusicSource != null && originalMusic != null)
        {
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            musicFadeCoroutine = StartCoroutine(FadeToOriginalMusic());
        }
        
        Debug.Log("Boss已死亡，隱藏Boss血條並淡出音樂");
    }
    
    // 淡入Boss音樂
    IEnumerator FadeToBossMusic()
    {
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
    }
    
    // 淡出回到原始音樂
    IEnumerator FadeToOriginalMusic()
    {
        float startVolume = backgroundMusicSource.volume;
        
        // 淡出Boss音樂
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (fadeDuration / 2f);
            backgroundMusicSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }
        
        // 切換回原始音樂
        backgroundMusicSource.clip = originalMusic;
        backgroundMusicSource.Play();
        
        // 淡入原始音樂
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (fadeDuration / 2f);
            backgroundMusicSource.volume = Mathf.Lerp(0f, startVolume, progress);
            yield return null;
        }
        
        backgroundMusicSource.volume = startVolume;
    }
    
    // 檢查Boss是否已死亡
    private bool IsBossDead()
    {
        if (bossHealthController == null) return true;
        
        // 檢查Boss的血量是否為0
        return bossHealthController.GetHealthPercentage() <= 0f;
    }

    // Boss死亡時調用
    public void OnBossDeath()
    {
        // 隱藏Boss血條
        if (bossHealthController != null)
        {
            bossHealthController.HideHealthBar();
        }
        
        // 淡出回到原始音樂
        if (backgroundMusicSource != null && originalMusic != null)
        {
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            musicFadeCoroutine = StartCoroutine(FadeToOriginalMusic());
        }
        
        Debug.Log("Boss已死亡，隱藏血條並恢復音樂");
    }
    
    // 在Scene視圖中顯示觸發範圍
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
} 