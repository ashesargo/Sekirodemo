using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomSoundPlayer : MonoBehaviour
{
    [Header("音效設定")]
    [Tooltip("要隨機播放的音效陣列")]
    public AudioClip[] audioClips;
    
    [Tooltip("是否在粒子開始時播放音效")]
    public bool playOnParticleStart = true;
    
    [Tooltip("是否在粒子結束時播放音效")]
    public bool playOnParticleEnd = false;
    
    [Tooltip("是否避免重複播放同一個音效")]
    public bool avoidRepeat = true;
    
    [Header("音量設定")]
    [Tooltip("音量範圍 (0-1)")]
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Tooltip("是否隨機音量")]
    public bool randomVolume = false;
    
    [Tooltip("隨機音量範圍")]
    [Range(0f, 1f)]
    public float minVolume = 0.7f;
    
    [Tooltip("隨機音量範圍")]
    [Range(0f, 1f)]
    public float maxVolume = 1f;
    
    [Header("音調設定")]
    [Tooltip("是否隨機音調")]
    public bool randomPitch = false;
    
    [Tooltip("隨機音調範圍")]
    [Range(0.5f, 2f)]
    public float minPitch = 0.8f;
    
    [Tooltip("隨機音調範圍")]
    [Range(0.5f, 2f)]
    public float maxPitch = 1.2f;

    private AudioSource audioSource;
    private ParticleSystem particleSystemComponent;
    private int lastIndex = -1;

    void Start()
    {
        // 獲取組件
        audioSource = GetComponent<AudioSource>();
        particleSystemComponent = GetComponent<ParticleSystem>();
        
        // 設定 AudioSource 基本屬性
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        
        // 如果沒有粒子系統，直接播放
        if (particleSystemComponent == null)
        {
            if (playOnParticleStart)
            {
                PlayRandomSound();
            }
            return;
        }
        
        // 設定粒子系統事件
        if (playOnParticleStart)
        {
            var main = particleSystemComponent.main;
            main.playOnAwake = false;
            particleSystemComponent.Play();
            PlayRandomSound();
        }
        
        if (playOnParticleEnd)
        {
            // 監聽粒子系統停止事件
            StartCoroutine(WaitForParticleEnd());
        }
    }

    /// <summary>
    /// 播放隨機音效
    /// </summary>
    public void PlayRandomSound()
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("RandomSoundPlayer: 沒有設定音效檔案！");
            return;
        }

        // 選擇隨機音效索引
        int newIndex;
        if (avoidRepeat && audioClips.Length > 1)
        {
            do
            {
                newIndex = Random.Range(0, audioClips.Length);
            } while (newIndex == lastIndex);
        }
        else
        {
            newIndex = Random.Range(0, audioClips.Length);
        }
        
        lastIndex = newIndex;
        
        // 設定音效
        audioSource.clip = audioClips[newIndex];
        
        // 隨機音量
        if (randomVolume)
        {
            audioSource.volume = Random.Range(minVolume, maxVolume);
        }
        else
        {
            audioSource.volume = volume;
        }
        
        // 隨機音調
        if (randomPitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
        }
        else
        {
            audioSource.pitch = 1f;
        }
        
        // 播放音效
        audioSource.Play();
        
        Debug.Log($"RandomSoundPlayer: 播放音效 {audioClips[newIndex].name}");
    }

    /// <summary>
    /// 等待粒子系統結束後播放音效
    /// </summary>
    private System.Collections.IEnumerator WaitForParticleEnd()
    {
        // 等待粒子系統播放完成
        while (particleSystemComponent.isPlaying)
        {
            yield return null;
        }
        
        // 粒子系統結束後播放音效
        PlayRandomSound();
    }

    /// <summary>
    /// 手動觸發播放（可用於外部調用）
    /// </summary>
    public void TriggerPlay()
    {
        PlayRandomSound();
    }

    /// <summary>
    /// 設定音效陣列
    /// </summary>
    public void SetAudioClips(AudioClip[] clips)
    {
        audioClips = clips;
    }

    /// <summary>
    /// 添加單個音效
    /// </summary>
    public void AddAudioClip(AudioClip clip)
    {
        if (audioClips == null)
        {
            audioClips = new AudioClip[0];
        }
        
        System.Array.Resize(ref audioClips, audioClips.Length + 1);
        audioClips[audioClips.Length - 1] = clip;
    }
} 