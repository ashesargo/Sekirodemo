using System.Collections.Generic;
using UnityEngine;

// 道具管理器 - 管理道具的生成和配置
public class ItemManager : MonoBehaviour
{
    [System.Serializable]
    public class ItemConfig
    {
        public ItemType itemType;
        public string itemName;
        public float effectValue;
        public float duration;
        public string description;
        public Sprite itemIcon;
        public GameObject itemPrefab;
        public AudioClip useSound;
        public GameObject useEffect;
    }
    
    [Header("道具配置")]
    public List<ItemConfig> itemConfigs = new List<ItemConfig>();
    
    [Header("道具生成設定")]
    public GameObject itemPickupPrefab;
    public Transform[] itemSpawnPoints;
    public bool autoSpawnItems = true;
    public float spawnInterval = 60f;
    
    [Header("預設道具設定")]
    public int defaultMedicinePillCount = 3;
    public int defaultSteelSugarCount = 2;
    public int defaultHealingGourdCount = 5;
    
    private float spawnTimer = 0f;
    private List<GameObject> spawnedItems = new List<GameObject>();
    
    void Start()
    {
        // 初始化預設道具配置
        InitializeDefaultItemConfigs();
        
        // 如果啟用自動生成，開始生成道具
        if (autoSpawnItems)
        {
            SpawnInitialItems();
        }
    }
    
    void Update()
    {
        if (autoSpawnItems)
        {
            UpdateItemSpawning();
        }
    }
    
    // 初始化預設道具配置
    void InitializeDefaultItemConfigs()
    {
        if (itemConfigs.Count == 0)
        {
            // 藥丸配置
            ItemConfig medicinePill = new ItemConfig
            {
                itemType = ItemType.MedicinePill,
                itemName = "藥丸",
                effectValue = 5f,
                duration = 10f,
                description = "緩慢恢復生命值，持續10秒"
            };
            itemConfigs.Add(medicinePill);
            
            // 剛幹糖配置
            ItemConfig steelSugar = new ItemConfig
            {
                itemType = ItemType.SteelSugar,
                effectValue = 0.5f,
                duration = 15f,
                description = "減少敵人攻擊增長的架勢槽，持續15秒"
            };
            itemConfigs.Add(steelSugar);
            
            // 傷藥葫蘆配置
            ItemConfig healingGourd = new ItemConfig
            {
                itemType = ItemType.HealingGourd,
                itemName = "傷藥葫蘆",
                effectValue = 30f,
                duration = 0f,
                description = "立即恢復30點生命值"
            };
            itemConfigs.Add(healingGourd);
        }
    }
    
    // 生成初始道具
    void SpawnInitialItems()
    {
        if (itemSpawnPoints == null || itemSpawnPoints.Length == 0) return;
        
        // 在每個生成點隨機生成道具
        foreach (Transform spawnPoint in itemSpawnPoints)
        {
            if (spawnPoint != null)
            {
                SpawnRandomItem(spawnPoint.position);
            }
        }
    }
    
