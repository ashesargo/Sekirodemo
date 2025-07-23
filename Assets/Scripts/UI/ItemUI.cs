using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 隻狼風格道具UI系統
public class ItemUI : MonoBehaviour
{
    [Header("UI 組件")]
    public GameObject itemSlotPrefab;
    public Transform itemContainer;
    public Text itemDescriptionText;
    public Image itemIconImage;
    
    [Header("道具圖示")]
    public Sprite medicinePillIcon;
    public Sprite steelSugarIcon;
    public Sprite healingGourdIcon;
    
    [Header("效果顯示")]
    public GameObject medicinePillEffect;
    public GameObject steelSugarEffect;
    public Image medicinePillEffectBar;
    public Image steelSugarEffectBar;
    
    [Header("當前選中道具顯示")]
    public Image currentItemIcon;
    public Text currentItemName;
    public Text currentItemQuantity;
    public GameObject selectionHighlight;
    
    [Header("隻狼風格UI")]
    public GameObject sekiroStyleUI;
    public Image itemUseProgressBar;
    public GameObject itemUseAnimation;
    public AudioClip itemUseSound;
    
    // 私有變數
    private ItemSystem itemSystem;
    private List<ItemSlotUI> itemSlots = new List<ItemSlotUI>();
    private Dictionary<ItemType, Sprite> itemIcons;
    
    void Start()
    {
        InitializeSystem();
    }
    
    void Update()
    {
        UpdateEffectBars();
        UpdateItemDescription();
        UpdateCurrentItemDisplay();
    }
    
    // 初始化系統
    void InitializeSystem()
    {
        itemSystem = FindObjectOfType<ItemSystem>();
        if (itemSystem == null)
        {
            Debug.LogError("找不到 ItemSystem 組件！");
            return;
        }
        
        InitializeItemIcons();
        SubscribeToEvents();
        InitializeItemUI();
    }
    
    // 初始化道具圖示
    void InitializeItemIcons()
    {
        itemIcons = new Dictionary<ItemType, Sprite>
        {
            { ItemType.MedicinePill, medicinePillIcon },
            { ItemType.SteelSugar, steelSugarIcon },
            { ItemType.HealingGourd, healingGourdIcon }
        };
    }
    
    // 訂閱事件
    void SubscribeToEvents()
    {
        itemSystem.OnItemUsed += OnItemUsed;
        itemSystem.OnItemAdded += OnItemAdded;
        itemSystem.OnItemRemoved += OnItemRemoved;
        itemSystem.OnItemSwitched += OnItemSwitched;
    }
    
    // 初始化道具UI
    void InitializeItemUI()
    {
        ClearItemSlots();
        CreateItemSlots();
        UpdateAllItemQuantities();
    }
    
    // 創建道具槽
    void CreateItemSlots()
    {
        CreateItemSlot(ItemType.MedicinePill, "1");
        CreateItemSlot(ItemType.SteelSugar, "2");
        CreateItemSlot(ItemType.HealingGourd, "3");
    }
    
    // 創建單個道具槽
    void CreateItemSlot(ItemType itemType, string hotkey)
    {
        if (itemSlotPrefab == null || itemContainer == null) return;
        
        GameObject slotObj = Instantiate(itemSlotPrefab, itemContainer);
        ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();
        
        if (slotUI != null)
        {
            slotUI.Initialize(itemType, hotkey, GetItemIcon(itemType));
            itemSlots.Add(slotUI);
        }
    }
    
    // 獲取道具圖示
    Sprite GetItemIcon(ItemType itemType)
    {
        return itemIcons.ContainsKey(itemType) ? itemIcons[itemType] : null;
    }
    
