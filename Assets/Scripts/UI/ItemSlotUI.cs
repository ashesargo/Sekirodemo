using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 隻狼風格道具槽UI
public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 組件")]
    public Image itemIcon;
    public Text quantityText;
    public Text hotkeyText;
    public Image backgroundImage;
    public Image cooldownImage;
    
    [Header("顏色設定")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color emptyColor = Color.gray;
    public Color cooldownColor = new Color(0, 0, 0, 0.5f);
    public Color selectedColor = Color.green;
    
    // 私有變數
    private ItemType itemType;
    private string hotkey;
    private bool isOnCooldown = false;
    private bool isSelected = false;
    private float cooldownTime = 0f;
    private float maxCooldownTime = 0f;
    
    private ItemSystem itemSystem;
    private ItemUI itemUI;
    
    public ItemType ItemType => itemType;
    
    void Start()
    {
        itemSystem = FindObjectOfType<ItemSystem>();
        itemUI = FindObjectOfType<ItemUI>();
    }
    
    void Update()
    {
        UpdateCooldown();
    }
    
    // 初始化道具槽
    public void Initialize(ItemType type, string key, Sprite icon)
    {
        itemType = type;
        hotkey = key;
        
        if (itemIcon != null && icon != null)
        {
            itemIcon.sprite = icon;
        }
        
        if (hotkeyText != null)
        {
            hotkeyText.text = key;
        }
        
        UpdateQuantity(0);
    }
    
    // 更新道具數量
    public void UpdateQuantity(int quantity)
    {
        if (quantityText != null)
        {
            quantityText.text = quantity > 0 ? quantity.ToString() : "0";
        }
        
        UpdateSlotColor(quantity);
    }
    
    // 更新槽位顏色
    void UpdateSlotColor(int quantity)
    {
        if (backgroundImage != null)
        {
            if (quantity <= 0)
            {
                backgroundImage.color = emptyColor;
            }
            else if (isSelected)
            {
                backgroundImage.color = selectedColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }
    }
    
    // 開始冷卻
    public void StartCooldown(float duration)
    {
        isOnCooldown = true;
        cooldownTime = duration;
        maxCooldownTime = duration;
        
        if (cooldownImage != null)
        {
            cooldownImage.gameObject.SetActive(true);
            cooldownImage.color = cooldownColor;
        }
    }
    
    // 更新冷卻顯示
    void UpdateCooldown()
    {
        if (!isOnCooldown) return;
        
        cooldownTime -= Time.deltaTime;
        
        if (cooldownImage != null)
        {
            float fillAmount = cooldownTime / maxCooldownTime;
            cooldownImage.fillAmount = fillAmount;
        }
        
        if (cooldownTime <= 0f)
        {
            EndCooldown();
        }
    }
    
    // 結束冷卻
    void EndCooldown()
    {
        isOnCooldown = false;
        cooldownTime = 0f;
        
        if (cooldownImage != null)
        {
            cooldownImage.gameObject.SetActive(false);
        }
    }
    
    // 滑鼠進入事件
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = hoverColor;
        }
        
        ShowItemDescription();
    }
    
    // 滑鼠離開事件
    public void OnPointerExit(PointerEventData eventData)
    {
        int quantity = itemSystem != null ? itemSystem.GetItemQuantity(itemType) : 0;
        UpdateSlotColor(quantity);
        ClearItemDescription();
    }
    
    // 顯示道具描述
    void ShowItemDescription()
    {
        if (itemUI != null)
        {
            string description = GetItemDescription();
            itemUI.SetItemDescription(description);
        }
    }
    
    // 清除道具描述
    void ClearItemDescription()
    {
        if (itemUI != null)
        {
            itemUI.ClearItemDescription();
        }
    }
    
    // 獲取道具描述
    string GetItemDescription()
    {
        switch (itemType)
        {
            case ItemType.MedicinePill:
                return "藥丸：緩慢恢復生命值，持續10秒";
            case ItemType.SteelSugar:
                return "剛幹糖：減少敵人攻擊增長的架勢槽，持續15秒";
            case ItemType.HealingGourd:
                return "傷藥葫蘆：立即恢復30點生命值";
            default:
                return "";
        }
    }
    
    // 檢查是否可以使用道具
    public bool CanUseItem()
    {
        if (isOnCooldown) return false;
        
        if (itemSystem != null)
        {
            int quantity = itemSystem.GetItemQuantity(itemType);
            return quantity > 0;
        }
        
        return false;
    }
    
    // 使用道具
    public bool UseItem()
    {
        if (!CanUseItem()) return false;
        
        if (itemSystem != null)
        {
            bool success = itemSystem.UseItem(GetItemByType());
            
            if (success)
            {
                float cooldownDuration = GetCooldownDuration();
                StartCooldown(cooldownDuration);
                
                int newQuantity = itemSystem.GetItemQuantity(itemType);
                UpdateQuantity(newQuantity);
            }
            
            return success;
        }
        
        return false;
    }
    
    // 獲取道具
    Item GetItemByType()
    {
        return itemSystem != null ? itemSystem.GetItemByType(itemType) : null;
    }
    
    // 獲取冷卻時間
    float GetCooldownDuration()
    {
        switch (itemType)
        {
            case ItemType.MedicinePill:
                return 15f;
            case ItemType.SteelSugar:
                return 20f;
            case ItemType.HealingGourd:
                return 5f;
            default:
                return 10f;
        }
    }
    
    // 設定選中狀態
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (itemSystem != null)
        {
            int quantity = itemSystem.GetItemQuantity(itemType);
            UpdateSlotColor(quantity);
        }
    }
    
    // 獲取選中狀態
    public bool IsSelected()
    {
        return isSelected;
    }
    
    // 設定道具圖示
    public void SetItemIcon(Sprite icon)
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
        }
    }
    
    // 設定快捷鍵
    public void SetHotkey(string key)
    {
        hotkey = key;
        if (hotkeyText != null)
        {
            hotkeyText.text = key;
        }
    }
} 