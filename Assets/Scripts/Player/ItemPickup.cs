using UnityEngine;

// 道具拾取類別
public class ItemPickup : MonoBehaviour
{
    [Header("道具設定")]
    public ItemType itemType;
    public int itemQuantity = 1;
    public float effectValue = 0f;
    public float duration = 0f;
    
    [Header("拾取設定")]
    public float pickupRange = 2f;
    public LayerMask playerLayer = 1;
    public bool destroyOnPickup = true;
    public bool respawnable = false;
    public float respawnTime = 30f;
    
    [Header("視覺效果")]
    public GameObject pickupEffect;
    public AudioClip pickupSound;
    public float rotationSpeed = 50f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    
    [Header("UI 提示")]
    public GameObject pickupPrompt;
    public string promptText = "按 E 拾取";
    
    private ItemSystem playerItemSystem;
    private Transform playerTransform;
    private bool isPlayerInRange = false;
    private bool isPickedUp = false;
    private Vector3 originalPosition;
    private AudioSource audioSource;
    
    void Start()
    {
        originalPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        
        // 如果沒有AudioSource，添加一個
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 隱藏拾取提示
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
    }
    
    void Update()
    {
        if (isPickedUp) return;
        
        // 旋轉和浮動效果
        UpdateVisualEffects();
        
        // 檢查玩家是否在拾取範圍內
        CheckPlayerInRange();
        
        // 處理拾取輸入
        HandlePickupInput();
    }
    
    // 更新視覺效果
    void UpdateVisualEffects()
    {
        // 旋轉效果
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // 浮動效果
        float newY = originalPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    // 檢查玩家是否在拾取範圍內
    void CheckPlayerInRange()
    {
        if (playerTransform == null)
        {
            // 尋找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerItemSystem = player.GetComponent<ItemSystem>();
            }
        }
        
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool wasInRange = isPlayerInRange;
            isPlayerInRange = distance <= pickupRange;
            
            // 如果狀態改變，更新UI提示
            if (wasInRange != isPlayerInRange)
            {
                UpdatePickupPrompt();
            }
        }
    }
    
    // 處理拾取輸入
    void HandlePickupInput()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            PickupItem();
        }
    }
    
    // 拾取道具
    void PickupItem()
    {
        if (isPickedUp || playerItemSystem == null) return;
        
        // 創建道具
        Item item = new Item(GetItemName(), itemType, itemQuantity, effectValue, duration, GetItemDescription());
        
        // 添加到玩家背包
        playerItemSystem.AddItem(item);
        
        // 播放拾取效果
        PlayPickupEffects();
        
        // 標記為已拾取
        isPickedUp = true;
        
        // 隱藏拾取提示
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
        
        // 處理拾取後的行為
        HandlePostPickup();
    }
    
    // 播放拾取效果
    void PlayPickupEffects()
    {
        // 播放音效
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        // 播放特效
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
    }
    
    // 處理拾取後的行為
    void HandlePostPickup()
    {
        if (destroyOnPickup)
        {
            if (respawnable)
            {
                // 延遲重生
                Invoke(nameof(RespawnItem), respawnTime);
            }
            else
            {
                // 直接銷毀
                Destroy(gameObject);
            }
        }
        else
        {
            // 隱藏物件但不銷毀
            gameObject.SetActive(false);
            
            if (respawnable)
            {
                Invoke(nameof(RespawnItem), respawnTime);
            }
        }
    }
    
    // 重生道具
    void RespawnItem()
    {
        isPickedUp = false;
        gameObject.SetActive(true);
        transform.position = originalPosition;
    }
    
    // 更新拾取提示
    void UpdatePickupPrompt()
    {
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(isPlayerInRange);
        }
    }
    
    // 獲取道具名稱
    string GetItemName()
    {
        switch (itemType)
        {
            case ItemType.MedicinePill:
                return "藥丸";
            case ItemType.SteelSugar:
                return "剛幹糖";
            case ItemType.HealingGourd:
                return "傷藥葫蘆";
            default:
                return "未知道具";
        }
    }
    
    // 獲取道具描述
    string GetItemDescription()
    {
        switch (itemType)
        {
            case ItemType.MedicinePill:
                return "緩慢恢復生命值";
            case ItemType.SteelSugar:
                return "減少敵人攻擊增長的架勢槽";
            case ItemType.HealingGourd:
                return "立即恢復生命值";
            default:
                return "";
        }
    }
    
    // 在Scene視圖中顯示拾取範圍
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
    
    // 設定道具類型
    public void SetItemType(ItemType type)
    {
        itemType = type;
    }
    
    // 設定道具數量
    public void SetItemQuantity(int quantity)
    {
        itemQuantity = quantity;
    }
    
    // 設定效果值
    public void SetEffectValue(float value)
    {
        effectValue = value;
    }
    
    // 設定持續時間
    public void SetDuration(float dur)
    {
        duration = dur;
    }
} 