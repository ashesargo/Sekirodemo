using UnityEngine;
using System.Collections.Generic;

// 道具管理器
public class ItemManager : MonoBehaviour
{
    [Header("道具 Prefab")]
    public HealingGourd healingGourdPrefab; // 治療葫蘆 Prefab
    public Sugar sugarPrefab;   // 糖類 Prefab
    public DivineConfetti divineConfettiPrefab; // 神隱糖 Prefab
    public SpiritEmblem spiritEmblemPrefab; // 紙人偶 Prefab
    
    [Header("測試用設定")]
    public bool addTestItemsOnStart = true;        // 是否在開始時添加測試道具
    public int testItemQuantity = 5;               // 測試道具數量
    
    [Header("UI 組件")]
    public InventoryUI inventoryUI;                // 道具欄 UI
    
    private Inventory inventory;                   // 道具系統
    private PlayerStats playerStats;               // 玩家狀態
    
    // 初始化道具管理器
    private void Start()
    {
        // 獲取必要的組件
        inventory = FindObjectOfType<Inventory>();
        playerStats = FindObjectOfType<PlayerStats>();
        
        // 檢查組件是否存在
        if (inventory == null)
        {   
            Debug.LogError("沒有狗屁包包！");
            return;
        }
        
        if (playerStats == null)
        {
            Debug.LogError("沒有狗屁玩家！");
            return;
        }
        
        // 如果設定為自動添加測試道具，則執行
        if (addTestItemsOnStart)
        {
            AddTestItems();
        }
        
        // 更新 UI
        if (inventoryUI != null)
        {
            inventoryUI.UpdateUI();
        }
    }
    
    // 添加測試道具到道具欄
    private void AddTestItems()
    {
        Debug.Log("添加測試道具");
        
        // 添加治療葫蘆
        if (healingGourdPrefab != null)
        {
            inventory.AddItem(healingGourdPrefab, testItemQuantity);
            Debug.Log($"添加了 {testItemQuantity} 個治療葫蘆");
        }
        
        // 添加糖類
        if (sugarPrefab != null)
        {
            inventory.AddItem(sugarPrefab, testItemQuantity);
            Debug.Log($"添加了 {testItemQuantity} 個糖類");
        }
        
        // 添加神隱糖
        if (divineConfettiPrefab != null)
        {
            inventory.AddItem(divineConfettiPrefab, testItemQuantity);
            Debug.Log($"添加了 {testItemQuantity} 個神隱糖");
        }
        
        // 添加紙人偶
        if (spiritEmblemPrefab != null)
        {
            inventory.AddItem(spiritEmblemPrefab, testItemQuantity);
            Debug.Log($"添加了 {testItemQuantity} 個紙人偶");
        }
    }
    
    // 添加道具到道具欄
    public void AddItem(ItemData item, int quantity = 1)
    {
        if (inventory != null)
        {
            bool success = inventory.AddItem(item, quantity);
            if (success)
            {
                Debug.Log($"成功添加 {quantity} 個 {item.itemName}");
                
                // 更新 UI
                if (inventoryUI != null)
                {
                    inventoryUI.UpdateUI();
                }
            }
            else
            {
                Debug.LogWarning($"無法添加 {item.itemName}，道具欄可能已滿");
            }
        }
    }
    
    // 從道具欄移除道具
    public void RemoveItem(ItemData item, int quantity = 1)
    {
        if (inventory != null)
        {
            // 找到道具所在的槽位
            for (int i = 0; i < inventory.slots.Count; i++)
            {
                if (inventory.slots[i].item == item)
                {
                    inventory.RemoveItem(i, quantity);
                    Debug.Log($"移除了 {quantity} 個 {item.itemName}");
                    
                    // 更新 UI
                    if (inventoryUI != null)
                    {
                        inventoryUI.UpdateUI();
                    }
                    return;
                }
            }
            Debug.LogWarning($"找不到 {item.itemName} 在道具欄中");
        }
    }
    
    // 檢查是否擁有指定數量的道具
    public bool HasItem(ItemData item, int quantity = 1)
    {
        if (inventory == null) return false;
        
        int totalQuantity = 0;
        foreach (var slot in inventory.slots)
        {
            if (slot.item == item)
            {
                totalQuantity += slot.quantity;
            }
        }
        
        return totalQuantity >= quantity;
    }
    
    // 獲取指定道具的總數量
    public int GetItemQuantity(ItemData item)
    {
        if (inventory == null) return 0;
        
        int totalQuantity = 0;
        foreach (var slot in inventory.slots)
        {
            if (slot.item == item)
            {
                totalQuantity += slot.quantity;
            }
        }
        
        return totalQuantity;
    }
    
    // 清空道具欄
    public void ClearInventory()
    {
        if (inventory != null)
        {
            inventory.slots.Clear();
            inventory.InitializeSlots();
            
            Debug.Log("道具欄已清空");
            
            // 更新 UI
            if (inventoryUI != null)
            {
                inventoryUI.UpdateUI();
            }
        }
    }
    
    // 重置測試道具（清空後重新添加）
    public void ResetTestItems()
    {
        ClearInventory();
        AddTestItems();
    }
    
    // 測試：對玩家造成傷害
    public void TestTakeDamage(int damage = 10)
    {
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
            Debug.Log($"測試受到 {damage} 點傷害");
        }
    }
    
    // 測試：增加玩家架勢值
    public void TestIncreasePosture(int amount = 10)
    {
        if (playerStats != null)
        {
            playerStats.IncreasePosture(amount);
            Debug.Log($"測試增加 {amount} 點架勢值");
        }
    }
    
    // 測試：減少玩家架勢值
    public void TestDecreasePosture(int amount = 10)
    {
        if (playerStats != null)
        {
            playerStats.DecreasePosture(amount);
            Debug.Log($"測試減少 {amount} 點架勢值");
        }
    }
    
    // 獲取當前生命值百分比
    public float GetHealthPercentage()
    {
        if (playerStats != null && playerStats.healthPostureSystem != null)
        {
            return playerStats.healthPostureSystem.GetHealthNormalized() * 100f;
        }
        return 0f;
    }
    
    // 獲取當前架勢值百分比
    public float GetPosturePercentage()
    {
        if (playerStats != null && playerStats.healthPostureSystem != null)
        {
            return playerStats.healthPostureSystem.GetPostureNormalized() * 100f;
        }
        return 0f;
    }
    
    // 顯示道具欄 UI
    public void ShowInventory()
    {
        if (inventoryUI != null)
        {
            inventoryUI.ShowInventory();
        }
    }
    
    // 隱藏道具欄 UI
    public void HideInventory()
    {
        if (inventoryUI != null)
        {
            inventoryUI.HideInventory();
        }
    }
    
    // 切換道具欄 UI 顯示狀態
    public void ToggleInventory()
    {
        if (inventoryUI != null)
        {
            inventoryUI.ToggleInventory();
        }
    }
} 