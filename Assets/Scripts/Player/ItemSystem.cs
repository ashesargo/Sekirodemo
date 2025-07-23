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

// 隻狼風格道具系統
public class ItemSystem : MonoBehaviour
{
    [Header("道具設定")]
    public List<Item> playerInventory = new List<Item>();
    
    [Header("道具效果參數")]
    public float medicinePillHealPerSecond = 5f;
    public float medicinePillDuration = 10f;
    public float steelSugarPostureReduction = 0.5f;
    public float steelSugarDuration = 15f;
    public int healingGourdHealAmount = 30;
    
    [Header("隻狼風格設定")]
    public float itemUseAnimationTime = 1.5f;
    public bool canUseItemWhileMoving = false;
    public bool canUseItemWhileAttacking = false;
    
    [Header("UI 參考")]
    public GameObject itemUseEffect;
    public AudioClip itemUseSound;
    
    // 組件引用
    private PlayerStatus playerStatus;
    private HealthPostureController healthController;
    private AudioSource audioSource;
    private Animator _animator;
    
    // 狀態管理
    private int currentItemIndex = 0;
    private bool isMedicinePillActive = false;
    private bool isSteelSugarActive = false;
    private bool isUsingItem = false;
    
    // 協程
    private Coroutine medicinePillCoroutine;
    private Coroutine steelSugarCoroutine;
    private Coroutine itemUseCoroutine;
    
    // 事件
    public System.Action<Item> OnItemUsed;
    public System.Action<Item> OnItemAdded;
    public System.Action<Item> OnItemRemoved;
    public System.Action<Item> OnItemSwitched;
    
    void Start()
    {
        // 獲取組件
        playerStatus = GetComponent<PlayerStatus>();
        healthController = GetComponent<HealthPostureController>();
        audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        
        // 添加AudioSource如果沒有
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 初始化預設道具
        InitializeDefaultItems();
    }
    
    void Update()
    {
        HandleItemInput();
    }
    
    // 初始化預設道具
    void InitializeDefaultItems()
    {
        AddItem(new Item("藥丸", ItemType.MedicinePill, 3, medicinePillHealPerSecond, medicinePillDuration, "緩慢恢復生命值"));
        AddItem(new Item("剛幹糖", ItemType.SteelSugar, 2, steelSugarPostureReduction, steelSugarDuration, "減少敵人攻擊增長的架勢槽"));
        AddItem(new Item("傷藥葫蘆", ItemType.HealingGourd, 5, healingGourdHealAmount, 0f, "立即恢復生命值"));
    }
    
    // 處理道具輸入
    void HandleItemInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SwitchToNextItem();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseCurrentSelectedItem();
        }
    }
    
    // 切換道具
    void SwitchToNextItem()
    {
        if (playerInventory.Count == 0) return;
        
        currentItemIndex = (currentItemIndex + 1) % playerInventory.Count;
        OnItemSwitched?.Invoke(GetCurrentSelectedItem());
    }
    
    // 使用當前選中的道具
    void UseCurrentSelectedItem()
    {
        if (playerInventory.Count == 0 || !CanUseItem()) return;
        
        Item currentItem = GetCurrentSelectedItem();
        if (currentItem != null)
        {
            UseItem(currentItem);
        }
    }
    
    // 檢查是否可以使用道具
    bool CanUseItem()
    {
        if (isUsingItem) return false;
        
        // 檢查移動和攻擊狀態（可根據需要調整）
        return true;
    }
    
    // 獲取當前選中的道具
    public Item GetCurrentSelectedItem()
    {
        if (playerInventory.Count == 0) return null;
        
        if (currentItemIndex >= playerInventory.Count)
        {
            currentItemIndex = 0;
        }
        
        return playerInventory[currentItemIndex];
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
            StartItemUseAnimation();
            PlayItemUseEffect();
            OnItemUsed?.Invoke(item);
            
            // 處理道具用完的情況
            if (item.quantity <= 0)
            {
                HandleItemDepleted(item);
            }
        }
        
        return success;
    }
    
    // 處理道具用完
    void HandleItemDepleted(Item item)
    {
        int removedIndex = playerInventory.IndexOf(item);
        playerInventory.Remove(item);
        OnItemRemoved?.Invoke(item);
        
        // 調整選中索引
        if (playerInventory.Count == 0)
        {
            currentItemIndex = 0;
        }
        else if (removedIndex <= currentItemIndex && currentItemIndex > 0)
        {
            currentItemIndex--;
        }
        
        if (currentItemIndex >= playerInventory.Count)
        {
            currentItemIndex = 0;
        }
    }
    
    // 使用藥丸
    bool UseMedicinePill(Item item)
    {
        if (isMedicinePillActive) return false;
        
        isMedicinePillActive = true;
        medicinePillCoroutine = StartCoroutine(MedicinePillEffect(item.effectValue, item.duration));
        return true;
    }
    
    // 使用剛幹糖
    bool UseSteelSugar(Item item)
    {
        if (isSteelSugarActive) return false;
        
        isSteelSugarActive = true;
        steelSugarCoroutine = StartCoroutine(SteelSugarEffect(item.effectValue, item.duration));
        return true;
    }
    
    // 使用傷藥葫蘆
    bool UseHealingGourd(Item item)
    {
        if (healthController != null)
        {
            healthController.HealHealth((int)item.effectValue);
            return true;
        }
        return false;
    }
    
    // 藥丸效果
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
    
    // 剛幹糖效果
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
    
    // 開始使用動畫
    void StartItemUseAnimation()
    {
        if (isUsingItem) return;
        
        isUsingItem = true;
        itemUseCoroutine = StartCoroutine(ItemUseAnimation());
    }
    
    // 使用動畫協程
    IEnumerator ItemUseAnimation()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("UseItem");
        }
        
        yield return new WaitForSeconds(itemUseAnimationTime);
        isUsingItem = false;
    }
    
    // 播放使用效果
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
    
    // 獲取架勢減少倍率
    public float GetPostureReductionRate()
    {
        return isSteelSugarActive ? steelSugarPostureReduction : 1f;
    }
    
    // 添加道具
    public void AddItem(Item item)
    {
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
    
    // 獲取當前選中索引
    public int GetCurrentItemIndex()
    {
        return currentItemIndex;
    }
    
    // 停止所有效果
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
        
        if (itemUseCoroutine != null)
        {
            StopCoroutine(itemUseCoroutine);
            isUsingItem = false;
        }
    }
} 