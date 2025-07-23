using System.Collections.Generic;
using UnityEngine;

// 隻狼風格道具管理器
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
    
    // 私有變數
    private float spawnTimer = 0f;
    private List<GameObject> spawnedItems = new List<GameObject>();
    
    void Start()
    {
        InitializeDefaultItemConfigs();
        
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
            itemConfigs.Add(new ItemConfig
            {
                itemType = ItemType.MedicinePill,
                itemName = "藥丸",
                effectValue = 5f,
                duration = 10f,
                description = "緩慢恢復生命值，持續10秒"
            });
            
            itemConfigs.Add(new ItemConfig
            {
                itemType = ItemType.SteelSugar,
                effectValue = 0.5f,
                duration = 15f,
                description = "減少敵人攻擊增長的架勢槽，持續15秒"
            });
            
            itemConfigs.Add(new ItemConfig
            {
                itemType = ItemType.HealingGourd,
                itemName = "傷藥葫蘆",
                effectValue = 30f,
                duration = 0f,
                description = "立即恢復30點生命值"
            });
        }
    }
    
    // 生成初始道具
    void SpawnInitialItems()
    {
        if (itemSpawnPoints == null || itemSpawnPoints.Length == 0) return;
        
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
            CleanupSpawnedItems();
            
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
        
        ItemType randomType = (ItemType)Random.Range(0, System.Enum.GetValues(typeof(ItemType)).Length);
        return SpawnItem(randomType, position, 1);
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
        
        AddDefaultItem(defaultItems, ItemType.MedicinePill, defaultMedicinePillCount);
        AddDefaultItem(defaultItems, ItemType.SteelSugar, defaultSteelSugarCount);
        AddDefaultItem(defaultItems, ItemType.HealingGourd, defaultHealingGourdCount);
        
        return defaultItems;
    }
    
    // 添加預設道具
    void AddDefaultItem(List<Item> defaultItems, ItemType itemType, int count)
    {
        ItemConfig config = GetItemConfig(itemType);
        if (config != null)
        {
            defaultItems.Add(new Item(
                config.itemName,
                itemType,
                count,
                config.effectValue,
                config.duration,
                config.description
            ));
        }
    }
    
    // 公共方法
    public void SetSpawnPoints(Transform[] spawnPoints) => itemSpawnPoints = spawnPoints;
    
    public void AddSpawnPoint(Transform spawnPoint)
    {
        if (itemSpawnPoints == null)
        {
            itemSpawnPoints = new Transform[0];
        }
        
        System.Array.Resize(ref itemSpawnPoints, itemSpawnPoints.Length + 1);
        itemSpawnPoints[itemSpawnPoints.Length - 1] = spawnPoint;
    }
    
    public void RemoveSpawnPoint(Transform spawnPoint)
    {
        if (itemSpawnPoints == null) return;
        
        List<Transform> newSpawnPoints = new List<Transform>(itemSpawnPoints);
        newSpawnPoints.Remove(spawnPoint);
        itemSpawnPoints = newSpawnPoints.ToArray();
    }
    
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
    
    public void PauseItemSpawning() => autoSpawnItems = false;
    public void ResumeItemSpawning() => autoSpawnItems = true;
} 