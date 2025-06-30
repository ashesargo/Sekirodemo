using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 道具欄 UI 管理器
public class ItemUI : MonoBehaviour
{
    [Header("UI 組件")]
    public GameObject itemPanel;
    public Transform itemSlotContainer;
    public GameObject itemSlotPrefab;
    
    [Header("快捷鍵")]
    public Transform quickSlotContainer;
    public GameObject quickSlotPrefab;
    
    [Header("道具詳情")]
    public GameObject itemDetailPanel;
    public Image itemDetailIcon;
    public TextMeshProUGUI itemDetailName;
    public TextMeshProUGUI itemDetailDescription;
    public TextMeshProUGUI itemDetailQuantity;
    public Button useItemButton;
    public Button dropItemButton;
    
    [Header("設定")]
    public bool showInventoryOnStart = false;
    public KeyCode toggleInventoryKey = KeyCode.I;
    
    private Inventory inventory;
    private List<GameObject> slotUIs = new List<GameObject>();
    private List<GameObject> quickSlotUIs = new List<GameObject>();
    private ItemSlot selectedSlot;
    
    private void Start()
    {
        // 獲取道具系統組件
        inventory = FindObjectOfType<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("沒有狗屁包包！");
            return;
        }
        
        // 初始化 UI
        InitializeUI();
        
