using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 道具槽UI組件
public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 組件")]
    public Image itemIcon;           // 道具圖示
    public Text quantityText;        // 數量文字
    public Text hotkeyText;          // 快捷鍵文字
    public Image backgroundImage;    // 背景圖片
    public Image cooldownImage;      // 冷卻圖片
    
    [Header("顏色設定")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color emptyColor = Color.gray;
    public Color cooldownColor = new Color(0, 0, 0, 0.5f);
    
    private ItemType itemType;
    private string hotkey;
    private bool isOnCooldown = false;
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
        // 更新冷卻顯示
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
        
        // 根據數量更新顏色
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
        
        // 顯示道具描述
        ShowItemDescription();
    }
    
    // 滑鼠離開事件
    public void OnPointerExit(PointerEventData eventData)
    {
        // 恢復正常顏色
        int quantity = itemSystem != null ? itemSystem.GetItemQuantity(itemType) : 0;
        UpdateSlotColor(quantity);
        
        // 清除道具描述
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
            bool success = itemSystem.UseItemByType(itemType);
            
            if (success)
            {
                // 開始冷卻（根據道具類型設定不同的冷卻時間）
                float cooldownDuration = GetCooldownDuration();
                StartCooldown(cooldownDuration);
                
                // 更新數量
                int newQuantity = itemSystem.GetItemQuantity(itemType);
                UpdateQuantity(newQuantity);
            }
            
            return success;
        }
        
        return false;
    }
    
    // 獲取冷卻時間
    float GetCooldownDuration()
    {
        switch (itemType)
        {
            case ItemType.MedicinePill:
                return 15f; // 藥丸冷卻15秒
            case ItemType.SteelSugar:
                return 20f; // 剛幹糖冷卻20秒
            case ItemType.HealingGourd:
                return 5f;  // 傷藥葫蘆冷卻5秒
            default:
                return 10f;
        }
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