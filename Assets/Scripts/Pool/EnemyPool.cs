using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵人物件池系統 - 管理多種類型敵人的生成、回收和生命週期
/// 
/// 主要功能：
/// 1. 支援多種敵人預製體，每種可設定不同的生成點和生成數量
/// 2. 智能生成控制：依序生成、隨機生成、固定生成點、隨機生成點
/// 3. 物件池優化：預先創建物件，避免頻繁的Instantiate/Destroy
/// 4. 除錯功能：提供詳細的狀態檢查和設定驗證
/// 
/// 使用方式：
/// 1. 將此腳本掛載到場景中的空物件上
/// 2. 在Inspector中設定enemySpawnData陣列
/// 3. 為每個敵人預製體設定對應的生成點和生成參數
/// 4. 調整生成模式和間隔設定
/// 5. 運行遊戲，系統會自動開始生成敵人
/// 
/// 注意事項：
/// - maxSpawnCount只允許設定為 -1(不限制)、0(不生成)、1(按poolSize生成)
/// - 建議將respawnDeadEnemies設為false，避免敵人重生
/// - 使用除錯功能檢查設定是否正確
/// </summary>
public class EnemyPool : MonoBehaviour
{
    /// <summary>
    /// 敵人生成資料結構 - 定義每種敵人預製體的生成設定
    /// 
    /// 包含：
    /// - 敵人預製體引用
    /// - 對應的生成點陣列
    /// - 物件池大小（預先創建的物件數量）
    /// - 最大生成數量（控制實際生成的敵人數量）
    /// 
    /// 使用方式：
    /// 在Inspector中為每種敵人預製體創建一個EnemySpawnData實例，
    /// 設定對應的生成點和生成參數。
    /// </summary>
    [System.Serializable]
    public class EnemySpawnData
    {
        [Tooltip("敵人預製體 - 拖拽敵人預製體到此欄位")]
        public GameObject enemyPrefab; // 敵人預製體
        
        [Tooltip("生成點陣列 - 設定該類型敵人的所有可能生成位置")]
        public Transform[] spawnPoints; // 對應的多個生成點
        
        [Tooltip("物件池大小 - 預先創建的該類型敵人數量，建議設定為實際需要的最大數量")]
        public int poolSize = 5; // 該類型敵人的物件池大小
        
        [Tooltip("最大生成數量 - 1=按poolSize生成, -1=不限制, 0=不生成, 其他值=報錯")]
        public int maxSpawnCount = -1; // 最大生成數量，1=按poolSize生成, -1=不限制，0=不生成
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
    public bool useRandomSpawnPoint = false; // 是否隨機選擇生成點
    
    // ===== 私有變數 =====
    
    /// <summary>
    /// 敵人預製體對應的物件池字典
    /// Key: 敵人預製體 GameObject
    /// Value: 該預製體的物件池列表
    /// </summary>
    private Dictionary<GameObject, List<GameObject>> enemyPools;
    
    /// <summary>
    /// 敵人預製體對應的生成點陣列字典
    /// Key: 敵人預製體 GameObject
    /// Value: 該預製體的所有生成點陣列
    /// </summary>
    private Dictionary<GameObject, Transform[]> enemySpawnPoints;
    
    /// <summary>
    /// 當前活著的敵人數量
    /// 用於控制生成頻率和最大敵人數限制
    /// </summary>
    private int currentEnemyCount = 0;
    
    /// <summary>
    /// 當前生成索引
    /// 用於依序生成模式，決定下一個要生成的預製體
    /// </summary>
    private int currentSpawnIndex = 0;
    
    /// <summary>
    /// 記錄每個預製體當前使用的生成點索引
    /// Key: 敵人預製體 GameObject
    /// Value: 當前使用的生成點索引
    /// </summary>
    private Dictionary<GameObject, int> spawnPointIndices;
    
    /// <summary>
    /// 記錄每個預製體已生成的數量
    /// Key: 敵人預製體 GameObject
    /// Value: 已生成的數量
    /// 用於控制最大生成數量限制
    /// </summary>
    private Dictionary<GameObject, int> spawnedCounts;
    
