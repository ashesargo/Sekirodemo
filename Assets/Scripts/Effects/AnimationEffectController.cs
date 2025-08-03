using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 動畫事件特效控制器
/// 用於在動畫的指定時間點觸發特效
/// </summary>
public class AnimationEffectController : MonoBehaviour
{
    [System.Serializable]
    public class EffectEvent
    {
        [Header("特效設定")]
        public string eventName = "EffectEvent"; // 事件名稱
        public GameObject effectPrefab; // 特效預製體
        public Transform spawnPoint; // 特效生成位置
        public Vector3 offset = Vector3.zero; // 位置偏移
        public Vector3 rotation = Vector3.zero; // 旋轉角度
        public float scale = 1f; // 縮放比例
        
        [Header("時間設定")]
        public float delay = 0f; // 延遲時間
        public float duration = 3f; // 特效持續時間
        public bool useAnimationTime = false; // 是否使用動畫時間
        
        [Header("條件設定")]
        public bool requireGrounded = false; // 是否需要在地面上
        public bool requireMoving = false; // 是否需要移動中
        public LayerMask targetLayer = -1; // 目標層級
        
        [Header("音效設定")]
        public AudioClip soundEffect; // 音效
        public float volume = 1f; // 音量
        public bool randomizePitch = false; // 是否隨機化音調
        public float pitchRange = 0.1f; // 音調變化範圍
    }

    [Header("特效事件列表")]
    public List<EffectEvent> effectEvents = new List<EffectEvent>();
    
    [Header("全局設定")]
    public bool enableEffects = true; // 是否啟用特效
    public Transform defaultSpawnPoint; // 預設生成位置
    public AudioSource audioSource; // 音效播放器
    
    [Header("調試設定")]
    public bool showDebugLogs = true; // 顯示調試日誌
    public bool showGizmos = true; // 顯示Gizmos
    
    private Animator animator;
    private Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();
    private List<GameObject> activeEffects = new List<GameObject>();
    
    // 事件回調
    public System.Action<EffectEvent> OnEffectTriggered;
    public System.Action<EffectEvent> OnEffectCompleted;
    public System.Action OnAllEffectsCompleted;

    void Start()
    {
        InitializeComponents();
    }

    void Update()
    {
        // 清理已銷毀的特效
        activeEffects.RemoveAll(effect => effect == null);
    }

