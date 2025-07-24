using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        [Tooltip("敵人預製體：拖拽敵人預製體到此欄位")]
        public GameObject enemyPrefab;
        
        [Tooltip("生成點陣列：設定該類型敵人的所有可能生成位置")]
        public Transform[] spawnPoints;
        
        [Tooltip("物件池大小：預先創建的該類型敵人數量，建議設定為實際需要的最大數量")]
        public int poolSize = 5;
        
        [Tooltip("最大生成數量：1=按poolSize生成, -1=不限制, 0=不生成, 其他值=報錯")]
        public int maxSpawnCount = -1;
    }
    
    [Header("多敵人預製體設定")]
    [Tooltip("敵人預製體和生成點陣列：每種敵人類型的詳細設定")]
    public EnemySpawnData[] enemySpawnData;
    
    [Header("物件池設定")]
    [Tooltip("物件池父物件：所有池化敵人的父物件，建議設為空物件")]
    public Transform poolParent;
    
    [Header("生成器設定")]
    [Tooltip("玩家位置：用於計算隨機生成位置")]
    public Transform playerTransform;
    
    [Tooltip("生成範圍：隨機生成時與玩家的距離")]
    public float spawnRadius = 25f;
    
    [Tooltip("生成間隔：每次生成敵人的時間間隔（秒）")]
    public float spawnInterval = 2f;
    
    [Tooltip("最大敵人數：同時存在的最大敵人數")]
    public int maxEnemies = 15;
    
    [Tooltip("是否重新生成死亡的敵人：建議設為 false")]
    public bool respawnDeadEnemies = false;
    
    [Header("生成模式")]
    [Tooltip("是否使用固定生成點：啟用時使用預設的生成點位置")]
    public bool useFixedSpawnPoints = true;
    
    [Tooltip("是否使用隨機生成：啟用時在玩家周圍隨機生成")]
    public bool useRandomSpawn = false;
    
    [Tooltip("是否隨機選擇生成點：啟用時隨機選擇預設生成點")]
    public bool useRandomSpawnPoint = false;
    
    private Dictionary<GameObject, List<GameObject>> enemyPools;
    private Dictionary<GameObject, Transform[]> enemySpawnPoints;
    private int currentEnemyCount = 0;
    private int currentSpawnIndex = 0;
    private Dictionary<GameObject, int> spawnPointIndices;
    private Dictionary<GameObject, int> spawnedCounts;
    
    void Awake()
    {
        SetupEnemyPools();
        StartCoroutine(CustomSpawnEnemies());
    }
    
    // 初始化敵人池
    private void SetupEnemyPools()
    {
        enemyPools = new Dictionary<GameObject, List<GameObject>>();
        enemySpawnPoints = new Dictionary<GameObject, Transform[]>();
        spawnPointIndices = new Dictionary<GameObject, int>();
        spawnedCounts = new Dictionary<GameObject, int>();
        
        if (enemySpawnData == null || enemySpawnData.Length == 0) return;
        
        foreach (EnemySpawnData data in enemySpawnData)
        {
            if (data.enemyPrefab == null) continue;
            
            // 驗證 maxSpawnCount 設定
            if (data.maxSpawnCount != -1 && data.maxSpawnCount != 0 && data.maxSpawnCount != 1) continue;
            
            // 創建物件池列表
            List<GameObject> pool = new List<GameObject>();
            
            // 預先創建物件
            for (int i = 0; i < data.poolSize; i++)
            {
                GameObject enemy = Instantiate(data.enemyPrefab, poolParent);
                enemy.SetActive(false);
                pool.Add(enemy);
            }
            
            // 儲存物件池和生成點
            enemyPools[data.enemyPrefab] = pool;
            if (data.spawnPoints != null && data.spawnPoints.Length > 0)
            {
                enemySpawnPoints[data.enemyPrefab] = data.spawnPoints;
                spawnPointIndices[data.enemyPrefab] = 0;
            }
            spawnedCounts[data.enemyPrefab] = 0;
        }
    }
    
    // 自定義生成敵人
    private IEnumerator CustomSpawnEnemies()
    {
        yield return null;
        
        while (true)
        {
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
                currentEnemyCount++;
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    // 生成敵人
    private void SpawnEnemy()
    {
        if (enemySpawnData == null || enemySpawnData.Length == 0) return;
        
        // 選擇要生成的敵人預製體
        GameObject selectedPrefab = SelectEnemyPrefab();
        if (selectedPrefab == null) return;
        
        // 取得對應的物件池
        if (!enemyPools.ContainsKey(selectedPrefab)) return;
        
        List<GameObject> pool = enemyPools[selectedPrefab];
        GameObject enemy = GetPooledEnemy(pool, selectedPrefab);
        
        if (enemy != null)
        {
            // 取得生成點資訊
            Transform spawnPoint = GetSelectedSpawnPoint(selectedPrefab);
            if (spawnPoint != null)
            {
                enemy.transform.position = spawnPoint.position;
                enemy.transform.rotation = spawnPoint.rotation;
            }
            else
            {
                // 使用隨機生成
                Vector3 spawnPosition = GetRandomSpawnPosition();
                enemy.transform.position = spawnPosition;
                enemy.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }
            
            // 重置敵人狀態
            HealthPostureController healthController = enemy.GetComponent<HealthPostureController>();
            if (healthController != null)
            {
                healthController.ResetHealth();
            }
            
            EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
            if (enemyTest != null)
            {
                enemyTest.isDead = false;
            }
            
            // 增加已生成數量計數
            if (spawnedCounts.ContainsKey(selectedPrefab))
            {
                spawnedCounts[selectedPrefab]++;
            }
        }
    }
    
    // 取得物件池中的敵人
    private GameObject GetPooledEnemy(List<GameObject> pool, GameObject prefab)
    {
        // 尋找未啟用的敵人
        foreach (GameObject enemy in pool)
        {
            if (!enemy.activeInHierarchy)
            {
                enemy.SetActive(true);
                return enemy;
            }
        }
        
        // 如果沒有可用的敵人，創建新的
        GameObject newEnemy = Instantiate(prefab, poolParent);
        pool.Add(newEnemy);
        return newEnemy;
    }
    
    // 選擇要生成的敵人預製體
    private GameObject SelectEnemyPrefab()
    {
        if (useFixedSpawnPoints)
        {
            // 依序生成每個預製體，但跳過已達到最大生成數量的預製體
            int attempts = 0;
            int maxAttempts = enemySpawnData.Length * 2;
            
            while (attempts < maxAttempts)
            {
                int dataIndex = currentSpawnIndex % enemySpawnData.Length;
                EnemySpawnData data = enemySpawnData[dataIndex];
                GameObject selectedPrefab = data.enemyPrefab;
                
                // 檢查是否已達到最大生成數量
                if (spawnedCounts.ContainsKey(selectedPrefab))
                {
                    int currentSpawned = spawnedCounts[selectedPrefab];
                    int maxSpawn = data.maxSpawnCount;
                    
                    if (maxSpawn == 0)
                    {
                        currentSpawnIndex++;
                        attempts++;
                        continue;
                    }
                    
                    if (maxSpawn == 1)
                    {
                        maxSpawn = data.poolSize;
                    }
                    
                    if (maxSpawn == -1 || currentSpawned < maxSpawn)
                    {
                        currentSpawnIndex++;
                        return selectedPrefab;
                    }
                }
                
                currentSpawnIndex++;
                attempts++;
            }
            
            return null;
        }
        else
        {
            // 隨機選擇預製體，但跳過已達到最大生成數量的預製體
            List<GameObject> availablePrefabs = new List<GameObject>();
            
            foreach (EnemySpawnData data in enemySpawnData)
            {
                if (data.enemyPrefab != null && spawnedCounts.ContainsKey(data.enemyPrefab))
                {
                    int currentSpawned = spawnedCounts[data.enemyPrefab];
                    int maxSpawn = data.maxSpawnCount;
                    
                    if (maxSpawn == 0) continue;
                    
                    if (maxSpawn == 1)
                    {
                        maxSpawn = data.poolSize;
                    }
                    
                    if (maxSpawn == -1 || currentSpawned < maxSpawn)
                    {
                        availablePrefabs.Add(data.enemyPrefab);
                    }
                }
            }
            
            if (availablePrefabs.Count > 0)
            {
                int randomIndex = Random.Range(0, availablePrefabs.Count);
                return availablePrefabs[randomIndex];
            }
            
            return null;
        }
    }
    
    // 取得選擇的生成點
    private Transform GetSelectedSpawnPoint(GameObject enemyPrefab)
    {
        if (useFixedSpawnPoints && enemySpawnPoints.ContainsKey(enemyPrefab))
        {
            Transform[] spawnPoints = enemySpawnPoints[enemyPrefab];
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                if (useRandomSpawnPoint)
                {
                    // 隨機選擇一個生成點
                    int randomIndex = Random.Range(0, spawnPoints.Length);
                    return spawnPoints[randomIndex];
                }
                else
                {
                    // 依序使用生成點
                    int currentIndex = spawnPointIndices[enemyPrefab];
                    Transform spawnPoint = spawnPoints[currentIndex];
                    
                    // 更新索引，循環使用
                    spawnPointIndices[enemyPrefab] = (currentIndex + 1) % spawnPoints.Length;
                    
                    return spawnPoint;
                }
            }
        }
        
        return null;
    }
    
    // 取得隨機生成位置
    private Vector3 GetRandomSpawnPosition()
    {
        if (useRandomSpawn && playerTransform != null)
        {
            // 使用隨機位置（在玩家周圍）
            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            Vector3 spawnPosition = playerTransform.position + randomDirection * spawnRadius;
            spawnPosition.y = 0; // 確保在地面上
            return spawnPosition;
        }
        
        return Vector3.zero;
    }
    
    // 敵人死亡
    public void OnEnemyDeath()
    {
        currentEnemyCount--;
        currentEnemyCount = Mathf.Max(0, currentEnemyCount);
        
        // 由於敵人不會重生，減少最大敵人數
        if (!respawnDeadEnemies)
        {
            maxEnemies--;
            maxEnemies = Mathf.Max(0, maxEnemies);
        }
    }
    
    // 手動生成敵人
    [ContextMenu("手動生成敵人")]
    public void SpawnEnemyManually()
    {
        if (enemySpawnData == null || enemySpawnData.Length == 0) return;
        
        GameObject selectedPrefab = enemySpawnData[0].enemyPrefab;
        if (selectedPrefab == null) return;
        
        if (enemyPools.ContainsKey(selectedPrefab))
        {
            List<GameObject> pool = enemyPools[selectedPrefab];
            GameObject enemy = GetPooledEnemy(pool, selectedPrefab);
            
            if (enemy != null)
            {
                Transform spawnPoint = GetSelectedSpawnPoint(selectedPrefab);
                if (spawnPoint != null)
                {
                    enemy.transform.position = spawnPoint.position;
                    enemy.transform.rotation = spawnPoint.rotation;
                }
                else
                {
                    Vector3 spawnPosition = GetRandomSpawnPosition();
                    enemy.transform.position = spawnPosition;
                    enemy.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                }
            }
        }
    }
    
    // 按索引生成敵人
    public void SpawnEnemyAtIndex(int index)
    {
        if (enemySpawnData == null || index < 0 || index >= enemySpawnData.Length) return;
        
        GameObject selectedPrefab = enemySpawnData[index].enemyPrefab;
        if (selectedPrefab == null) return;
        
        if (enemyPools.ContainsKey(selectedPrefab))
        {
            List<GameObject> pool = enemyPools[selectedPrefab];
            GameObject enemy = GetPooledEnemy(pool, selectedPrefab);
            
            if (enemy != null)
            {
                Transform spawnPoint = GetSelectedSpawnPoint(selectedPrefab);
                if (spawnPoint != null)
                {
                    enemy.transform.position = spawnPoint.position;
                    enemy.transform.rotation = spawnPoint.rotation;
                }
                else
                {
                    Vector3 spawnPosition = GetRandomSpawnPosition();
                    enemy.transform.position = spawnPosition;
                    enemy.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                }
            }
        }
    }
    
    // 設定生成間隔
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }
    
    // 設定最大敵人數
    public void SetMaxEnemies(int maxEnemies)
    {
        this.maxEnemies = maxEnemies;
    }
    
    // 設定生成模式
    public void SetSpawnMode(bool useFixedPoints, bool useRandom)
    {
        useFixedSpawnPoints = useFixedPoints;
        useRandomSpawn = useRandom;
    }
    
    // 重置敵人系統
    public void ResetEnemySystem(int newMaxEnemies = -1)
    {
        currentEnemyCount = 0;
        currentSpawnIndex = 0;
        
        // 重置所有生成點索引
        foreach (var kvp in spawnPointIndices)
        {
            spawnPointIndices[kvp.Key] = 0;
        }
        
        // 重置所有已生成數量
        foreach (var kvp in spawnedCounts)
        {
            spawnedCounts[kvp.Key] = 0;
        }
        
        if (newMaxEnemies >= 0)
        {
            maxEnemies = newMaxEnemies;
        }
    }
    
    // 停止生成敵人
    public void StopSpawning()
    {
        StopAllCoroutines();
    }
    
    // 開始生成敵人
    public void StartSpawning()
    {
        StartCoroutine(CustomSpawnEnemies());
    }
    
    // 取得當前敵人數
    public int GetCurrentEnemyCount()
    {
        return currentEnemyCount;
    }
    
    // 取得最大敵人數
    public int GetMaxEnemies()
    {
        return maxEnemies;
    }
    
    // 驗證設定
    void OnValidate()
    {
        respawnDeadEnemies = false;
    }
}