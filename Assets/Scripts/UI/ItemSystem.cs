using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 道具系統
public class ItemSystem : MonoBehaviour
{
    [System.Serializable]
    public class ItemData
    {
        public ItemType type; // 道具類型
        public string name; // 道具名稱
        public int quantity; // 道具數量
        public float effectValue; // 道具效果值
        public float duration; // 道具持續時間
        public float cooldown; // 道具冷卻時間
        public Sprite icon; // 道具圖示
        public string description; // 道具描述
        public GameObject useEffect; // 使用效果
        public AudioClip useSound; // 使用音效
    }
    
    public enum ItemType
    {
        MedicinePill,    // 藥丸：緩慢恢復HP
        SteelSugar,      // 剛幹糖：減少架勢增加
        HealingGourd     // 傷藥葫蘆：立即恢復HP
    }
    
    [Header("道具設定")]
    public List<ItemData> items = new List<ItemData>();
    
    [Header("UI 組件")]
    public Transform itemContainer; // 道具槽容器
    public GameObject itemSlotPrefab; // 道具槽預製體
    public TMP_Text currentItemText; // 當前道具文字 (TextMeshPro)
    public Image currentItemIcon; // 當前道具圖示
    public Image useProgressBar; // 使用進度條
    
    [Header("效果設定")]
    public float useAnimationTime = 1.5f;
    public GameObject useEffectPrefab;
    
    // 私有變數
    private int currentIndex = 0;
    private bool isUsingItem = false;
    private HealthPostureController healthController;
    private AudioSource audioSource;
    private List<ItemSlotUI> slots = new List<ItemSlotUI>();
    
    // 事件
    public System.Action<ItemData> OnItemUsed;
    public System.Action<ItemData> OnItemSwitched;
    
    void Start()
    {
        Debug.Log("[ItemSystem] 開始初始化道具系統");
        InitializeSystem();
        CreateDefaultItems();
        CreateUI();
        Debug.Log($"[ItemSystem] 初始化完成，共有 {items.Count} 個道具");
    }
    
    void Update()
    {
        HandleInput();
        UpdateUI();
    }
    
    void InitializeSystem()
    {
        Debug.Log("[ItemSystem] 初始化系統組件");
        healthController = GetComponent<HealthPostureController>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        Debug.Log($"[ItemSystem] 組件初始化完成 - HealthController: {(healthController != null ? "找到" : "未找到")}, AudioSource: {(audioSource != null ? "找到" : "未找到")}");
    }
    
    void CreateDefaultItems()
    {
        Debug.Log("[ItemSystem] 創建預設道具");
        if (items.Count == 0)
        {
            items.Add(new ItemData
            {
                type = ItemType.MedicinePill,
                name = "藥丸",
                quantity = 3,
                effectValue = 3f,
                duration = 30f, 
                cooldown = 30f,
                description = "緩慢恢復生命值"
            });
            Debug.Log("[ItemSystem] 添加藥丸 x3");
            
            items.Add(new ItemData
            {
                type = ItemType.SteelSugar,
                name = "剛幹糖",
                quantity = 3,
                effectValue = 10f,
                duration = 30f,
                cooldown = 30f,
                description = "減少架勢增加"
            });
            Debug.Log("[ItemSystem] 添加剛幹糖 x3");
            
            items.Add(new ItemData
            {
                type = ItemType.HealingGourd,
                name = "傷藥葫蘆",
                quantity = 10,
                effectValue = 40f,
                duration = 1f,
                cooldown = 1f,
                description = "立即恢復生命值"
            });
            Debug.Log("[ItemSystem] 添加傷藥葫蘆 x10");
        }
        else
        {
            Debug.Log($"[ItemSystem] 道具已存在，跳過創建預設道具");
        }
    }
    