    /// <summary>
    /// 初始化組件
    /// </summary>
    protected virtual void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        if (defaultSpawnPoint == null)
        {
            defaultSpawnPoint = transform;
        }
    }

    /// <summary>
    /// 觸發指定名稱的特效事件
    /// </summary>
    /// <param name="eventName">事件名稱</param>
    public void TriggerEffect(string eventName)
    {
        if (!enableEffects) return;
        
        EffectEvent effectEvent = effectEvents.Find(e => e.eventName == eventName);
        if (effectEvent != null)
        {
            StartCoroutine(ExecuteEffectEvent(effectEvent));
        }
        else
        {
            Debug.LogWarning($"[AnimationEffectController] 未找到名為 '{eventName}' 的特效事件");
        }
    }

    /// <summary>
    /// 觸發指定索引的特效事件
    /// </summary>
    /// <param name="eventIndex">事件索引</param>
    public void TriggerEffect(int eventIndex)
    {
        if (!enableEffects) return;
        
        if (eventIndex >= 0 && eventIndex < effectEvents.Count)
        {
            StartCoroutine(ExecuteEffectEvent(effectEvents[eventIndex]));
        }
        else
        {
            Debug.LogWarning($"[AnimationEffectController] 事件索引 {eventIndex} 超出範圍");
        }
    }

    /// <summary>
    /// 執行特效事件
    /// </summary>
    /// <param name="effectEvent">特效事件</param>
    private IEnumerator ExecuteEffectEvent(EffectEvent effectEvent)
    {
        // 檢查條件
        if (!CheckConditions(effectEvent))
        {
            yield break;
        }

        // 延遲執行
        if (effectEvent.delay > 0)
        {
            yield return new WaitForSeconds(effectEvent.delay);
        }

        // 生成特效
        GameObject effect = SpawnEffect(effectEvent);
        if (effect != null)
        {
            activeEffects.Add(effect);
            
            // 播放音效
            PlaySoundEffect(effectEvent);
            
            // 觸發事件回調
            OnEffectTriggered?.Invoke(effectEvent);
            
            if (showDebugLogs)
            {
                Debug.Log($"[AnimationEffectController] 觸發特效: {effectEvent.eventName}");
            }
            
            // 等待特效完成
            yield return new WaitForSeconds(effectEvent.duration);
            
            // 銷毀特效
            if (effect != null)
            {
                Destroy(effect);
            }
            
            // 觸發完成回調
            OnEffectCompleted?.Invoke(effectEvent);
        }
    }

    /// <summary>
    /// 檢查特效觸發條件
    /// </summary>
    /// <param name="effectEvent">特效事件</param>
    /// <returns>是否滿足條件</returns>
    protected virtual bool CheckConditions(EffectEvent effectEvent)
    {
        // 檢查是否在地面上
        if (effectEvent.requireGrounded)
        {
            // 這裡可以添加地面檢測邏輯
            // 例如：檢查是否在地面上
        }
        
        // 檢查是否在移動中
        if (effectEvent.requireMoving)
        {
            // 這裡可以添加移動檢測邏輯
            // 例如：檢查速度是否大於閾值
        }
        
        return true;
    }

    /// <summary>
    /// 生成特效
    /// </summary>
    /// <param name="effectEvent">特效事件</param>
    /// <returns>生成的特效物件</returns>
    protected virtual GameObject SpawnEffect(EffectEvent effectEvent)
    {
        if (effectEvent.effectPrefab == null)
        {
            Debug.LogWarning($"[AnimationEffectController] 特效預製體為空: {effectEvent.eventName}");
            return null;
        }

        // 計算生成位置
        Vector3 spawnPosition = CalculateSpawnPosition(effectEvent);
        Quaternion spawnRotation = CalculateSpawnRotation(effectEvent);

        // 生成特效
        GameObject effect = Instantiate(effectEvent.effectPrefab, spawnPosition, spawnRotation);
        
        // 設置縮放
        if (effectEvent.scale != 1f)
        {
            effect.transform.localScale = Vector3.one * effectEvent.scale;
        }

        // 設置父物件（可選）
        if (effectEvent.spawnPoint != null)
        {
            effect.transform.SetParent(effectEvent.spawnPoint);
        }

        return effect;
    }

    /// <summary>
    /// 計算特效生成位置
    /// </summary>
    /// <param name="effectEvent">特效事件</param>
    /// <returns>生成位置</returns>
    private Vector3 CalculateSpawnPosition(EffectEvent effectEvent)
    {
        Transform spawnTransform = effectEvent.spawnPoint != null ? effectEvent.spawnPoint : defaultSpawnPoint;
        Vector3 basePosition = spawnTransform.position;
        
        // 應用偏移
        Vector3 offset = spawnTransform.TransformDirection(effectEvent.offset);
        
        return basePosition + offset;
    }

    /// <summary>
    /// 計算特效生成旋轉
    /// </summary>
    /// <param name="effectEvent">特效事件</param>
    /// <returns>生成旋轉</returns>
    private Quaternion CalculateSpawnRotation(EffectEvent effectEvent)
    {
        Transform spawnTransform = effectEvent.spawnPoint != null ? effectEvent.spawnPoint : defaultSpawnPoint;
        Quaternion baseRotation = spawnTransform.rotation;
        
        // 應用額外旋轉
        Quaternion additionalRotation = Quaternion.Euler(effectEvent.rotation);
        
        return baseRotation * additionalRotation;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="effectEvent">特效事件</param>
    private void PlaySoundEffect(EffectEvent effectEvent)
    {
        if (effectEvent.soundEffect == null || audioSource == null) return;

        // 設置音調
        float pitch = 1f;
        if (effectEvent.randomizePitch)
        {
            pitch = Random.Range(1f - effectEvent.pitchRange, 1f + effectEvent.pitchRange);
        }
        
        audioSource.pitch = pitch;
        audioSource.volume = effectEvent.volume;
        audioSource.PlayOneShot(effectEvent.soundEffect);
    }

    /// <summary>
    /// 停止所有特效
    /// </summary>
    public void StopAllEffects()
    {
        // 停止所有協程
        foreach (var coroutine in activeCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeCoroutines.Clear();
        
        // 銷毀所有特效
        foreach (var effect in activeEffects)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        activeEffects.Clear();
        
        if (showDebugLogs)
        {
            Debug.Log("[AnimationEffectController] 停止所有特效");
        }
    }

    /// <summary>
    /// 停止指定特效
    /// </summary>
    /// <param name="eventName">事件名稱</param>
    public void StopEffect(string eventName)
    {
        if (activeCoroutines.ContainsKey(eventName))
        {
            StopCoroutine(activeCoroutines[eventName]);
            activeCoroutines.Remove(eventName);
        }
        
        // 移除對應的特效
        activeEffects.RemoveAll(effect => effect != null && effect.name.Contains(eventName));
    }

    /// <summary>
    /// 檢查特效是否正在播放
    /// </summary>
    /// <param name="eventName">事件名稱</param>
    /// <returns>是否正在播放</returns>
    public bool IsEffectPlaying(string eventName)
    {
        return activeCoroutines.ContainsKey(eventName);
    }

    /// <summary>
    /// 獲取活躍特效數量
    /// </summary>
    /// <returns>特效數量</returns>
    public int GetActiveEffectCount()
    {
        return activeEffects.Count;
    }

    /// <summary>
    /// 設置特效啟用狀態
    /// </summary>
    /// <param name="enabled">是否啟用</param>
    public void SetEffectsEnabled(bool enabled)
    {
        enableEffects = enabled;
        if (!enabled)
        {
            StopAllEffects();
        }
    }

    // === 動畫事件方法（可在Animator中調用） ===
    
    /// <summary>
    /// 動畫事件：觸發特效1
    /// </summary>
    public void OnAnimationEffect1()
    {
        TriggerEffect(0);
    }
    
    /// <summary>
    /// 動畫事件：觸發特效2
    /// </summary>
    public void OnAnimationEffect2()
    {
        TriggerEffect(1);
    }
    
    /// <summary>
    /// 動畫事件：觸發特效3
    /// </summary>
    public void OnAnimationEffect3()
    {
        TriggerEffect(2);
    }
    
    /// <summary>
    /// 動畫事件：觸發指定特效
    /// </summary>
    /// <param name="effectIndex">特效索引</param>
    public void OnAnimationEffect(int effectIndex)
    {
        TriggerEffect(effectIndex);
    }

    // === Gizmos 繪製 ===
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        foreach (var effectEvent in effectEvents)
        {
            if (effectEvent.effectPrefab == null) continue;
            
            Transform spawnTransform = effectEvent.spawnPoint != null ? effectEvent.spawnPoint : defaultSpawnPoint;
            if (spawnTransform == null) continue;
            
            Vector3 spawnPosition = CalculateSpawnPosition(effectEvent);
            
            // 繪製生成位置
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPosition, 0.1f);
            
            // 繪製連接線
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(spawnTransform.position, spawnPosition);
            
            // 繪製特效範圍
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(spawnPosition, Vector3.one * 0.5f);
        }
    }

    void OnDestroy()
    {
        StopAllEffects();
    }
} 