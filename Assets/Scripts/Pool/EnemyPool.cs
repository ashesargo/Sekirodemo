using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵人物件池系統
/// 功能：
/// 1. 管理多種類型敵人的物件池
/// 2. 支援固定生成點和隨機生成
/// 3. 自動控制敵人生成數量和頻率
/// 4. 提供手動生成和系統重置功能
/// 
/// 使用方式：
/// 1. 在 Inspector 中設定 EnemySpawnData 陣列
/// 2. 為每種敵人類型指定預製體和生成點
/// 3. 調整生成參數（間隔、最大數量等）
/// 4. 系統會自動開始生成敵人
/// </summary>
public class EnemyPool : MonoBehaviour
{
    /// <summary>
    /// 敵人生成資料結構
    /// 用於定義每種敵人類型的生成設定
    /// </summary>
    [System.Serializable]
    public class EnemySpawnData
    {
        [Tooltip("敵人預製體 - 拖拽敵人預製體到此欄位")]
        public GameObject enemyPrefab; // 敵人預製體
        
        [Tooltip("生成點陣列 - 設定該類型敵人的所有可能生成位置")]
        public Transform[] spawnPoints; // 對應的多個生成點
        
        [Tooltip("物件池大小 - 預先創建的該類型敵人數量")]
        public int poolSize = 5; // 該類型敵人的物件池大小
    }

    [Header("多敵人預製體設定")]
    [Tooltip("敵人預製體和生成點陣列 - 每種敵人類型的詳細設定")]
    public EnemySpawnData[] enemySpawnData; // 敵人預製體和生成點陣列

    [Header("物件池設定")]
    [Tooltip("物件池父物件 - 所有池化敵人的父物件，建議設為空物件")]
    public Transform poolParent; // 物件池父物件

    [Header("生成器設定")]
    [Tooltip("玩家位置 - 用於計算隨機生成位置")]
    public Transform playerTransform; // 玩家位置
    
    [Tooltip("生成範圍 - 隨機生成時與玩家的距離")]
    public float spawnRadius = 25f; // 生成範圍（用於隨機生成）
    
    [Tooltip("生成間隔 - 每次生成敵人的時間間隔（秒）")]
    public float spawnInterval = 2f; // 生成間隔
    
    [Tooltip("最大敵人數 - 同時存在的最大敵人數")]
    public int maxEnemies = 15; // 最大敵人數
    
    [Tooltip("是否重新生成死亡的敵人 - 建議設為 false")]
    public bool respawnDeadEnemies = false; // 是否重新生成死亡的敵人（設為false）

    [Header("生成模式")]
    [Tooltip("是否使用固定生成點 - 啟用時使用預設的生成點位置")]
    public bool useFixedSpawnPoints = true; // 是否使用固定生成點
    
    [Tooltip("是否使用隨機生成 - 啟用時在玩家周圍隨機生成")]
    public bool useRandomSpawn = false; // 是否使用隨機生成
    
    [Tooltip("是否隨機選擇生成點 - 啟用時隨機選擇預設生成點")]
    public bool useRandomSpawnPoint = true; // 是否隨機選擇生成點

    // 私有變數
    private Dictionary<GameObject, List<GameObject>> enemyPools; // 敵人預製體對應的物件池
    private Dictionary<GameObject, Transform[]> enemySpawnPoints; // 敵人預製體對應的多個生成點
    private int currentEnemyCount = 0; // 當前敵人數
    private int currentSpawnIndex = 0; // 當前生成索引
    private Dictionary<GameObject, int> spawnPointIndices; // 記錄每個預製體當前使用的生成點索引

    /// <summary>
    /// 初始化敵人池系統
    /// </summary>
    void Start()
    {
        SetupEnemyPools();
        StartCoroutine(CustomSpawnEnemies());
    }

    /// <summary>
    /// 設置多個敵人物件池
    /// 為每種敵人類型創建對應的物件池和生成點映射
    /// </summary>
    private void SetupEnemyPools()
    {
        // 初始化字典
        enemyPools = new Dictionary<GameObject, List<GameObject>>();
        enemySpawnPoints = new Dictionary<GameObject, Transform[]>();
        spawnPointIndices = new Dictionary<GameObject, int>();

        // 檢查設定是否正確
        if (enemySpawnData == null || enemySpawnData.Length == 0)
        {
            Debug.LogError("請設置敵人預製體和生成點！");
            return;
        }

        // 為每種敵人類型創建物件池
        foreach (EnemySpawnData data in enemySpawnData)
        {
            if (data.enemyPrefab == null)
            {
                Debug.LogWarning("發現空的敵人預製體，已跳過");
                continue;
            }

            // 創建物件池列表
            List<GameObject> pool = new List<GameObject>();

            // 預先創建指定數量的敵人物件
            for (int i = 0; i < data.poolSize; i++)
            {
                GameObject enemy = Instantiate(data.enemyPrefab, poolParent);
                enemy.SetActive(false); // 初始狀態為非啟用
                pool.Add(enemy);
            }

            // 儲存物件池和生成點資訊
            enemyPools[data.enemyPrefab] = pool;
            if (data.spawnPoints != null && data.spawnPoints.Length > 0)
            {
                enemySpawnPoints[data.enemyPrefab] = data.spawnPoints;
                spawnPointIndices[data.enemyPrefab] = 0; // 初始化生成點索引
            }

            Debug.Log($"已創建敵人預製體 {data.enemyPrefab.name} 的物件池，大小: {data.poolSize}，生成點數量: {(data.spawnPoints != null ? data.spawnPoints.Length : 0)}");
        }
    }

