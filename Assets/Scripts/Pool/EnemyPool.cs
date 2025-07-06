using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敵人物件池使用範例
public class EnemyPool : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab; // 敵人預製體
        public Transform[] spawnPoints; // 對應的多個生成點
        public int poolSize = 5; // 該類型敵人的物件池大小
    }

    [Header("多敵人預製體設定")]
    public EnemySpawnData[] enemySpawnData; // 敵人預製體和生成點陣列

    [Header("物件池設定")]
    public Transform poolParent; // 物件池父物件

    [Header("生成器設定")]
    public Transform playerTransform; // 玩家位置
    public float spawnRadius = 25f; // 生成範圍（用於隨機生成）
    public float spawnInterval = 2f; // 生成間隔
    public int maxEnemies = 15; // 最大敵人數
    public bool respawnDeadEnemies = false; // 是否重新生成死亡的敵人（設為false）

    [Header("生成模式")]
    public bool useFixedSpawnPoints = true; // 是否使用固定生成點
    public bool useRandomSpawn = false; // 是否使用隨機生成
    public bool useRandomSpawnPoint = true; // 是否隨機選擇生成點

    private Dictionary<GameObject, List<GameObject>> enemyPools; // 敵人預製體對應的物件池
    private Dictionary<GameObject, Transform[]> enemySpawnPoints; // 敵人預製體對應的多個生成點
    private int currentEnemyCount = 0; // 當前敵人數
    private int currentSpawnIndex = 0; // 當前生成索引
    private Dictionary<GameObject, int> spawnPointIndices; // 記錄每個預製體當前使用的生成點索引

    void Start()
    {
        SetupEnemyPools();
        StartCoroutine(CustomSpawnEnemies());
    }

    // 設置多個敵人物件池
    private void SetupEnemyPools()
    {
        enemyPools = new Dictionary<GameObject, List<GameObject>>();
        enemySpawnPoints = new Dictionary<GameObject, Transform[]>();
        spawnPointIndices = new Dictionary<GameObject, int>();

        if (enemySpawnData == null || enemySpawnData.Length == 0)
        {
            Debug.LogError("請設置敵人預製體和生成點！");
            return;
        }

        foreach (EnemySpawnData data in enemySpawnData)
        {
            if (data.enemyPrefab == null)
            {
                Debug.LogWarning("發現空的敵人預製體，已跳過");
                continue;
            }

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
                spawnPointIndices[data.enemyPrefab] = 0; // 初始化生成點索引
            }

            Debug.Log($"已創建敵人預製體 {data.enemyPrefab.name} 的物件池，大小: {data.poolSize}，生成點數量: {(data.spawnPoints != null ? data.spawnPoints.Length : 0)}");
        }
    }

    // 自定義敵人生成邏輯
    private IEnumerator CustomSpawnEnemies()
    {
        // 等待一幀，確保初始化完成
        yield return null;

        while (true)
        {
            // 檢查當前敵人數是否達到上限
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
                currentEnemyCount++;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // 生成單個敵人
    private void SpawnEnemy()
    {
        if (enemySpawnData == null || enemySpawnData.Length == 0) return;

        // 選擇要生成的敵人預製體
        GameObject selectedPrefab = SelectEnemyPrefab();
        if (selectedPrefab == null) return;

        // 取得對應的物件池
        if (!enemyPools.ContainsKey(selectedPrefab))
        {
            Debug.LogError($"找不到敵人預製體 {selectedPrefab.name} 的物件池");
            return;
        }

        List<GameObject> pool = enemyPools[selectedPrefab];
        GameObject enemy = GetPooledEnemy(pool, selectedPrefab);

        if (enemy != null)
        {
            // 設置生成位置
            Vector3 spawnPosition = GetSpawnPosition(selectedPrefab);
            enemy.transform.position = spawnPosition;

            // 讓敵人面向玩家
            if (playerTransform != null)
            {
                Vector3 directionToPlayer = (playerTransform.position - spawnPosition).normalized;
                enemy.transform.rotation = Quaternion.LookRotation(directionToPlayer);
            }

            // 重置敵人狀態
            EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
            if (enemyTest != null)
            {
                enemyTest.currentHP = enemyTest.maxHP;
            }

            Debug.Log($"已生成敵人 {selectedPrefab.name} 在位置 {spawnPosition}");
        }
    }

    // 從物件池取得敵人
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
        Debug.Log($"為 {prefab.name} 創建新的敵人實例");
        return newEnemy;
    }

    // 選擇要生成的敵人預製體
    private GameObject SelectEnemyPrefab()
    {
        if (useFixedSpawnPoints)
        {
            // 依序生成每個預製體
            GameObject selectedPrefab = enemySpawnData[currentSpawnIndex % enemySpawnData.Length].enemyPrefab;
            currentSpawnIndex++;
            return selectedPrefab;
        }
        else
        {
            // 隨機選擇預製體
            int randomIndex = Random.Range(0, enemySpawnData.Length);
            return enemySpawnData[randomIndex].enemyPrefab;
        }
    }

    // 取得生成位置
    private Vector3 GetSpawnPosition(GameObject enemyPrefab)
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
                    return spawnPoints[randomIndex].position;
                }
                else
                {
                    // 依序使用生成點
                    int currentIndex = spawnPointIndices[enemyPrefab];
                    Vector3 position = spawnPoints[currentIndex].position;

                    // 更新索引，循環使用
                    spawnPointIndices[enemyPrefab] = (currentIndex + 1) % spawnPoints.Length;

                    return position;
                }
            }
        }
        else if (useRandomSpawn && playerTransform != null)
        {
            // 使用隨機位置（在玩家周圍）
            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            Vector3 spawnPosition = playerTransform.position + randomDirection * spawnRadius;
            spawnPosition.y = 0; // 確保在地面上
            return spawnPosition;
        }

        // 預設位置
        return Vector3.zero;
    }

    // 敵人死亡時減少計數
    public void OnEnemyDeath()
    {
        currentEnemyCount--;
        currentEnemyCount = Mathf.Max(0, currentEnemyCount); // 確保不會小於0

        // 由於敵人不會重生，減少最大敵人數
        if (!respawnDeadEnemies)
        {
            maxEnemies--;
            maxEnemies = Mathf.Max(0, maxEnemies); // 確保不會小於0
        }

        Debug.Log($"敵人死亡，當前敵人數: {currentEnemyCount}，最大敵人數: {maxEnemies}");
    }

    // 手動生成指定敵人（用於測試）
    [ContextMenu("手動生成敵人")]
    public void SpawnEnemyManually()
    {
        if (enemySpawnData == null || enemySpawnData.Length == 0) return;

        // 選擇第一個可用的敵人預製體
        GameObject selectedPrefab = enemySpawnData[0].enemyPrefab;
        if (selectedPrefab == null) return;

        if (enemyPools.ContainsKey(selectedPrefab))
        {
            List<GameObject> pool = enemyPools[selectedPrefab];
            GameObject enemy = GetPooledEnemy(pool, selectedPrefab);

            if (enemy != null)
            {
                Vector3 spawnPosition = GetSpawnPosition(selectedPrefab);
                enemy.transform.position = spawnPosition;

                if (playerTransform != null)
                {
                    Vector3 directionToPlayer = (playerTransform.position - spawnPosition).normalized;
                    enemy.transform.rotation = Quaternion.LookRotation(directionToPlayer);
                }

                Debug.Log($"手動生成敵人 {selectedPrefab.name} 成功！");
            }
        }
    }

    // 手動生成指定索引的敵人
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
                Vector3 spawnPosition = GetSpawnPosition(selectedPrefab);
                enemy.transform.position = spawnPosition;

                if (playerTransform != null)
                {
                    Vector3 directionToPlayer = (playerTransform.position - spawnPosition).normalized;
                    enemy.transform.rotation = Quaternion.LookRotation(directionToPlayer);
                }

                Debug.Log($"手動生成敵人 {selectedPrefab.name} 在索引 {index} 成功！");
            }
        }
    }

    // 設置生成間隔
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }

    // 設置最大敵人數
    public void SetMaxEnemies(int maxEnemies)
    {
        this.maxEnemies = maxEnemies;
    }

    // 切換生成模式
    public void SetSpawnMode(bool useFixedPoints, bool useRandom)
    {
        useFixedSpawnPoints = useFixedPoints;
        useRandomSpawn = useRandom;
    }

    // 重置敵人系統（重新開始生成）
    public void ResetEnemySystem(int newMaxEnemies = -1)
    {
        currentEnemyCount = 0;
        currentSpawnIndex = 0;

        // 重置所有生成點索引
        foreach (var kvp in spawnPointIndices)
        {
            spawnPointIndices[kvp.Key] = 0;
        }

        if (newMaxEnemies >= 0)
        {
            maxEnemies = newMaxEnemies;
        }

        Debug.Log($"敵人系統已重置，最大敵人數: {maxEnemies}");
    }

    // 停止生成敵人
    public void StopSpawning()
    {
        StopAllCoroutines();
        Debug.Log("敵人生成已停止");
    }

    // 開始生成敵人
    public void StartSpawning()
    {
        StartCoroutine(CustomSpawnEnemies());
        Debug.Log("敵人生成已開始");
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

    // 在Inspector中顯示當前狀態
    void OnValidate()
    {
        // 確保不重生設定為false
        respawnDeadEnemies = false;
    }
}