        // 初始化狀態
        if (itemPanel != null)
            itemPanel.SetActive(showInventoryOnStart);
        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);
    }
    
    private void Update()
    {
        // 按下切換道具欄
        if (Input.GetKeyDown(toggleInventoryKey))
        {
            ToggleInventory();
        }
    }
    
    // 初始化 UI
    private void InitializeUI()
    {
        // 創建道具欄 UI
        CreateInventorySlots();
        
        // 創建快捷鍵 UI
        CreateQuickSlots();
        
        // 設定按鈕事件
        SetupButtons();
    }
    
    // 初始化道具欄 UI
    private void CreateInventorySlots()
    {
        if (itemSlotContainer == null || itemSlotPrefab == null) return;
        
        // 清除現有 UI
        foreach (var slot in slotUIs)
        {
            if (slot != null) DestroyImmediate(slot);
        }

        slotUIs.Clear();
        
        // 以道具欄最大槽數創建 UI
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            GameObject slotUI = Instantiate(itemSlotPrefab, itemSlotContainer);
            slotUIs.Add(slotUI);
            
            // 為每個槽位設定點擊事件
            Button slotButton = slotUI.GetComponent<Button>();
            if (slotButton != null)
            {
                int slotIndex = i; // 閉包變數，保存當前索引
                slotButton.onClick.AddListener(() => OnSlotClicked(slotIndex));
            }
        }
    }
    

    // 創建快捷鍵 UI
    private void CreateQuickSlots()
    {
        if (quickSlotContainer == null || quickSlotPrefab == null) return;
        
        // 清除現有的快捷鍵UI
        foreach (var slot in quickSlotUIs)
        {
            if (slot != null)
                DestroyImmediate(slot);
        }
        quickSlotUIs.Clear();
        
        // 根據快捷鍵數量創建UI
        for (int i = 0; i < inventory.quickSlotKeys.Length; i++)
        {
            GameObject slotUI = Instantiate(quickSlotPrefab, quickSlotContainer);
            quickSlotUIs.Add(slotUI);
            
            // 設定快捷鍵數字顯示
            TextMeshProUGUI keyText = slotUI.GetComponentInChildren<TextMeshProUGUI>();
            if (keyText != null)
            {
                keyText.text = (i + 1).ToString();
            }
            
            // 設定點擊事件
            Button slotButton = slotUI.GetComponent<Button>();
            if (slotButton != null)
            {
                int slotIndex = i;
                slotButton.onClick.AddListener(() => OnQuickSlotClicked(slotIndex));
            }
        }
    }
    

    // 設定按鈕事件監聽器
    private void SetupButtons()
    {
        if (useItemButton != null)
            useItemButton.onClick.AddListener(OnUseItemClicked);
        
        if (dropItemButton != null)
            dropItemButton.onClick.AddListener(OnDropItemClicked);
    }
    
    // 更新所有UI顯示
    public void UpdateUI()
    {
        UpdateInventorySlots();
        UpdateQuickSlots();
    }
    
    // 更新道具欄槽位顯示
    private void UpdateInventorySlots()
    {
        for (int i = 0; i < slotUIs.Count && i < inventory.slots.Count; i++)
        {
            UpdateSlotUI(slotUIs[i], inventory.slots[i]);
        }
    }
    
    // 更新快捷鍵顯示
    private void UpdateQuickSlots()
    {
        for (int i = 0; i < quickSlotUIs.Count && i < inventory.slots.Count; i++)
        {
            UpdateSlotUI(quickSlotUIs[i], inventory.slots[i]);
        }
    }
    
    // 更新單個槽位的UI顯示
    private void UpdateSlotUI(GameObject slotUI, ItemSlot slot)
    {
        if (slotUI == null) return;
        
        // 獲取UI組件引用
        Image iconImage = slotUI.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI quantityText = slotUI.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
        Image backgroundImage = slotUI.GetComponent<Image>();
        
        if (slot.IsEmpty)
        {
            // 空槽位的顯示設定
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.color = new Color(1, 1, 1, 0.3f); // 半透明
            }
            if (quantityText != null)
                quantityText.text = "";
            if (backgroundImage != null)
                backgroundImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 灰色背景
        }
        else
        {
            // 有道具的槽位顯示設定
            if (iconImage != null)
            {
                iconImage.sprite = slot.item.icon;
                iconImage.color = Color.white;
            }
            if (quantityText != null)
            {
                // 只有數量大於1時才顯示數量
                quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
            }
            if (backgroundImage != null)
                backgroundImage.color = Color.white;
        }
    }
    
    // 道具槽位被點擊時的處理
    private void OnSlotClicked(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.slots.Count) return;
        
        selectedSlot = inventory.slots[slotIndex];
        ShowItemDetail(selectedSlot);
    }
    
    // 快捷鍵被點擊時的處理
    private void OnQuickSlotClicked(int slotIndex)
    {
        inventory.UseItem(slotIndex);
        UpdateUI();
    }
    
    // 顯示道具詳情面板
    private void ShowItemDetail(ItemSlot slot)
    {
        if (itemDetailPanel == null) return;
        
        if (slot.IsEmpty)
        {
            // 如果槽位為空，隱藏詳情面板
            itemDetailPanel.SetActive(false);
            return;
        }
        
        // 顯示詳情面板
        itemDetailPanel.SetActive(true);
        
        // 更新詳情UI內容
        if (itemDetailIcon != null)
            itemDetailIcon.sprite = slot.item.icon;
        
        if (itemDetailName != null)
            itemDetailName.text = slot.item.itemName;
        
        if (itemDetailDescription != null)
            itemDetailDescription.text = slot.item.description;
        
        if (itemDetailQuantity != null)
            itemDetailQuantity.text = $"數量: {slot.quantity}";
        
        // 設定按鈕的可用狀態
        if (useItemButton != null)
            useItemButton.interactable = slot.item.isConsumable || !slot.IsEmpty;
        
        if (dropItemButton != null)
            dropItemButton.interactable = !slot.IsEmpty;
    }
    
    // 使用道具按鈕點擊處理
    private void OnUseItemClicked()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty) return;
        
        // 找到選中槽位的索引
        int slotIndex = inventory.slots.IndexOf(selectedSlot);
        if (slotIndex >= 0)
        {
            inventory.UseItem(slotIndex);
            UpdateUI();
            
            // 重新顯示詳情（如果道具還在）
            if (slotIndex < inventory.slots.Count)
            {
                ShowItemDetail(inventory.slots[slotIndex]);
            }
            else
            {
                itemDetailPanel.SetActive(false);
            }
        }
    }
    
    // 丟棄道具按鈕點擊處理
    private void OnDropItemClicked()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty) return;
        
        // 找到選中槽位的索引
        int slotIndex = inventory.slots.IndexOf(selectedSlot);
        if (slotIndex >= 0)
        {
            inventory.RemoveItem(slotIndex, 1);
            UpdateUI();
            
            // 重新顯示詳情
            if (slotIndex < inventory.slots.Count)
            {
                ShowItemDetail(inventory.slots[slotIndex]);
            }
            else
            {
                itemDetailPanel.SetActive(false);
            }
        }
    }
    
    // 切換道具欄顯示狀態
    public void ToggleInventory()
    {
        if (itemPanel != null)
        {
            bool newState = !itemPanel.activeSelf;
            itemPanel.SetActive(newState);
            
            // 如果關閉道具欄，也關閉詳情面板
            if (!newState && itemDetailPanel != null)
            {
                itemDetailPanel.SetActive(false);
            }
        }
    }
    
    // 顯示道具欄
    public void ShowInventory()
    {
        if (itemPanel != null)
            itemPanel.SetActive(true);
    }
    
    // 隱藏道具欄
    public void HideInventory()
    {
        if (itemPanel != null)
            itemPanel.SetActive(false);
        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);
    }
} 