    /// <summary>
    /// 自定義敵人生成邏輯協程
    /// 持續生成敵人直到達到最大數量
    /// </summary>
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

            // 等待指定的生成間隔
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 生成單個敵人
    /// 從物件池取得敵人並設定位置和狀態
    /// </summary>
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

        // 從物件池取得敵人
        List<GameObject> pool = enemyPools[selectedPrefab];
        GameObject enemy = GetPooledEnemy(pool, selectedPrefab);

        if (enemy != null)
        {
            // 取得生成點資訊並設定位置
            Transform spawnPoint = GetSelectedSpawnPoint(selectedPrefab);
            if (spawnPoint != null)
            {
                // 使用預設生成點
                enemy.transform.position = spawnPoint.position;
                enemy.transform.rotation = spawnPoint.rotation;
            }
            else
            {
                // 使用隨機生成位置
                Vector3 spawnPosition = GetRandomSpawnPosition();
                enemy.transform.position = spawnPosition;
                enemy.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }

            // 重置敵人狀態（恢復血量等）
            EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
            if (enemyTest != null)
            {
                enemyTest.currentHP = enemyTest.maxHP;
            }

            Debug.Log($"已生成敵人 {selectedPrefab.name} 在位置 {enemy.transform.position}，旋轉 {enemy.transform.rotation.eulerAngles}");
        }
    }

    /// <summary>
    /// 從物件池取得敵人
    /// 優先使用已存在的非啟用物件，不足時創建新的
    /// </summary>
    /// <param name="pool">物件池列表</param>
    /// <param name="prefab">敵人預製體</param>
    /// <returns>可用的敵人物件</returns>
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

        // 如果沒有可用的敵人，創建新的並加入池中
        GameObject newEnemy = Instantiate(prefab, poolParent);
        pool.Add(newEnemy);
        Debug.Log($"為 {prefab.name} 創建新的敵人實例");
        return newEnemy;
    }

    /// <summary>
    /// 選擇要生成的敵人預製體
    /// 根據設定決定是依序生成還是隨機選擇
    /// </summary>
    /// <returns>選中的敵人預製體</returns>
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

    /// <summary>
    /// 取得選定的生成點
    /// 根據設定決定是隨機選擇還是依序使用生成點
    /// </summary>
    /// <param name="enemyPrefab">敵人預製體</param>
    /// <returns>選中的生成點</returns>
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

    /// <summary>
    /// 取得隨機生成位置
    /// 在玩家周圍指定半徑內隨機生成
    /// </summary>
    /// <returns>隨機生成位置</returns>
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

        // 預設位置
        return Vector3.zero;
    }

    /// <summary>
    /// 敵人死亡時的回調方法
    /// 減少當前敵人計數，並根據設定調整最大敵人數
    /// </summary>
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

    /// <summary>
    /// 手動生成指定敵人（用於測試）
    /// 在 Inspector 中右鍵選擇此方法進行測試
    /// </summary>
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

                Debug.Log($"手動生成敵人 {selectedPrefab.name} 成功！");
            }
        }
    }

    /// <summary>
    /// 手動生成指定索引的敵人
    /// </summary>
    /// <param name="index">敵人在 enemySpawnData 陣列中的索引</param>
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

                Debug.Log($"手動生成敵人 {selectedPrefab.name} 在索引 {index} 成功！");
            }
        }
    }

    /// <summary>
    /// 設置生成間隔
    /// </summary>
    /// <param name="interval">新的生成間隔（秒）</param>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }

    /// <summary>
    /// 設置最大敵人數
    /// </summary>
    /// <param name="maxEnemies">新的最大敵人數</param>
    public void SetMaxEnemies(int maxEnemies)
    {
        this.maxEnemies = maxEnemies;
    }

    /// <summary>
    /// 切換生成模式
    /// </summary>
    /// <param name="useFixedPoints">是否使用固定生成點</param>
    /// <param name="useRandom">是否使用隨機生成</param>
    public void SetSpawnMode(bool useFixedPoints, bool useRandom)
    {
        useFixedSpawnPoints = useFixedPoints;
        useRandomSpawn = useRandom;
    }

    /// <summary>
    /// 重置敵人系統（重新開始生成）
    /// </summary>
    /// <param name="newMaxEnemies">新的最大敵人數，-1 表示保持原值</param>
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

    /// <summary>
    /// 停止生成敵人
    /// 停止所有正在運行的生成協程
    /// </summary>
    public void StopSpawning()
    {
        StopAllCoroutines();
        Debug.Log("敵人生成已停止");
    }

    /// <summary>
    /// 開始生成敵人
    /// 重新啟動生成協程
    /// </summary>
    public void StartSpawning()
    {
        StartCoroutine(CustomSpawnEnemies());
        Debug.Log("敵人生成已開始");
    }

    /// <summary>
    /// 取得當前敵人數
    /// </summary>
    /// <returns>當前活著的敵人數</returns>
    public int GetCurrentEnemyCount()
    {
        return currentEnemyCount;
    }

    /// <summary>
    /// 取得最大敵人數
    /// </summary>
    /// <returns>最大敵人數</returns>
    public int GetMaxEnemies()
    {
        return maxEnemies;
    }

    /// <summary>
    /// 在 Inspector 中顯示當前狀態
    /// 確保不重生設定為 false
    /// </summary>
    void OnValidate()
    {
        // 確保不重生設定為false
        respawnDeadEnemies = false;
    }
}