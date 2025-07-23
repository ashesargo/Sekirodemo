using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 道具類型枚舉
public enum ItemType
{
    MedicinePill,    // 藥丸：緩慢恢復HP
    SteelSugar,      // 剛幹糖：減少敵人攻擊增長的架勢槽
    HealingGourd     // 傷藥葫蘆：回復一定HP
}

// 道具基礎類別
[System.Serializable]
public class Item
{
    public string itemName;
    public ItemType itemType;
    public int quantity;
    public float effectValue;
    public float duration;
    public Sprite itemIcon;
    public string description;
    
    public Item(string name, ItemType type, int qty, float value, float dur, string desc)
    {
        itemName = name;
        itemType = type;
        quantity = qty;
        effectValue = value;
        duration = dur;
        description = desc;
    }
}

// 道具系統主類別
public class ItemSystem : MonoBehaviour
{
    [Header("道具設定")]
    public List<Item> playerInventory = new List<Item>();
    
    [Header("道具效果參數")]
    public float medicinePillHealPerSecond = 5f;  // 藥丸每秒恢復量
    public float medicinePillDuration = 10f;      // 藥丸持續時間
    public float steelSugarPostureReduction = 0.5f; // 剛幹糖架勢減少倍率
    public float steelSugarDuration = 15f;        // 剛幹糖持續時間
    public int healingGourdHealAmount = 30;       // 傷藥葫蘆恢復量
    
    [Header("UI 參考")]
    public GameObject itemUseEffect;  // 道具使用特效
    public AudioClip itemUseSound;    // 道具使用音效
    
    private PlayerStatus playerStatus;
    private HealthPostureController healthController;
    private AudioSource audioSource;
    
    // 當前生效的狀態效果
    private bool isMedicinePillActive = false;
    private bool isSteelSugarActive = false;
    private Coroutine medicinePillCoroutine;
    private Coroutine steelSugarCoroutine;
    
    // 事件
    public System.Action<Item> OnItemUsed;
    public System.Action<Item> OnItemAdded;
    public System.Action<Item> OnItemRemoved;
    
    void Start()
    {
        playerStatus = GetComponent<PlayerStatus>();
        healthController = GetComponent<HealthPostureController>();
        audioSource = GetComponent<AudioSource>();
        
        // 如果沒有AudioSource，添加一個
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 初始化預設道具
        InitializeDefaultItems();
    }
    
    void Update()
    {
        // 道具使用快捷鍵
        HandleItemInput();
    }
    
    // 初始化預設道具
    void InitializeDefaultItems()
    {
        // 添加預設道具到背包
        AddItem(new Item("藥丸", ItemType.MedicinePill, 3, medicinePillHealPerSecond, medicinePillDuration, "緩慢恢復生命值"));
        AddItem(new Item("剛幹糖", ItemType.SteelSugar, 2, steelSugarPostureReduction, steelSugarDuration, "減少敵人攻擊增長的架勢槽"));
        AddItem(new Item("傷藥葫蘆", ItemType.HealingGourd, 5, healingGourdHealAmount, 0f, "立即恢復生命值"));
    }
    
    // 處理道具輸入
    void HandleItemInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) // 數字鍵1
        {
            UseItemByType(ItemType.MedicinePill);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // 數字鍵2
        {
            UseItemByType(ItemType.SteelSugar);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) // 數字鍵3
        {
            UseItemByType(ItemType.HealingGourd);
        }
    }
    
    // 根據類型使用道具
    public bool UseItemByType(ItemType itemType)
    {
        Item item = GetItemByType(itemType);
        if (item != null && item.quantity > 0)
        {
            return UseItem(item);
        }
        return false;
    }
    
    // 使用道具
    public bool UseItem(Item item)
    {
        if (item.quantity <= 0) return false;
        
        bool success = false;
        
        switch (item.itemType)
        {
            case ItemType.MedicinePill:
                success = UseMedicinePill(item);
                break;
            case ItemType.SteelSugar:
                success = UseSteelSugar(item);
                break;
            case ItemType.HealingGourd:
                success = UseHealingGourd(item);
                break;
        }
        
        if (success)
        {
            item.quantity--;
            PlayItemUseEffect();
            OnItemUsed?.Invoke(item);
            
            // 如果道具用完，從背包移除
            if (item.quantity <= 0)
            {
                playerInventory.Remove(item);
                OnItemRemoved?.Invoke(item);
            }
        }
        
        return success;
    }
    
    // 使用藥丸
    bool UseMedicinePill(Item item)
    {
        if (isMedicinePillActive) return false; // 如果已經在使用中，不能重複使用
        
        isMedicinePillActive = true;
        medicinePillCoroutine = StartCoroutine(MedicinePillEffect(item.effectValue, item.duration));
        return true;
    }
    
    // 使用剛幹糖
    bool UseSteelSugar(Item item)
    {
        if (isSteelSugarActive) return false; // 如果已經在使用中，不能重複使用
        
        isSteelSugarActive = true;
        steelSugarCoroutine = StartCoroutine(SteelSugarEffect(item.effectValue, item.duration));
        return true;
    }
    
    // 使用傷藥葫蘆
    bool UseHealingGourd(Item item)
    {
        if (healthController != null)
        {
            // 直接恢復生命值
            healthController.HealHealth((int)item.effectValue);
            return true;
        }
        return false;
    }
    
    // 藥丸效果協程
    IEnumerator MedicinePillEffect(float healPerSecond, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            if (healthController != null)
            {
                healthController.HealHealth((int)(healPerSecond * Time.deltaTime));
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        isMedicinePillActive = false;
    }
    
    // 剛幹糖效果協程
    IEnumerator SteelSugarEffect(float reductionRate, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        isSteelSugarActive = false;
    }
    
    // 獲取架勢減少倍率（供敵人攻擊時使用）
    public float GetPostureReductionRate()
    {
        return isSteelSugarActive ? steelSugarPostureReduction : 1f;
    }
    
    // 添加道具到背包
    public void AddItem(Item item)
    {
        // 檢查是否已有相同類型的道具
        Item existingItem = GetItemByType(item.itemType);
        if (existingItem != null)
        {
            existingItem.quantity += item.quantity;
        }
        else
        {
            playerInventory.Add(item);
        }
        
        OnItemAdded?.Invoke(item);
    }
    
    // 根據類型獲取道具
    public Item GetItemByType(ItemType itemType)
    {
        return playerInventory.Find(item => item.itemType == itemType);
    }
    
    // 獲取道具數量
    public int GetItemQuantity(ItemType itemType)
    {
        Item item = GetItemByType(itemType);
        return item != null ? item.quantity : 0;
    }
    
    // 播放道具使用特效
    void PlayItemUseEffect()
    {
        if (itemUseEffect != null)
        {
            Instantiate(itemUseEffect, transform.position, Quaternion.identity);
        }
        
        if (audioSource != null && itemUseSound != null)
        {
            audioSource.PlayOneShot(itemUseSound);
        }
    }
    
    // 停止所有效果（用於死亡或場景切換時）
    public void StopAllEffects()
    {
        if (medicinePillCoroutine != null)
        {
            StopCoroutine(medicinePillCoroutine);
            isMedicinePillActive = false;
        }
        
        if (steelSugarCoroutine != null)
        {
            StopCoroutine(steelSugarCoroutine);
            isSteelSugarActive = false;
        }
    }
} 