    // 清除道具槽
    void ClearItemSlots()
    {
        foreach (ItemSlotUI slot in itemSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        itemSlots.Clear();
    }
    
    // 更新所有道具數量
    void UpdateAllItemQuantities()
    {
        foreach (ItemSlotUI slot in itemSlots)
        {
            if (slot != null)
            {
                int quantity = itemSystem.GetItemQuantity(slot.ItemType);
                slot.UpdateQuantity(quantity);
            }
        }
    }
    
    // 更新效果進度條
    void UpdateEffectBars()
    {
        // 這裡可以添加效果進度條的更新邏輯
    }
    
    // 更新道具描述
    void UpdateItemDescription()
    {
        // 根據滑鼠懸停的道具顯示描述
    }
    
    // 更新當前選中道具顯示
    void UpdateCurrentItemDisplay()
    {
        if (itemSystem == null) return;
        
        Item currentItem = itemSystem.GetCurrentSelectedItem();
        UpdateItemSlotSelection();
        UpdateCurrentItemInfo(currentItem);
    }
    
    // 更新道具槽選中狀態
    void UpdateItemSlotSelection()
    {
        if (itemSystem == null) return;
        
        Item currentItem = itemSystem.GetCurrentSelectedItem();
        
        foreach (ItemSlotUI slot in itemSlots)
        {
            if (slot != null)
            {
                bool isSelected = (currentItem != null && slot.ItemType == currentItem.itemType);
                slot.SetSelected(isSelected);
            }
        }
    }
    
    // 更新當前道具信息
    void UpdateCurrentItemInfo(Item currentItem)
    {
        if (currentItem != null)
        {
            UpdateCurrentItemUI(currentItem);
        }
        else
        {
            ClearCurrentItemUI();
        }
    }
    
    // 更新當前道具UI
    void UpdateCurrentItemUI(Item item)
    {
        if (currentItemIcon != null)
        {
            Sprite icon = GetItemIcon(item.itemType);
            if (icon != null)
            {
                currentItemIcon.sprite = icon;
                currentItemIcon.gameObject.SetActive(true);
            }
        }
        
        if (currentItemName != null)
        {
            currentItemName.text = item.itemName;
        }
        
        if (currentItemQuantity != null)
        {
            currentItemQuantity.text = $"x{item.quantity}";
        }
        
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(true);
        }
    }
    
    // 清除當前道具UI
    void ClearCurrentItemUI()
    {
        if (currentItemIcon != null)
        {
            currentItemIcon.gameObject.SetActive(false);
        }
        
        if (currentItemName != null)
        {
            currentItemName.text = "無道具";
        }
        
        if (currentItemQuantity != null)
        {
            currentItemQuantity.text = "";
        }
        
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(false);
        }
    }
    
    // 事件處理方法
    void OnItemUsed(Item item)
    {
        UpdateAllItemQuantities();
        ShowItemUseEffect(item.itemType);
        StartCoroutine(PlaySekiroItemUseAnimation());
        ShowTemporaryMessage($"使用了 {item.itemName}", 2f);
    }
    
    void OnItemAdded(Item item)
    {
        UpdateAllItemQuantities();
        ShowTemporaryMessage($"獲得了 {item.itemName} x{item.quantity}", 2f);
    }
    
    void OnItemRemoved(Item item)
    {
        UpdateAllItemQuantities();
        ShowTemporaryMessage($"{item.itemName} 已用完", 2f);
    }
    
    void OnItemSwitched(Item item)
    {
        UpdateCurrentItemDisplay();
        UpdateAllItemQuantities();
        
        if (item != null)
        {
            ShowTemporaryMessage($"切換到 {item.itemName}", 1f);
        }
    }
    
    // 顯示道具使用效果
    void ShowItemUseEffect(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.MedicinePill:
                ShowEffectTemporarily(medicinePillEffect, 2f);
                break;
            case ItemType.SteelSugar:
                ShowEffectTemporarily(steelSugarEffect, 2f);
                break;
        }
    }
    
    // 暫時顯示效果
    IEnumerator ShowEffectTemporarily(GameObject effect, float duration)
    {
        if (effect != null)
        {
            effect.SetActive(true);
            yield return new WaitForSeconds(duration);
            effect.SetActive(false);
        }
    }
    
    // 播放隻狼風格道具使用動畫
    IEnumerator PlaySekiroItemUseAnimation()
    {
        if (itemUseAnimation != null)
        {
            itemUseAnimation.SetActive(true);
        }
        
        if (itemUseProgressBar != null)
        {
            float duration = 1.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                itemUseProgressBar.fillAmount = progress;
                yield return null;
            }
            
            itemUseProgressBar.fillAmount = 1f;
        }
        
        if (itemUseSound != null)
        {
            AudioSource.PlayClipAtPoint(itemUseSound, Camera.main.transform.position);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (itemUseAnimation != null)
        {
            itemUseAnimation.SetActive(false);
        }
        
        if (itemUseProgressBar != null)
        {
            itemUseProgressBar.fillAmount = 0f;
        }
    }
    
    // 顯示臨時消息
    void ShowTemporaryMessage(string message, float duration)
    {
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = message;
            StartCoroutine(ClearDescriptionAfterDelay(duration));
        }
    }
    
    // 延遲清除描述
    IEnumerator ClearDescriptionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = "";
        }
    }
    
    // 公共方法
    public void SetItemDescription(string description)
    {
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = description;
        }
    }
    
    public void ClearItemDescription()
    {
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = "";
        }
    }
} 