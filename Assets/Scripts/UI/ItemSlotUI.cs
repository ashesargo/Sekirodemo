using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 道具槽UI
public class ItemSlotUI : MonoBehaviour
{
    [Header("UI 組件")]
    public Image icon;
    public TMP_Text quantityText; // 數量文字 (TextMeshPro)
    public TMP_Text hotkeyText; // 快捷鍵文字 (TextMeshPro)
    public Image background;
    public Image cooldownOverlay;
    
    [Header("顏色")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.green;
    public Color emptyColor = Color.gray;
    
    private ItemSystem.ItemData itemData;
    private bool isSelected = false;
    private bool isOnCooldown = false;
    private float cooldownTime = 0f;
    private float maxCooldownTime = 0f;
    
    public void Initialize(ItemSystem.ItemData data, bool selected)
    {
        Debug.Log($"[ItemSlotUI] 初始化道具槽: {data.name} (選中: {selected})");
        itemData = data;
        isSelected = selected;
        
        if (icon != null && data.icon != null)
        {
            icon.sprite = data.icon;
            Debug.Log($"[ItemSlotUI] 設置道具圖示: {data.name}");
        }
        else
        {
            Debug.LogWarning($"[ItemSlotUI] 道具圖示缺失: {data.name}");
        }
        
        if (hotkeyText != null)
        {
            hotkeyText.text = "E";
        }
        
        UpdateQuantity(data.quantity);
        SetSelected(selected);
    }
    
    public void UpdateQuantity(int quantity)
    {
        if (quantityText != null)
        {
            quantityText.text = quantity.ToString();
        }
        
        UpdateColor(quantity);
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateColor(itemData?.quantity ?? 0);
    }
    
    void UpdateColor(int quantity)
    {
        if (background == null) return;
        
        if (quantity <= 0)
        {
            background.color = emptyColor;
        }
        else if (isSelected)
        {
            background.color = selectedColor;
        }
        else
        {
            background.color = normalColor;
        }
    }
    
    public void StartCooldown(float duration)
    {
        Debug.Log($"[ItemSlotUI] 開始冷卻: {itemData?.name} ({duration}秒)");
        isOnCooldown = true;
        cooldownTime = duration;
        maxCooldownTime = duration;
        
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[ItemSlotUI] cooldownOverlay為null，無法顯示冷卻效果");
        }
    }
    
    void Update()
    {
        if (isOnCooldown)
        {
            cooldownTime -= Time.deltaTime;
            
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = cooldownTime / maxCooldownTime;
            }
            
            if (cooldownTime <= 0f)
            {
                isOnCooldown = false;
                if (cooldownOverlay != null)
                {
                    cooldownOverlay.gameObject.SetActive(false);
                }
            }
        }
    }
    
    public bool IsOnCooldown() => isOnCooldown;
} 