    // 更新道具生成
    void UpdateItemSpawning()
    {
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            
            // 清理已拾取的道具
            CleanupSpawnedItems();
            
            // 生成新道具
            if (itemSpawnPoints != null && itemSpawnPoints.Length > 0)
            {
                Transform randomSpawnPoint = itemSpawnPoints[Random.Range(0, itemSpawnPoints.Length)];
                if (randomSpawnPoint != null)
                {
                    SpawnRandomItem(randomSpawnPoint.position);
                }
            }
        }
    }
    
    // 生成隨機道具
    public GameObject SpawnRandomItem(Vector3 position)
    {
        if (itemPickupPrefab == null) return null;
        
        // 隨機選擇道具類型
        ItemType randomType = (ItemType)Random.Range(0, System.Enum.GetValues(typeof(ItemType)).Length);
        
        // 生成道具
        GameObject itemObj = Instantiate(itemPickupPrefab, position, Quaternion.identity);
        ItemPickup itemPickup = itemObj.GetComponent<ItemPickup>();
        
        if (itemPickup != null)
        {
            // 設定道具屬性
            ItemConfig config = GetItemConfig(randomType);
            if (config != null)
            {
                itemPickup.SetItemType(randomType);
                itemPickup.SetItemQuantity(1);
                itemPickup.SetEffectValue(config.effectValue);
                itemPickup.SetDuration(config.duration);
            }
        }
        
        spawnedItems.Add(itemObj);
        return itemObj;
    }
    
    // 生成指定類型的道具
    public GameObject SpawnItem(ItemType itemType, Vector3 position, int quantity = 1)
    {
        if (itemPickupPrefab == null) return null;
        
        GameObject itemObj = Instantiate(itemPickupPrefab, position, Quaternion.identity);
        ItemPickup itemPickup = itemObj.GetComponent<ItemPickup>();
        
        if (itemPickup != null)
        {
            ItemConfig config = GetItemConfig(itemType);
            if (config != null)
            {
                itemPickup.SetItemType(itemType);
                itemPickup.SetItemQuantity(quantity);
                itemPickup.SetEffectValue(config.effectValue);
                itemPickup.SetDuration(config.duration);
            }
        }
        
        spawnedItems.Add(itemObj);
        return itemObj;
    }
    
    // 獲取道具配置
    public ItemConfig GetItemConfig(ItemType itemType)
    {
        return itemConfigs.Find(config => config.itemType == itemType);
    }
    
    // 清理已拾取的道具
    void CleanupSpawnedItems()
    {
        spawnedItems.RemoveAll(item => item == null);
    }
    
    // 獲取預設道具列表
    public List<Item> GetDefaultItems()
    {
        List<Item> defaultItems = new List<Item>();
        
        // 添加藥丸
        ItemConfig medicinePillConfig = GetItemConfig(ItemType.MedicinePill);
        if (medicinePillConfig != null)
        {
            defaultItems.Add(new Item(
                medicinePillConfig.itemName,
                ItemType.MedicinePill,
                defaultMedicinePillCount,
                medicinePillConfig.effectValue,
                medicinePillConfig.duration,
                medicinePillConfig.description
            ));
        }
        
        // 添加剛幹糖
        ItemConfig steelSugarConfig = GetItemConfig(ItemType.SteelSugar);
        if (steelSugarConfig != null)
        {
            defaultItems.Add(new Item(
                steelSugarConfig.itemName,
                ItemType.SteelSugar,
                defaultSteelSugarCount,
                steelSugarConfig.effectValue,
                steelSugarConfig.duration,
                steelSugarConfig.description
            ));
        }
        
        // 添加傷藥葫蘆
        ItemConfig healingGourdConfig = GetItemConfig(ItemType.HealingGourd);
        if (healingGourdConfig != null)
        {
            defaultItems.Add(new Item(
                healingGourdConfig.itemName,
                ItemType.HealingGourd,
                defaultHealingGourdCount,
                healingGourdConfig.effectValue,
                healingGourdConfig.duration,
                healingGourdConfig.description
            ));
        }
        
        return defaultItems;
    }
    
    // 設定道具生成點
    public void SetSpawnPoints(Transform[] spawnPoints)
    {
        itemSpawnPoints = spawnPoints;
    }
    
    // 添加道具生成點
    public void AddSpawnPoint(Transform spawnPoint)
    {
        if (itemSpawnPoints == null)
        {
            itemSpawnPoints = new Transform[0];
        }
        
        System.Array.Resize(ref itemSpawnPoints, itemSpawnPoints.Length + 1);
        itemSpawnPoints[itemSpawnPoints.Length - 1] = spawnPoint;
    }
    
    // 移除道具生成點
    public void RemoveSpawnPoint(Transform spawnPoint)
    {
        if (itemSpawnPoints == null) return;
        
        List<Transform> newSpawnPoints = new List<Transform>(itemSpawnPoints);
        newSpawnPoints.Remove(spawnPoint);
        itemSpawnPoints = newSpawnPoints.ToArray();
    }
    
    // 清理所有生成的道具
    public void ClearAllSpawnedItems()
    {
        foreach (GameObject item in spawnedItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        spawnedItems.Clear();
    }
    
    // 暫停道具生成
    public void PauseItemSpawning()
    {
        autoSpawnItems = false;
    }
    
    // 恢復道具生成
    public void ResumeItemSpawning()
    {
        autoSpawnItems = true;
    }
} 