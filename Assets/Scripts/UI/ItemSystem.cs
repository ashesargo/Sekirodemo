using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject useEffectPosition; // 使用效果位置
    
    [Header("效果設定")]
    public float useAnimationTime = 1.5f;
    public GameObject useEffectPrefab;
    
    // 私有變數
    private int currentIndex = 0;
    private bool isUsingItem = false;
    private HealthPostureController healthController;
    private AudioSource audioSource;
    private List<ItemSlotUI> slots = new List<ItemSlotUI>();
    private Animator animator; // 動畫控制器
    
    // 事件
    public System.Action<ItemData> OnItemUsed;
    public System.Action<ItemData> OnItemSwitched;
    
    void Start()
    {
        InitializeSystem();
        CreateDefaultItems();
        CreateUI();
    }
    
    void Update()
    {
        HandleInput();
        UpdateUI();
    }
    
    void InitializeSystem()
    {
        healthController = GetComponent<HealthPostureController>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }
    
    void CreateDefaultItems()
    {
        if (items.Count == 0)
        {
            items.Add(new ItemData
            {
                type = ItemType.MedicinePill,
                name = "藥丸",
                quantity = 3,
                effectValue = 5f, // 提高每秒恢復量到5點
                duration = 30f, 
                cooldown = 30f,
                description = "緩慢恢復生命值"
            });
            
            items.Add(new ItemData
            {
                type = ItemType.SteelSugar,
                name = "剛幹糖",
                quantity = 3,
                effectValue = 0.3f,
                duration = 30f,
                cooldown = 30f,
                description = "減少架勢增加"
            });
            
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
        }
    }
    
    void CreateUI()
    {
        if (itemContainer == null || itemSlotPrefab == null) 
        {
            Debug.LogError("[ItemSystem] UI組件缺失 - itemContainer或itemSlotPrefab為null");
            return;
        }
        
        // 檢查 itemSlotPrefab 是否有 ItemSlotUI 組件
        ItemSlotUI prefabSlot = itemSlotPrefab.GetComponent<ItemSlotUI>();
        if (prefabSlot == null)
        {
            Debug.LogError("[ItemSystem] itemSlotPrefab上沒有ItemSlotUI組件！");
            return;
        }
        
        // 清除現有UI
        foreach (Transform child in itemContainer) Destroy(child.gameObject);
        slots.Clear();
        
        // 創建道具槽
        for (int i = 0; i < items.Count; i++)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemContainer);
            ItemSlotUI slot = slotObj.GetComponent<ItemSlotUI>();
            if (slot != null)
            {
                try
                {
                    bool isSelected = (i == currentIndex);
                    slot.Initialize(items[i], isSelected);
                    slot.gameObject.SetActive(isSelected); // 只顯示當前選中的道具槽
                    slots.Add(slot);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ItemSystem] 初始化道具槽 {i} 時發生錯誤: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"[ItemSystem] 道具槽 {i} 組件獲取失敗");
            }
        }
    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SwitchItem();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
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
        
        // 檢查 slots 是否正確初始化
        if (slots.Count == 0)
        {
            CreateUI();
            if (slots.Count == 0)
            {
                Debug.LogError("[ItemSystem] SwitchItem: 重新創建UI失敗，無法切換道具");
                return;
            }
        }
        
        int oldIndex = currentIndex;
        
        // 正向切換：0->1->2->0->1->2...
        currentIndex = (currentIndex + 1) % items.Count;
        
        UpdateSlotSelection();
        OnItemSwitched?.Invoke(items[currentIndex]);
    }
    
    void UseCurrentItem()
    {
        if (isUsingItem) 
        {
            return;
        }
        
        if (items.Count == 0) 
        {
            Debug.LogWarning("[ItemSystem] 無法使用道具 - 道具列表為空");
            return;
        }
        
        // 確保 currentIndex 在有效範圍內（使用迴圈模式）
        if (currentIndex >= items.Count)
        {
            currentIndex = currentIndex % items.Count;
        }
        
        ItemData item = items[currentIndex];
        
        if (item.quantity <= 0)
        {
            Debug.LogWarning($"[ItemSystem] 道具 {item.name} 數量不足");
            return;
        }
        
        // 檢查 slots 是否正確初始化
        if (slots.Count == 0)
        {
            Debug.LogError($"[ItemSystem] slots列表為空！items數量: {items.Count}, currentIndex: {currentIndex}");
            return;
        }
        
        if (currentIndex >= slots.Count)
        {
            CreateUI();
            return;
        }
        
        if (slots[currentIndex].IsOnCooldown())
        {
            return;
        }
        
        StartCoroutine(UseItemCoroutine(item));
    }
    
    IEnumerator UseItemCoroutine(ItemData item)
    {
        isUsingItem = true;
        
        // 立即播放效果（按下E鍵的瞬間）
        PlayUseEffect(item);
        
        // 觸發動畫
        if (animator != null)
        {
            animator.SetTrigger("Heal");
        }
        
        // 等待使用動畫時間
        yield return new WaitForSeconds(useAnimationTime);
        
        // 應用效果
        ApplyItemEffect(item);
        
        // 減少數量
        item.quantity--;
        
        // 開始冷卻
        if (currentIndex < slots.Count)
        {
            slots[currentIndex].StartCooldown(item.cooldown);
        }
        
        // 觸發事件
        OnItemUsed?.Invoke(item);
        
        isUsingItem = false;
    }
    
    void ApplyItemEffect(ItemData item)
    {
        if (healthController == null) 
        {
            Debug.LogError("[ItemSystem] 無法應用道具效果 - HealthController為null");
            return;
        }
        
        switch (item.type)
        {
            case ItemType.MedicinePill:
                StartCoroutine(HealOverTime(item.effectValue, item.duration)); // 緩慢恢復生命值
                Debug.Log($"[ItemSystem] 使用藥丸，每秒恢復 {item.effectValue} 點生命值，持續 {item.duration} 秒");
                break;
            case ItemType.SteelSugar:
                StartCoroutine(ReducePostureGain(item.effectValue, item.duration)); // 減少架勢增加
                Debug.Log($"[ItemSystem] 使用剛幹糖，減少架勢增加 {item.effectValue}，持續 {item.duration} 秒");
                break;
            case ItemType.HealingGourd:
                healthController.HealHealth((int)item.effectValue); // 立即恢復生命值
                Debug.Log($"[ItemSystem] 使用傷藥葫蘆，立即恢復 {item.effectValue} 點生命值");
                break;
            default:
                Debug.LogWarning($"[ItemSystem] 未知的道具類型: {item.type}");
                break;
        }
    }
    
    IEnumerator HealOverTime(float healPerSecond, float duration)
    {
        float elapsed = 0f;
        float accumulatedHeal = 0f; // 累積的治療量
        int totalHealed = 0; // 總治療量（用於調試）
        
        Debug.Log($"[ItemSystem] 開始緩慢治療：每秒 {healPerSecond} 點，持續 {duration} 秒");
        
        while (elapsed < duration)
        {
            if (healthController != null)
            {
                // 累積治療量
                accumulatedHeal += healPerSecond * Time.deltaTime;
                
                // 當累積量達到1或以上時，進行治療
                if (accumulatedHeal >= 1f)
                {
                    int healAmount = Mathf.FloorToInt(accumulatedHeal);
                    healthController.HealHealth(healAmount);
                    totalHealed += healAmount;
                    accumulatedHeal -= healAmount; // 減去已治療的量
                    
                    Debug.Log($"[ItemSystem] 治療中：恢復 {healAmount} 點生命值，累計恢復 {totalHealed} 點");
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 治療結束時，處理剩餘的治療量
        if (healthController != null && accumulatedHeal > 0f)
        {
            int finalHeal = Mathf.FloorToInt(accumulatedHeal);
            healthController.HealHealth(finalHeal);
            totalHealed += finalHeal;
            Debug.Log($"[ItemSystem] 治療結束：最後恢復 {finalHeal} 點生命值，總共恢復 {totalHealed} 點");
        }
    }
    
    IEnumerator ReducePostureGain(float reduction, float duration)
    {
        // 設置架勢減少效果
        // 這裡可以通過修改 ItemData 的 duration 來標記效果是否啟用
        // 當 duration > 0 時，表示效果正在啟用中
        
        // 等待效果持續時間
        yield return new WaitForSeconds(duration);
        
        // 效果結束後，可以重置相關狀態
        Debug.Log("[ItemSystem] 剛幹糖效果結束");
    }
    
    void PlayUseEffect(ItemData item)
    {
        if (item.useSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(item.useSound);
        }
        
        if (item.useEffect != null)
        {
            Vector3 effectPosition = useEffectPosition.transform.position;
            Instantiate(item.useEffect, effectPosition, Quaternion.identity);
        }
        else if (useEffectPrefab != null)
        {
            Vector3 effectPosition = useEffectPosition.transform.position;
            Instantiate(useEffectPrefab, effectPosition, Quaternion.identity);
        }
    }
    
    void UpdateUI()
    {
        if (items.Count == 0) return;
        
        // 確保 currentIndex 在有效範圍內（使用迴圈模式）
        if (currentIndex >= items.Count)
        {
            currentIndex = currentIndex % items.Count;
        }
        
        ItemData currentItem = items[currentIndex];
        
        if (currentItemText != null)
        {
            currentItemText.text = $"{currentItem.name}";
        }
        
        // 檢查 slots 是否正確初始化
        if (slots.Count == 0)
        {
            return;
        }
        
        // 更新所有槽位
        for (int i = 0; i < slots.Count && i < items.Count; i++)
        {
            if (slots[i] != null)
            {
                slots[i].UpdateQuantity(items[i].quantity);
            }
        }
    }
    
    void UpdateSlotSelection()
    {
        // 確保 currentIndex 在有效範圍內（使用迴圈模式）
        if (currentIndex >= items.Count)
        {
            currentIndex = currentIndex % items.Count;
        }
        
        // 檢查 slots 是否正確初始化
        if (slots.Count == 0)
        {
            return;
        }
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null)
            {
                // 只顯示當前選中的道具槽，隱藏其他
                bool isSelected = (i == currentIndex);
                slots[i].SetSelected(isSelected);
                slots[i].gameObject.SetActive(isSelected);
            }
        }
    }
    
    // 公共方法
    public void AddItem(ItemType type, int quantity = 1)
    {
        ItemData item = items.Find(x => x.type == type);
        if (item != null)
        {
            item.quantity += quantity;
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
        
        // 確保 currentIndex 在有效範圍內（使用迴圈模式）
        if (currentIndex >= items.Count)
        {
            currentIndex = currentIndex % items.Count;
        }
        
        return items[currentIndex];
    }

    public float GetPostureReductionRate()
    {
        // 暫時禁用剛幹糖效果，總是返回 1.0f（無效果）
        return 1f;
        
        // 原始代碼（已註解）：
        // 這裡假設 SteelSugar 效果啟用時，duration > 0
        // var steelSugar = items.Find(x => x.type == ItemType.SteelSugar && x.duration > 0);
        // if (steelSugar != null)
        // {
        //     return steelSugar.effectValue;
        // }
        // return 1f;
    }
    

}