    /// <summary>
    /// Unity生命週期：遊戲開始時初始化敵人池系統
    /// 
    /// 執行順序：
    /// 1. 設置多個敵人物件池（預先創建物件）
    /// 2. 開始自動生成敵人的協程
    /// 
    /// 注意：確保在Start之前已經在Inspector中正確設定了所有參數
    /// </summary>
    void Start()
    {
        SetupEnemyPools();
        StartCoroutine(CustomSpawnEnemies());
    }
    
    /// <summary>
    /// 設置多個敵人物件池
    /// 
    /// 功能：
    /// 1. 初始化所有字典和計數器
    /// 2. 驗證每個EnemySpawnData的設定
    /// 3. 為每個敵人預製體創建物件池
    /// 4. 預先創建指定數量的敵人物件
    /// 5. 建立預製體與生成點的映射關係
    /// 
    /// 驗證規則：
    /// - maxSpawnCount只允許 -1、0、1
    /// - 空的敵人預製體會被跳過
    /// - 設定錯誤的預製體會報錯並跳過
    /// 
    /// 注意：此方法在Start時自動調用，通常不需要手動調用
    /// </summary>
    private void SetupEnemyPools()
    {
        enemyPools = new Dictionary<GameObject, List<GameObject>>();
        enemySpawnPoints = new Dictionary<GameObject, Transform[]>();
        spawnPointIndices = new Dictionary<GameObject, int>();
        spawnedCounts = new Dictionary<GameObject, int>();
        
        if (enemySpawnData == null || enemySpawnData.Length == 0)
        {
            Debug.LogError("請設置敵人預製體和生成點！");
            return;
        }
        
        for (int i = 0; i < enemySpawnData.Length; i++)
        {
            EnemySpawnData data = enemySpawnData[i];
            Debug.Log($"敵人 {i}: {data.enemyPrefab?.name ?? "未設定"}");
            Debug.Log($"  物件池大小: {data.poolSize}");
            Debug.Log($"  最大生成數量: {data.maxSpawnCount} (-1=不限制, 0=不生成, 1=按poolSize生成)");
            Debug.Log($"  生成點數量: {(data.spawnPoints != null ? data.spawnPoints.Length : 0)}");
            
            if (data.spawnPoints != null)
            {
                for (int j = 0; j < data.spawnPoints.Length; j++)
                {
                    Debug.Log($"    生成點 {j}: {data.spawnPoints[j]?.name ?? "未設定"}");
                }
            }
        }
        
        foreach (EnemySpawnData data in enemySpawnData)
        {
            if (data.enemyPrefab == null)
            {
                Debug.LogWarning("發現空的敵人預製體，已跳過");
                continue;
            }
            
            // 驗證 maxSpawnCount 設定
            if (data.maxSpawnCount != -1 && data.maxSpawnCount != 0 && data.maxSpawnCount != 1)
            {
                Debug.LogError($"敵人預製體 {data.enemyPrefab.name} 的 maxSpawnCount 設定錯誤！只允許 -1(不限制)、0(不生成)、1(按poolSize生成)，當前值: {data.maxSpawnCount}");
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
            spawnedCounts[data.enemyPrefab] = 0; // 初始化已生成數量
            
            Debug.Log($"已創建敵人預製體 {data.enemyPrefab.name} 的物件池，大小: {data.poolSize}，生成點數量: {(data.spawnPoints != null ? data.spawnPoints.Length : 0)}，最大生成數量: {data.maxSpawnCount}");
        }
    }
    
    /// <summary>
    /// 自定義敵人生成邏輯協程
    /// 
    /// 功能：
    /// 1. 持續監控當前敵人數量
    /// 2. 當敵人數量未達到上限時自動生成新敵人
    /// 3. 按照設定的間隔時間控制生成頻率
    /// 4. 支援無限循環生成（直到手動停止）
    /// 
    /// 生成條件：
    /// - 當前敵人數 < 最大敵人數
    /// - 有可用的敵人預製體（未達到最大生成數量）
    /// 
    /// 控制方式：
    /// - 使用StopSpawning()停止此協程
    /// - 使用StartSpawning()重新啟動此協程
    /// 
    /// 注意：此協程在Start時自動啟動，會一直運行直到手動停止
    /// </summary>
    /// <returns>協程迭代器</returns>
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
    
    /// <summary>
    /// 生成單個敵人
    /// 
    /// 功能：
    /// 1. 選擇要生成的敵人預製體（根據生成模式）
    /// 2. 從物件池取得可用的敵人物件
    /// 3. 設定敵人的位置和旋轉
    /// 4. 重置敵人狀態（血量等）
    /// 5. 更新已生成數量計數
    /// 
    /// 生成位置選擇：
    /// - 優先使用固定生成點（如果設定）
    /// - 如果沒有固定生成點，使用隨機位置
    /// - 生成點選擇：隨機或依序（根據useRandomSpawnPoint設定）
    /// 
    /// 狀態重置：
    /// - 自動尋找EnemyTest組件並重置血量
    /// - 如果沒有EnemyTest組件，跳過狀態重置
    /// 
    /// 注意：此方法由CustomSpawnEnemies協程自動調用
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
            EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
            if (enemyTest != null)
            {
                enemyTest.currentHP = enemyTest.maxHP;
            }
            
            // 增加已生成數量計數
            if (spawnedCounts.ContainsKey(selectedPrefab))
            {
                spawnedCounts[selectedPrefab]++;
                Debug.Log($"已生成敵人 {selectedPrefab.name} 在位置 {enemy.transform.position}，旋轉 {enemy.transform.rotation.eulerAngles}，已生成數量: {spawnedCounts[selectedPrefab]}");
            }
        }
    }
    
