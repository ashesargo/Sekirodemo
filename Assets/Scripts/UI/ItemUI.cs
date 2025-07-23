using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 道具UI系統
public class ItemUI : MonoBehaviour
{
    [Header("UI 組件")]
    public GameObject itemSlotPrefab;  // 道具槽預製體
    public Transform itemContainer;    // 道具容器
    public Text itemDescriptionText;   // 道具描述文字
    public Image itemIconImage;        // 道具圖示
    
    [Header("道具圖示")]
    public Sprite medicinePillIcon;
    public Sprite steelSugarIcon;
    public Sprite healingGourdIcon;
    
    [Header("效果顯示")]
    public GameObject medicinePillEffect;  // 藥丸效果UI
    public GameObject steelSugarEffect;    // 剛幹糖效果UI
    public Image medicinePillEffectBar;    // 藥丸效果進度條
    public Image steelSugarEffectBar;      // 剛幹糖效果進度條
    
    private ItemSystem itemSystem;
    private List<ItemSlotUI> itemSlots = new List<ItemSlotUI>();
    private Dictionary<ItemType, Sprite> itemIcons;
    
    void Start()
    {
        // 獲取道具系統
        itemSystem = FindObjectOfType<ItemSystem>();
        if (itemSystem == null)
        {
            Debug.LogError("找不到 ItemSystem 組件！");
            return;
        }
        
        // 初始化道具圖示字典
        InitializeItemIcons();
        
        // 訂閱道具系統事件
        itemSystem.OnItemUsed += OnItemUsed;
        itemSystem.OnItemAdded += OnItemAdded;
        itemSystem.OnItemRemoved += OnItemRemoved;
        
        // 初始化UI
        InitializeItemUI();
    }
    
    void Update()
    {
        // 更新效果進度條
        UpdateEffectBars();
        
        // 更新道具描述
        UpdateItemDescription();
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
    
    // 初始化道具UI
    void InitializeItemUI()
    {
        // 清除現有道具槽
        ClearItemSlots();
        
        // 為每種道具類型創建道具槽
        CreateItemSlot(ItemType.MedicinePill, "1");
        CreateItemSlot(ItemType.SteelSugar, "2");
        CreateItemSlot(ItemType.HealingGourd, "3");
        
        // 更新道具數量
        UpdateAllItemQuantities();
    }
    
    // 創建道具槽
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
        // 需要從 ItemSystem 獲取當前效果的剩餘時間
    }
    
    // 更新道具描述
    void UpdateItemDescription()
    {
        // 根據滑鼠懸停的道具顯示描述
        // 這裡可以添加滑鼠懸停檢測邏輯
    }
    
    // 道具使用事件處理
    void OnItemUsed(Item item)
    {
        // 更新道具數量
        UpdateAllItemQuantities();
        
        // 顯示使用效果
        ShowItemUseEffect(item.itemType);
        
        // 更新道具描述
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = $"使用了 {item.itemName}";
            StartCoroutine(ClearDescriptionAfterDelay(2f));
        }
    }
    
    // 道具添加事件處理
    void OnItemAdded(Item item)
    {
        UpdateAllItemQuantities();
        
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = $"獲得了 {item.itemName} x{item.quantity}";
            StartCoroutine(ClearDescriptionAfterDelay(2f));
        }
    }
    
    // 道具移除事件處理
    void OnItemRemoved(Item item)
    {
        UpdateAllItemQuantities();
        
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = $"{item.itemName} 已用完";
            StartCoroutine(ClearDescriptionAfterDelay(2f));
        }
    }
    
    // 顯示道具使用效果
    void ShowItemUseEffect(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.MedicinePill:
                if (medicinePillEffect != null)
                {
                    StartCoroutine(ShowEffectTemporarily(medicinePillEffect, 2f));
                }
                break;
            case ItemType.SteelSugar:
                if (steelSugarEffect != null)
                {
                    StartCoroutine(ShowEffectTemporarily(steelSugarEffect, 2f));
                }
                break;
            case ItemType.HealingGourd:
                // 傷藥葫蘆的立即效果
                break;
        }
    }
    
    // 暫時顯示效果
    IEnumerator ShowEffectTemporarily(GameObject effect, float duration)
    {
        effect.SetActive(true);
        yield return new WaitForSeconds(duration);
        effect.SetActive(false);
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
    
    // 設定道具描述
    public void SetItemDescription(string description)
    {
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = description;
        }
    }
    
    // 清除道具描述
    public void ClearItemDescription()
    {
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = "";
        }
    }
} 