    void CreateUI()
    {
        Debug.Log("[ItemSystem] 創建UI");
        if (itemContainer == null || itemSlotPrefab == null) 
        {
            Debug.LogError("[ItemSystem] UI組件缺失 - itemContainer或itemSlotPrefab為null");
            return;
        }
        
        // 清除現有UI
        foreach (Transform child in itemContainer) Destroy(child.gameObject);
        slots.Clear();
        Debug.Log("[ItemSystem] 清除現有UI");
        
        // 創建道具槽
        for (int i = 0; i < items.Count; i++)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemContainer);
            ItemSlotUI slot = slotObj.GetComponent<ItemSlotUI>();
            if (slot != null)
            {
                slot.Initialize(items[i], i == currentIndex);
                slots.Add(slot);
                Debug.Log($"[ItemSystem] 創建道具槽 {i}: {items[i].name}");
            }
            else
            {
                Debug.LogError($"[ItemSystem] 道具槽 {i} 組件獲取失敗");
            }
        }
        Debug.Log($"[ItemSystem] UI創建完成，共創建 {slots.Count} 個道具槽");
    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[ItemSystem] 按下R鍵 - 切換道具");
            SwitchItem();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[ItemSystem] 按下E鍵 - 使用道具");
            UseCurrentItem();
        }
    }
    
    void SwitchItem()
    {
        if (items.Count == 0) 
        {
            Debug.LogWarning("[ItemSystem] 無法切換道具 - 道具列表為空");
            return;
        }
        
        int oldIndex = currentIndex;
        currentIndex = (currentIndex + 1) % items.Count;
        Debug.Log($"[ItemSystem] 切換道具: {oldIndex} -> {currentIndex} ({items[currentIndex].name})");
        UpdateSlotSelection();
        OnItemSwitched?.Invoke(items[currentIndex]);
    }
    
    void UseCurrentItem()
    {
        if (isUsingItem) 
        {
            Debug.Log("[ItemSystem] 無法使用道具 - 正在使用中");
            return;
        }
        
        if (items.Count == 0) 
        {
            Debug.LogWarning("[ItemSystem] 無法使用道具 - 道具列表為空");
            return;
        }
        
        // 檢查 currentIndex 是否在有效範圍內
        if (currentIndex >= items.Count)
        {
            Debug.LogWarning($"[ItemSystem] currentIndex超出範圍 ({currentIndex} >= {items.Count})，重置為0");
            currentIndex = 0;
        }
        
        ItemData item = items[currentIndex];
        Debug.Log($"[ItemSystem] 嘗試使用道具: {item.name} (數量: {item.quantity}, 索引: {currentIndex})");
        
        if (item.quantity <= 0)
        {
            Debug.LogWarning($"[ItemSystem] 道具 {item.name} 數量不足");
            return;
        }
        
        if (currentIndex >= slots.Count)
        {
            Debug.LogError($"[ItemSystem] slots索引超出範圍 ({currentIndex} >= {slots.Count})");
            return;
        }
        
        if (slots[currentIndex].IsOnCooldown())
        {
            Debug.Log($"[ItemSystem] 道具 {item.name} 正在冷卻中");
            return;
        }
        
        Debug.Log($"[ItemSystem] 開始使用道具: {item.name}");
        StartCoroutine(UseItemCoroutine(item));
    }
    
    IEnumerator UseItemCoroutine(ItemData item)
    {
        Debug.Log($"[ItemSystem] 開始使用道具協程: {item.name}");
        isUsingItem = true;
        
        // 播放使用動畫
        if (useProgressBar != null)
        {
            Debug.Log($"[ItemSystem] 播放使用動畫，持續時間: {useAnimationTime}秒");
            float elapsed = 0f;
            while (elapsed < useAnimationTime)
            {
                elapsed += Time.deltaTime;
                useProgressBar.fillAmount = elapsed / useAnimationTime;
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("[ItemSystem] useProgressBar為null，跳過動畫");
        }
        
        // 應用效果
        Debug.Log($"[ItemSystem] 應用道具效果: {item.name}");
        ApplyItemEffect(item);
        
        // 減少數量
        item.quantity--;
        Debug.Log($"[ItemSystem] 道具 {item.name} 數量減少為: {item.quantity}");
        
        // 開始冷卻
        if (currentIndex < slots.Count)
        {
            slots[currentIndex].StartCooldown(item.cooldown);
            Debug.Log($"[ItemSystem] 開始冷卻: {item.name} ({item.cooldown}秒)");
        }
        else
        {
            Debug.LogError($"[ItemSystem] 無法開始冷卻 - slots索引超出範圍");
        }
        
        // 播放效果
        PlayUseEffect(item);
        
        // 觸發事件
        OnItemUsed?.Invoke(item);
        
        isUsingItem = false;
        if (useProgressBar != null) useProgressBar.fillAmount = 0f;
        Debug.Log($"[ItemSystem] 道具使用完成: {item.name}");
    }
    
    void ApplyItemEffect(ItemData item)
    {
        if (healthController == null) 
        {
            Debug.LogError("[ItemSystem] 無法應用道具效果 - HealthController為null");
            return;
        }
        
        Debug.Log($"[ItemSystem] 應用道具效果: {item.name} (類型: {item.type}, 效果值: {item.effectValue}, 持續時間: {item.duration})");
        
        switch (item.type)
        {
            case ItemType.MedicinePill:
                Debug.Log($"[ItemSystem] 啟動藥丸效果: 每秒恢復{item.effectValue}HP，持續{item.duration}秒");
                StartCoroutine(HealOverTime(item.effectValue, item.duration)); // 緩慢恢復生命值
                break;
            case ItemType.SteelSugar:
                Debug.Log($"[ItemSystem] 啟動剛幹糖效果: 架勢減少{item.effectValue}，持續{item.duration}秒");
                StartCoroutine(ReducePostureGain(item.effectValue, item.duration)); // 減少架勢增加
                break;
            case ItemType.HealingGourd:
                Debug.Log($"[ItemSystem] 立即恢復生命值: {item.effectValue}HP");
                healthController.HealHealth((int)item.effectValue); // 立即恢復生命值
                break;
            default:
                Debug.LogWarning($"[ItemSystem] 未知的道具類型: {item.type}");
                break;
        }
    }
    
    IEnumerator HealOverTime(float healPerSecond, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (healthController != null)
            {
                healthController.HealHealth((int)(healPerSecond * Time.deltaTime));
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    IEnumerator ReducePostureGain(float reduction, float duration)
    {
        // 這裡可以設置架勢減少效果
        yield return new WaitForSeconds(duration);
    }
    
    void PlayUseEffect(ItemData item)
    {
        if (item.useSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(item.useSound);
        }
        
        if (item.useEffect != null)
        {
            Instantiate(item.useEffect, transform.position, Quaternion.identity);
        }
        else if (useEffectPrefab != null)
        {
            Instantiate(useEffectPrefab, transform.position, Quaternion.identity);
        }
    }
    
    void UpdateUI()
    {
        if (items.Count == 0) return;
        
        // 確保 currentIndex 在有效範圍內
        if (currentIndex >= items.Count)
        {
            currentIndex = 0;
        }
        
        ItemData currentItem = items[currentIndex];
        
        if (currentItemText != null)
        {
            currentItemText.text = $"{currentItem.name} x{currentItem.quantity}";
        }
        
        if (currentItemIcon != null && currentItem.icon != null)
        {
            currentItemIcon.sprite = currentItem.icon;
        }
        
        // 更新所有槽位
        for (int i = 0; i < slots.Count && i < items.Count; i++)
        {
            slots[i].UpdateQuantity(items[i].quantity);
        }
    }
    
    void UpdateSlotSelection()
    {
        // 確保 currentIndex 在有效範圍內
        if (currentIndex >= items.Count)
        {
            currentIndex = 0;
        }
        
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetSelected(i == currentIndex);
        }
    }
    
    // 公共方法
    public void AddItem(ItemType type, int quantity = 1)
    {
        Debug.Log($"[ItemSystem] 嘗試添加道具: {type} x{quantity}");
        ItemData item = items.Find(x => x.type == type);
        if (item != null)
        {
            int oldQuantity = item.quantity;
            item.quantity += quantity;
            Debug.Log($"[ItemSystem] 道具 {item.name} 數量更新: {oldQuantity} -> {item.quantity}");
        }
        else
        {
            Debug.LogWarning($"[ItemSystem] 找不到道具類型: {type}");
        }
    }
    
    public int GetItemQuantity(ItemType type)
    {
        ItemData item = items.Find(x => x.type == type);
        return item?.quantity ?? 0;
    }
    
    public ItemData GetCurrentItem()
    {
        if (items.Count == 0) return null;
        
        // 確保 currentIndex 在有效範圍內
        if (currentIndex >= items.Count)
        {
            currentIndex = 0;
        }
        
        return items[currentIndex];
    }

    public float GetPostureReductionRate()
    {
        // 這裡假設 SteelSugar 效果啟用時，duration > 0
        var steelSugar = items.Find(x => x.type == ItemType.SteelSugar && x.duration > 0);
        if (steelSugar != null)
        {
            Debug.Log($"[ItemSystem] 架勢減少倍率: {steelSugar.effectValue}");
            return steelSugar.effectValue;
        }
        Debug.Log("[ItemSystem] 架勢減少倍率: 1.0 (無效果)");
        return 1f;
    }
}