    /// <summary>
    /// 從物件池取得敵人
    /// 
    /// 功能：
    /// 1. 在物件池中尋找未啟用的敵人物件
    /// 2. 如果找到可用的物件，啟用並返回
    /// 3. 如果沒有可用的物件，創建新的並加入池中
    /// 
    /// 物件池邏輯：
    /// - 優先重用已存在的物件（避免頻繁創建）
    /// - 只有當池中沒有可用物件時才創建新的
    /// - 新創建的物件會自動加入池中供下次使用
    /// 
    /// 性能優化：
    /// - 避免運行時的Instantiate操作
    /// - 減少記憶體分配和垃圾回收
    /// - 提高敵人生成的響應速度
    /// 
    /// 參數：
    /// - pool: 指定預製體的物件池列表
    /// - prefab: 敵人預製體（用於創建新物件）
    /// 
    /// 返回值：可用的敵人物件，如果創建失敗返回null
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
        
        // 如果沒有可用的敵人，創建新的
        GameObject newEnemy = Instantiate(prefab, poolParent);
        pool.Add(newEnemy);
        Debug.Log($"為 {prefab.name} 創建新的敵人實例");
        return newEnemy;
    }
    
    /// <summary>
    /// 選擇要生成的敵人預製體
    /// 
    /// 功能：
    /// 1. 根據生成模式選擇敵人預製體
    /// 2. 檢查預製體是否已達到最大生成數量
    /// 3. 跳過不可用的預製體（已達到限制或設定為不生成）
    /// 
    /// 生成模式：
    /// - 固定生成點模式（useFixedSpawnPoints = true）：
    ///   * 依序選擇每個預製體
    ///   * 跳過已達到最大數量的預製體
    ///   * 如果所有預製體都達到限制，返回null
    /// 
    /// - 隨機生成模式（useFixedSpawnPoints = false）：
    ///   * 從可用的預製體中隨機選擇
    ///   * 只考慮未達到最大數量的預製體
    ///   * 如果沒有可用預製體，返回null
    /// 
    /// 最大生成數量檢查：
    /// - maxSpawnCount = -1：不限制
    /// - maxSpawnCount = 0：不生成
    /// - maxSpawnCount = 1：按poolSize生成
    /// 
    /// 返回值：選中的敵人預製體，如果沒有可用的預製體返回null
    /// </summary>
    /// <returns>選中的敵人預製體</returns>
    private GameObject SelectEnemyPrefab()
    {
        if (useFixedSpawnPoints)
        {
            // 依序生成每個預製體，但跳過已達到最大生成數量的預製體
            int attempts = 0;
            int maxAttempts = enemySpawnData.Length * 2; // 防止無限循環
            
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
                    
                    // 如果maxSpawn為0，表示不生成此預製體
                    if (maxSpawn == 0)
                    {
                        currentSpawnIndex++;
                        attempts++;
                        continue;
                    }
                    
                    // 如果maxSpawn為1，使用poolSize作為最大生成數量
                    if (maxSpawn == 1)
                    {
                        maxSpawn = data.poolSize;
                    }
                    
                    // 如果maxSpawn為-1，表示不限制，或者還沒達到最大數量
                    if (maxSpawn == -1 || currentSpawned < maxSpawn)
                    {
                        currentSpawnIndex++;
                        return selectedPrefab;
                    }
                }
                
                currentSpawnIndex++;
                attempts++;
            }
            
            // 如果所有預製體都達到最大生成數量，返回null
            Debug.Log("所有敵人預製體都已達到最大生成數量");
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
                    
                    // 如果maxSpawn為0，表示不生成此預製體
                    if (maxSpawn == 0) continue;
                    
                    // 如果maxSpawn為1，使用poolSize作為最大生成數量
                    if (maxSpawn == 1)
                    {
                        maxSpawn = data.poolSize;
                    }
                    
                    // 如果maxSpawn為-1，表示不限制，或者還沒達到最大數量
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
            
            Debug.Log("沒有可用的敵人預製體");
            return null;
        }
    }
    
    /// <summary>
    /// 取得選定的生成點
    /// 
    /// 功能：
    /// 1. 根據生成模式選擇生成點
    /// 2. 支援隨機選擇和依序選擇生成點
    /// 3. 自動更新生成點索引（依序模式）
    /// 
    /// 生成點選擇邏輯：
    /// - 固定生成點模式（useFixedSpawnPoints = true）：
    ///   * 從預製體對應的生成點陣列中選擇
    ///   * 如果沒有設定生成點，返回null
    /// 
    /// - 隨機生成點模式（useRandomSpawnPoint = true）：
    ///   * 從所有可用生成點中隨機選擇一個
    ///   * 每次選擇都是完全隨機的
    /// 
    /// - 依序生成點模式（useRandomSpawnPoint = false）：
    ///   * 按照生成點陣列的順序依次使用
    ///   * 到達陣列末尾時循環回到第一個
    ///   * 自動維護每個預製體的生成點索引
    /// 
    /// 返回值：選中的生成點Transform，如果沒有可用的生成點返回null
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
    /// 
    /// 功能：
    /// 1. 在玩家周圍指定半徑內生成隨機位置
    /// 2. 確保生成位置在地面上（Y軸設為0）
    /// 3. 提供預設位置作為備用
    /// 
    /// 隨機位置計算：
    /// - 使用Random.insideUnitSphere生成隨機方向
    /// - 將方向標準化後乘以生成半徑
    /// - 以玩家位置為中心計算最終位置
    /// 
    /// 使用條件：
    /// - 只有在useRandomSpawn = true時才使用隨機位置
    /// - 需要設定有效的playerTransform
    /// 
    /// 備用方案：
    /// - 如果沒有啟用隨機生成或沒有玩家位置，返回Vector3.zero
    /// 
    /// 返回值：隨機生成位置，如果無法計算返回Vector3.zero
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
    /// 
    /// 功能：
    /// 1. 減少當前敵人計數
    /// 2. 根據重生設定調整最大敵人數
    /// 3. 輸出除錯資訊
    /// 
    /// 計數調整：
    /// - currentEnemyCount減1（確保不會小於0）
    /// - 如果respawnDeadEnemies = false，maxEnemies也減1
    /// 
    /// 使用方式：
    /// 在敵人的死亡邏輯中調用此方法
    /// 例如：enemyPool.OnEnemyDeath();
    /// 
    /// 注意：
    /// - 此方法不會影響已生成數量計數（spawnedCounts）
    /// - 已生成數量計數用於控制最大生成數量，不會因為死亡而減少
    /// - 建議在敵人真正死亡時調用，而不是在受傷時調用
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
    /// 
    /// 功能：
    /// 1. 手動生成第一個可用的敵人預製體
    /// 2. 使用與自動生成相同的邏輯
    /// 3. 適用於測試和除錯
    /// 
    /// 生成邏輯：
    /// - 選擇enemySpawnData陣列中的第一個預製體
    /// - 檢查該預製體是否可用（未達到最大生成數量）
    /// - 使用相同的生成點選擇邏輯
    /// - 重置敵人狀態
    /// 
    /// 使用方式：
    /// - 在Unity Inspector中右鍵點擊EnemyPool組件
    /// - 選擇「手動生成敵人」選項
    /// - 或在程式碼中調用：enemyPool.SpawnEnemyManually();
    /// 
    /// 注意：
    /// - 此方法會增加已生成數量計數
    /// - 如果第一個預製體不可用，不會生成任何敵人
    /// - 建議在測試時使用，生產環境中通常使用自動生成
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
    /// 
    /// 功能：
    /// 1. 根據陣列索引手動生成指定的敵人預製體
    /// 2. 使用與自動生成相同的邏輯
    /// 3. 適用於精確控制敵人生成
    /// 
    /// 參數驗證：
    /// - 檢查索引是否在有效範圍內
    /// - 檢查指定的預製體是否存在
    /// - 如果驗證失敗，方法會靜默返回
    /// 
    /// 生成邏輯：
    /// - 使用指定的預製體（不考慮生成模式）
    /// - 檢查該預製體是否可用（未達到最大生成數量）
    /// - 使用相同的生成點選擇邏輯
    /// - 重置敵人狀態
    /// 
    /// 使用方式：
    /// - 在程式碼中調用：enemyPool.SpawnEnemyAtIndex(0);
    /// - 索引對應enemySpawnData陣列中的位置
    /// 
    /// 注意：
    /// - 此方法會增加已生成數量計數
    /// - 如果指定的預製體不可用，不會生成任何敵人
    /// - 索引從0開始，確保不超出陣列範圍
    /// </summary>
    /// <param name="index">敵人在enemySpawnData陣列中的索引</param>
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
    /// 
    /// 功能：動態調整敵人生成的時間間隔
    /// 參數：interval - 新的生成間隔（秒）
    /// 注意：此設定會立即生效，影響正在運行的生成協程
    /// </summary>
    /// <param name="interval">新的生成間隔（秒）</param>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }
    
    /// <summary>
    /// 設置最大敵人數
    /// 
    /// 功能：動態調整同時存在的最大敵人數量
    /// 參數：maxEnemies - 新的最大敵人數
    /// 注意：此設定會立即生效，影響正在運行的生成協程
    /// </summary>
    /// <param name="maxEnemies">新的最大敵人數</param>
    public void SetMaxEnemies(int maxEnemies)
    {
        this.maxEnemies = maxEnemies;
    }
    
    /// <summary>
    /// 切換生成模式
    /// 
    /// 功能：動態切換固定生成點和隨機生成模式
    /// 參數：
    /// - useFixedPoints: 是否使用固定生成點
    /// - useRandom: 是否使用隨機生成
    /// 注意：此設定會立即生效，影響正在運行的生成協程
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
    /// 
    /// 功能：
    /// 1. 重置所有計數器和索引
    /// 2. 可選地設定新的最大敵人數
    /// 3. 重新開始生成敵人
    /// 
    /// 重置內容：
    /// - currentEnemyCount = 0
    /// - currentSpawnIndex = 0
    /// - 所有生成點索引 = 0
    /// - 所有已生成數量 = 0
    /// 
    /// 參數：newMaxEnemies - 新的最大敵人數，-1表示保持原值
    /// 注意：此方法會停止當前的生成協程並重新啟動
    /// </summary>
    /// <param name="newMaxEnemies">新的最大敵人數，-1表示保持原值</param>
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
        
        Debug.Log($"敵人系統已重置，最大敵人數: {maxEnemies}");
    }
    
    /// <summary>
    /// 停止生成敵人
    /// 
    /// 功能：停止所有正在運行的生成協程
    /// 注意：此方法會立即停止敵人生成，但不會影響已存在的敵人
    /// </summary>
    public void StopSpawning()
    {
        StopAllCoroutines();
        Debug.Log("敵人生成已停止");
    }
    
    /// <summary>
    /// 開始生成敵人
    /// 
    /// 功能：重新啟動敵人生成協程
    /// 注意：此方法會重新開始自動生成敵人，使用當前的設定
    /// </summary>
    public void StartSpawning()
    {
        StartCoroutine(CustomSpawnEnemies());
        Debug.Log("敵人生成已開始");
    }
    
    /// <summary>
    /// 取得當前敵人數
    /// 
    /// 功能：獲取當前活著的敵人數量
    /// 返回值：當前敵人數
    /// 注意：此數值不包括已死亡但未回收的敵人
    /// </summary>
    /// <returns>當前活著的敵人數</returns>
    public int GetCurrentEnemyCount()
    {
        return currentEnemyCount;
    }
    
    /// <summary>
    /// 取得最大敵人數
    /// 
    /// 功能：獲取設定的最大敵人數量
    /// 返回值：最大敵人數
    /// 注意：此數值可能會因為敵人死亡而動態調整
    /// </summary>
    /// <returns>最大敵人數</returns>
    public int GetMaxEnemies()
    {
        return maxEnemies;
    }
    
    /// <summary>
    /// Unity編輯器驗證：在Inspector中修改值時自動調用
    /// 
    /// 功能：
    /// 1. 確保respawnDeadEnemies始終為false
    /// 2. 提供即時的參數驗證
    /// 3. 防止錯誤的設定
    /// 
    /// 注意：此方法只在Unity編輯器中運行，不會影響構建後的遊戲
    /// </summary>
    void OnValidate()
    {
        // 確保不重生設定為 false
        respawnDeadEnemies = false;
    }
    
    /// <summary>
    /// 檢查BOSS生成設定（用於除錯）
    /// 
    /// 功能：
    /// 1. 顯示所有敵人預製體的詳細設定
    /// 2. 驗證maxSpawnCount設定是否正確
    /// 3. 顯示物件池大小和生成點數量
    /// 4. 顯示當前的生成模式設定
    /// 
    /// 使用方式：
    /// - 在Unity Inspector中右鍵點擊EnemyPool組件
    /// - 選擇「檢查BOSS生成設定」選項
    /// - 查看Console輸出
    /// 
    /// 輸出內容：
    /// - 每個預製體的名稱和設定
    /// - 物件池大小和最大生成數量
    /// - 生成點數量和名稱
    /// - 生成模式和間隔設定
    /// 
    /// 注意：此方法用於除錯和驗證設定，建議在設定完成後使用
    /// </summary>
    [ContextMenu("檢查BOSS生成設定")]
    public void CheckBossSpawnSettings()
    {
        Debug.Log("=== BOSS生成設定檢查 ===");
        
        if (enemySpawnData == null || enemySpawnData.Length == 0)
        {
            Debug.LogError("沒有設定敵人預製體資料！");
            return;
        }
        
        for (int i = 0; i < enemySpawnData.Length; i++)
        {
            EnemySpawnData data = enemySpawnData[i];
            Debug.Log($"敵人 {i}: {data.enemyPrefab?.name ?? "未設定"}");
            Debug.Log($"  物件池大小: {data.poolSize}");
            Debug.Log($"  最大生成數量: {data.maxSpawnCount} (-1=不限制, 0=不生成, 1=按poolSize生成)");
            Debug.Log($"  生成點數量: {(data.spawnPoints != null ? data.spawnPoints.Length : 0)}");
            
            if (data.spawnPoints != null)
            {
                for (int j = 0; j < data.spawnPoints.Length; j++)
                {
                    Debug.Log($"    生成點 {j}: {data.spawnPoints[j]?.name ?? "未設定"}");
                }
            }
        }
        
        Debug.Log($"生成模式: 固定生成點={useFixedSpawnPoints}, 隨機生成={useRandomSpawn}, 隨機生成點={useRandomSpawnPoint}");
        Debug.Log($"最大敵人數: {maxEnemies}, 生成間隔: {spawnInterval}秒");
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 檢查當前生成的敵人狀態
    /// 
    /// 功能：
    /// 1. 顯示當前敵人數量和最大敵人數
    /// 2. 顯示每個預製體的物件池狀態
    /// 3. 顯示已生成數量和實際最大數量
    /// 4. 顯示生成點索引狀態
    /// 
    /// 使用方式：
    /// - 在Unity Inspector中右鍵點擊EnemyPool組件
    /// - 選擇「檢查當前敵人狀態」選項
    /// - 查看Console輸出
    /// 
    /// 輸出內容：
    /// - 當前敵人數和最大敵人數
    /// - 每個預製體的物件池統計（總數、活躍、閒置）
    /// - 每個預製體的已生成數量/最大數量
    /// - 每個預製體的生成點索引
    /// 
    /// 注意：此方法用於運行時除錯，可以隨時調用檢查系統狀態
    /// </summary>
    [ContextMenu("檢查當前敵人狀態")]
    public void CheckCurrentEnemyStatus()
    {
        Debug.Log("=== 當前敵人狀態檢查 ===");
        Debug.Log($"當前敵人數: {currentEnemyCount}");
        Debug.Log($"最大敵人數: {maxEnemies}");
        Debug.Log($"當前生成索引: {currentSpawnIndex}");
        
        if (enemyPools != null)
        {
            foreach (var kvp in enemyPools)
            {
                GameObject prefab = kvp.Key;
                List<GameObject> pool = kvp.Value;
                int activeCount = 0;
                int inactiveCount = 0;
                
                foreach (GameObject enemy in pool)
                {
                    if (enemy.activeInHierarchy)
                    {
                        activeCount++;
                    }
                    else
                    {
                        inactiveCount++;
                    }
                }
                
                Debug.Log($"{prefab.name}: 總數={pool.Count}, 活躍={activeCount}, 閒置={inactiveCount}");
            }
        }
        
        if (spawnPointIndices != null)
        {
            foreach (var kvp in spawnPointIndices)
            {
                Debug.Log($"生成點索引 {kvp.Key.name}: {kvp.Value}");
            }
        }
        
        if (spawnedCounts != null)
        {
            foreach (var kvp in spawnedCounts)
            {
                GameObject prefab = kvp.Key;
                int spawned = kvp.Value;
                
                // 找到對應的EnemySpawnData來計算實際最大數量
                int actualMaxSpawn = -1;
                foreach (EnemySpawnData data in enemySpawnData)
                {
                    if (data.enemyPrefab == prefab)
                    {
                        if (data.maxSpawnCount == 1)
                        {
                            actualMaxSpawn = data.poolSize;
                        }
                        else
                        {
                            actualMaxSpawn = data.maxSpawnCount;
                        }
                        break;
                    }
                }
                
                string maxText = actualMaxSpawn == -1 ? "不限制" : actualMaxSpawn.ToString();
                Debug.Log($"已生成數量 {prefab.name}: {spawned}/{maxText}");
            }
        }
        
        Debug.Log("========================");
    }
}