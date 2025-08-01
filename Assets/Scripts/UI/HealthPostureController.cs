using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// 生命值與架勢條控制器
public class HealthPostureController : MonoBehaviour
{
    public int live = 1;   // 復活次數
    [SerializeField] private int maxLive = 1; // 最大生命球數
    [SerializeField] private int maxHealth = 100;   // 最大生命值
    [SerializeField] private int maxPosture = 100;  // 最大架勢值
    [SerializeField] public HealthPostureUI healthPostureUI;   // 生命值與架勢 UI 顯示
    [SerializeField] private GameObject deathUI;   // 死亡UI
    [SerializeField] private GameObject reviveEffectPrefab;   // 復活特效Prefab
    [SerializeField] private Transform reviveEffectPosition;   // 復活特效生成位置

    private HealthPostureSystem healthPostureSystem;    // 引用生命值與架勢系統
    private Coroutine hideUICoroutine;  // 隱藏 UI 的協程
    private static HealthPostureController lastAttackedEnemy;  // 最後一個被攻擊的敵人
    private bool colliderDisabled = false;  // 標記碰撞器是否已被關閉
    private bool isPlayerDead = false;  // 玩家是否已死亡
    [HideInInspector] public bool canIncreasePosture = true;  // 是否可以增加架勢值
    private bool canRevive = false; // 是否可復活
    private Coroutine deathCountdownCoroutine; // 死亡倒數協程
    Animator playerAnimator;

    void Awake()
    {
        // 檢查是否為玩家，如果是則調用玩家初始化
        if (GetComponent<PlayerStatus>() != null)
        {
            InitPlayer();
        }
        else
        {
            // 敵人初始化邏輯
            // 自動檢測並設定最大生命值
            SetMaxHealthBasedOnComponent();

            // 初始化生命值與架勢系統
            healthPostureSystem = new HealthPostureSystem(maxHealth, maxPosture);

            // 檢查是否為Boss，如果是則確保血條UI正確設置
            bool isBoss = IsBoss();
            if (isBoss)
            {
                Debug.Log($"[HealthPostureController] {gameObject.name} 是 Boss，檢查血條UI設置");
                
                // 檢查血條UI引用
                if (healthPostureUI == null)
                {
                    Debug.LogWarning($"[HealthPostureController] Boss {gameObject.name} 的 healthPostureUI 為 null，嘗試修復");
                    
                    // 嘗試在Boss物件下找到血條UI
                    HealthPostureUI[] healthUIs = GetComponentsInChildren<HealthPostureUI>(true);
                    if (healthUIs.Length > 0)
                    {
                        healthPostureUI = healthUIs[0];
                        Debug.Log($"[HealthPostureController] 已修復 Boss 血條UI引用: {healthUIs[0].gameObject.name}");
                    }
                    else
                    {
                        Debug.LogError($"[HealthPostureController] 無法在 Boss {gameObject.name} 下找到血條UI");
                    }
                }
                
                // 不在此時啟用Boss的血條UI，讓它只在進入觸發區域時才顯示
                if (healthPostureUI != null)
                {
                    Debug.Log($"[HealthPostureController] Boss 血條UI引用已設置: {healthPostureUI.gameObject.name}，但暫時不啟用");
                }
            }

            // 初始化 HealthPostureUI
            if (healthPostureUI != null)
            {
                healthPostureUI.SetHealthPostureSystem(healthPostureSystem);
                healthPostureUI.UpdateLifeBalls(live, maxLive);
                
                // 如果是Boss，確保血條UI在初始化時是隱藏的
                if (isBoss)
                {
                    healthPostureUI.gameObject.SetActive(false);
                    Debug.Log($"[HealthPostureController] Boss 血條UI已隱藏: {healthPostureUI.gameObject.name}");
                }
            }

            // 訂閱事件
            healthPostureSystem.OnDead += OnDead;
            healthPostureSystem.OnPostureBroken += OnPostureBroken;
            live = maxLive;

            playerAnimator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // 檢查是否為玩家且已死亡
        bool isPlayer = GetComponent<PlayerStatus>() != null;

        if (isPlayer && isPlayerDead)
        {
            // 只有可復活時才可操作
            if (canRevive && Input.GetMouseButtonDown(0))
            {
                if (live > 0)
                {
                    ResurrectPlayer();
                }
                else
                {
                    ReturnToMainMenu();
                }
            }
        }
    }

    // 初始化玩家血條架勢條
    public void InitPlayer()
    {
        // 檢查是否有 PlayerStatus 組件（玩家）
        if (GetComponent<PlayerStatus>() != null)
        {
            // 初始化生命球數
            live = maxLive;
            
            // 初始化生命值
            maxHealth = PlayerStatus.maxHP;
            
            // 初始化架勢值
            maxPosture = 100; // 玩家最大架勢值預設為100
            
            // 設定生命值與架勢系統
            healthPostureSystem = new HealthPostureSystem(maxHealth, maxPosture);
            
            // 設定生命值與架勢 UI
            if (healthPostureUI != null)
            {
                healthPostureUI.SetHealthPostureSystem(healthPostureSystem);
                healthPostureUI.UpdateLifeBalls(live, maxLive);
            }
            
            // 訂閱事件
            healthPostureSystem.OnDead += OnDead;
            healthPostureSystem.OnPostureBroken += OnPostureBroken;
            
            // 初始化玩家動畫器
            playerAnimator = GetComponent<Animator>();
            
            // 確保玩家初始狀態正常
            isPlayerDead = false;
            canIncreasePosture = true;
            canRevive = false;
            
            Debug.Log($"[玩家初始化] 生命值: {maxHealth}, 架勢值: {maxPosture}, 復活次數: {live}");
        }
    }

    // 根據組件類型自動設定最大生命值
    private void SetMaxHealthBasedOnComponent()
    {
        // 檢查是否有 PlayerStatus 組件（玩家）
        if (GetComponent<PlayerStatus>() != null)
        {
            maxHealth = PlayerStatus.maxHP;
        }
        // 檢查是否有 EnemyTest 組件（敵人）
        else if (GetComponent<EnemyTest>() != null)
        {
            maxHealth = EnemyTest.maxHP;
        }
        // 如果都沒有，使用預設值
    }

    // 受到傷害
    public void TakeDamage(int amount, PlayerStatus.HitState hitState)
    {
        // 檢查 healthPostureSystem 是否已初始化
        if (healthPostureSystem == null)
        {
            Debug.LogWarning("[架勢控制器] healthPostureSystem 尚未初始化，跳過傷害處理");
            return;
        }

        healthPostureSystem.HealthDamage(amount);

        // 檢查是否可以增加架勢值
        if (canIncreasePosture)
        {
            // 檢查是否有剛幹糖效果
            float postureReductionRate = 1f;
            ItemSystem itemSystem = GetComponent<ItemSystem>();
            if (itemSystem != null)
            {
                postureReductionRate = itemSystem.GetPostureReductionRate();
            }
            
            // 根據攻擊狀態決定架勢增加量（基於最大架勢值的百分比）
            int basePostureAmount = 0;
            bool isParry = (hitState == PlayerStatus.HitState.Parry);
            
            switch (hitState)
            {
                case PlayerStatus.HitState.Hit:
                    basePostureAmount = Mathf.RoundToInt(healthPostureSystem.GetMaxPosture() * 0.3f); 
                    break;
                case PlayerStatus.HitState.Guard:
                    basePostureAmount = Mathf.RoundToInt(healthPostureSystem.GetMaxPosture() * 0.1f); 
                    break;
                case PlayerStatus.HitState.Parry:
                    // 當玩家處於Parry狀態時，不增加架勢值
                    // 因為玩家Parry敵人時，只有被Parry的敵人應該增加架勢值
                    basePostureAmount = 0; 
                    break;
            }
            
            // 根據剛幹糖效果調整架勢增加量
            int adjustedPostureAmount = Mathf.RoundToInt(basePostureAmount * postureReductionRate);
            
            healthPostureSystem.PostureIncrease(adjustedPostureAmount, isParry);
        }
        // 顯示血條並設定為最後一個被攻擊的敵人
        ShowHealthBar();
    }

    // 增加架勢
    public void AddPosture(int amount, bool isParry = false)
    {
        // 檢查 healthPostureSystem 是否已初始化
        if (healthPostureSystem == null)
        {
            return;
        }

        // 檢查是否可以增加架勢值
        if (!canIncreasePosture)
        {
            return; // 如果架勢被打破，暫時不能增加架勢值
        }

        // 記錄架勢值增加前的狀態
        float previousPosturePercentage = healthPostureSystem.GetPostureNormalized();
        int previousPostureAmount = Mathf.RoundToInt(previousPosturePercentage * healthPostureSystem.GetMaxPosture());

        healthPostureSystem.PostureIncrease(amount, isParry);

        // 記錄架勢值增加後的狀態
        float currentPosturePercentage = healthPostureSystem.GetPostureNormalized();
        int currentPostureAmount = Mathf.RoundToInt(currentPosturePercentage * healthPostureSystem.GetMaxPosture());

        // 顯示血條並設定為最後一個被攻擊的敵人
        ShowHealthBar();
    }
    // 顯示血條
    private void ShowHealthBar()
    {
        Debug.Log($"[HealthPostureController] ShowHealthBar 被調用，物件: {gameObject.name}");
        
        // 檢查 healthPostureSystem 是否已初始化
        if (healthPostureSystem == null)
        {
            Debug.LogWarning($"HealthPostureController: healthPostureSystem 為 null，無法顯示血條，物件: {gameObject.name}");
            return;
        }
        
        // 檢查血量是否小於等於 0，如果是則不顯示血條
        if (healthPostureSystem.GetHealthNormalized() <= 0)
        {
            return;
        }
        // 檢查是否為玩家
        bool isPlayer = GetComponent<PlayerStatus>() != null;
        if (isPlayer)
        {
            // 檢查架勢值是否大於50%，如果是則常駐顯示
            float posturePercentage = healthPostureSystem.GetPostureNormalized();
            if (posturePercentage > 0.5f)
            {
                // 架勢值大於50%時常駐顯示，停止隱藏協程
                if (hideUICoroutine != null)
                {
                    StopCoroutine(hideUICoroutine);
                    hideUICoroutine = null;
                }
            }
            
            // 玩家血條始終顯示，不需要隱藏邏輯
            if (healthPostureUI != null)
            {
                healthPostureUI.gameObject.SetActive(true);
            }
        }
        else
        {
            // 檢查是否為Boss（通過檢查是否有BossTriggerZone引用）
            bool isBoss = IsBoss();
            
            if (isBoss)
            {
                // Boss血條邏輯：檢查玩家是否在Boss區域內
                bool isPlayerInBossZone = IsPlayerInBossZone();
                Debug.Log($"[HealthPostureController] {gameObject.name} 是 Boss，玩家在區域內: {isPlayerInBossZone}");
                
                if (isPlayerInBossZone)
                {
                    // 玩家在Boss區域內，顯示血條
                    if (healthPostureUI != null)
                    {
                        healthPostureUI.gameObject.SetActive(true);
                        Debug.Log($"[HealthPostureController] {gameObject.name} 顯示 Boss 血條");
                    }
                    
                    // 停止隱藏協程
                    if (hideUICoroutine != null)
                    {
                        StopCoroutine(hideUICoroutine);
                        hideUICoroutine = null;
                    }
                    
                    // 設定為最後一個被攻擊的敵人，防止被其他敵人覆蓋
                    lastAttackedEnemy = this;
                }
                else
                {
                    // 玩家不在Boss區域內，隱藏血條
                    if (healthPostureUI != null)
                    {
                        healthPostureUI.gameObject.SetActive(false);
                        Debug.Log($"[HealthPostureController] {gameObject.name} 隱藏 Boss 血條");
                    }
                }
            }
            else
            {
                // 普通敵人血條邏輯：隱藏其他敵人的血條
                if (lastAttackedEnemy != null && lastAttackedEnemy != this)
                {
                    // 檢查之前的敵人是否為 Boss，如果是則不隱藏
                    bool isPreviousEnemyBoss = lastAttackedEnemy.IsBoss();
                    Debug.Log($"[HealthPostureController] {gameObject.name} 嘗試隱藏之前的敵人 {lastAttackedEnemy.gameObject.name}，是否為 Boss: {isPreviousEnemyBoss}");
                    if (!isPreviousEnemyBoss)
                    {
                        lastAttackedEnemy.HideHealthBar();
                    }
                    else
                    {
                        Debug.Log($"[HealthPostureController] {gameObject.name} 跳過隱藏 Boss 血條: {lastAttackedEnemy.gameObject.name}");
                    }
                }

                // 設定為最後一個被攻擊的敵人
                lastAttackedEnemy = this;

                // 顯示血條 UI
                if (healthPostureUI != null)
                {
                    healthPostureUI.gameObject.SetActive(true);
                }

                // 停止之前的隱藏協程（如果有的話）
                if (hideUICoroutine != null)
                {
                    StopCoroutine(hideUICoroutine);
                }

                // 開始新的隱藏協程
                if (gameObject.activeInHierarchy)
                {
                    hideUICoroutine = StartCoroutine(HideHealthBarAfterDelay(5f));
                }
            }
        }
    }

    // 隱藏血條
    public void HideHealthBar()
    {
        Debug.Log($"[HealthPostureController] HideHealthBar 被調用，物件: {gameObject.name}");
        
        // 檢查是否為玩家
        bool isPlayer = GetComponent<PlayerStatus>() != null;

        if (isPlayer)
        {
            // 檢查 healthPostureSystem 是否已初始化
            if (healthPostureSystem == null)
            {
                Debug.LogWarning($"HealthPostureController: healthPostureSystem 為 null，無法檢查架勢值，物件: {gameObject.name}");
                return;
            }
            
            // 檢查架勢值是否大於50%，如果是則不隱藏
            float posturePercentage = healthPostureSystem.GetPostureNormalized();
            if (posturePercentage > 0.5f)
            {
                // 架勢值大於50%時不隱藏，只停止協程
                if (hideUICoroutine != null)
                {
                    StopCoroutine(hideUICoroutine);
                    hideUICoroutine = null;
                }
                return;
            }
            
            // 玩家血條不隱藏，只停止協程
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
                hideUICoroutine = null;
            }
            return;
        }

        // 檢查是否為 Boss，如果是則記錄但不阻止隱藏血條
        bool isBoss = IsBoss();
        if (isBoss)
        {
            Debug.Log($"[HealthPostureController] {gameObject.name} 是 Boss，允許隱藏血條");
        }

        // 敵人血條隱藏邏輯
        if (healthPostureUI != null)
        {
            healthPostureUI.gameObject.SetActive(false);
        }

        // 停止隱藏協程
        if (hideUICoroutine != null)
        {
            StopCoroutine(hideUICoroutine);
            hideUICoroutine = null;
        }

        // 如果不是最後一個被攻擊的敵人，清除引用
        if (lastAttackedEnemy == this)
        {
            lastAttackedEnemy = null;
        }
    }

    // 確保 GameObject 及其所有父物件都是啟用的
    private void EnsureGameObjectHierarchyActive(GameObject targetObject)
    {
        if (targetObject == null) return;
        
        // 從目標物件開始，向上遍歷所有父物件，確保它們都是啟用的
        Transform current = targetObject.transform;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                Debug.Log($"[HealthPostureController] 啟用父物件: {current.gameObject.name}");
                current.gameObject.SetActive(true);
            }
            current = current.parent;
        }
        
        Debug.Log($"[HealthPostureController] 確保 {targetObject.name} 的整個層級都是啟用的");
    }

    // 強制顯示血條（用於Boss）
    public void ForceShowHealthBar()
    {
        Debug.Log($"[HealthPostureController] ForceShowHealthBar 被調用: {gameObject.name}");
        
        if (healthPostureUI == null)
        {
            Debug.LogWarning($"[HealthPostureController] healthPostureUI 為 null: {gameObject.name}");
            
            // 嘗試自動找到血條UI
            HealthPostureUI[] healthUIs = GetComponentsInChildren<HealthPostureUI>(true);
            if (healthUIs.Length > 0)
            {
                healthPostureUI = healthUIs[0];
                Debug.Log($"[HealthPostureController] 自動找到血條UI: {healthUIs[0].gameObject.name}");
            }
            else
            {
                Debug.LogError($"[HealthPostureController] 無法找到血條UI: {gameObject.name}");
            return;
            }
        }
        
        // 確保整個層級都是啟用的
        EnsureGameObjectHierarchyActive(gameObject);
        EnsureGameObjectHierarchyActive(healthPostureUI.gameObject);
        
        Debug.Log($"[HealthPostureController] 強制顯示血條: {gameObject.name}");
        Debug.Log($"[HealthPostureController] healthPostureUI GameObject 狀態: activeInHierarchy={healthPostureUI.gameObject.activeInHierarchy}, activeSelf={healthPostureUI.gameObject.activeSelf}");
        
        // 遞迴啟用所有父物件
        Transform current = healthPostureUI.transform;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                Debug.Log($"[HealthPostureController] 啟用父物件: {current.gameObject.name}");
                current.gameObject.SetActive(true);
            }
            current = current.parent;
        }
        
        // 啟用血條 UI
        healthPostureUI.gameObject.SetActive(true);
        
        Debug.Log($"[HealthPostureController] 血條 UI 已啟用: {healthPostureUI.gameObject.name}");
        Debug.Log($"[HealthPostureController] 血條 UI 最終狀態: activeInHierarchy={healthPostureUI.gameObject.activeInHierarchy}, activeSelf={healthPostureUI.gameObject.activeSelf}");
        
        // 對於 Boss，直接確保血條顯示，不依賴協程
        if (IsBoss())
        {
            Debug.Log($"[HealthPostureController] Boss {gameObject.name} 血條顯示完成，跳過協程檢查");
            
            // 再次確保血條 UI 是啟用的
            if (healthPostureUI != null)
            {
                healthPostureUI.gameObject.SetActive(true);
                bool finalActive = healthPostureUI.gameObject.activeInHierarchy;
                Debug.Log($"[HealthPostureController] Boss 血條最終狀態: {(finalActive ? "啟用" : "禁用")}");
            }
        }
        else
        {
            // 對於非 Boss，使用協程檢查狀態
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(CheckHealthBarStateAfterFrame());
        }
        else
        {
            Debug.LogWarning($"[HealthPostureController] GameObject {gameObject.name} 未啟用，無法啟動 Coroutine");
            }
        }
    }
    
    private IEnumerator CheckHealthBarStateAfterFrame()
    {
        yield return null; // 等待一幀
        
        if (healthPostureUI != null)
        {
            bool isActive = healthPostureUI.gameObject.activeInHierarchy;
            Debug.Log($"[HealthPostureController] 一幀後血條狀態: {(isActive ? "啟用" : "禁用")}");
            
            if (!isActive)
            {
                Debug.LogWarning($"[HealthPostureController] 警告：血條仍然被禁用！可能被其他腳本干擾");
                
                // 再次嘗試啟用
                healthPostureUI.gameObject.SetActive(true);
                yield return null;
                
                bool finalActive = healthPostureUI.gameObject.activeInHierarchy;
                Debug.Log($"[HealthPostureController] 最終血條狀態: {(finalActive ? "啟用" : "禁用")}");
            }
        }
    }

    // 檢查玩家是否在Boss區域內
    private bool IsPlayerInBossZone()
    {
        // 找到所有BossTriggerZone
        BossTriggerZone[] bossZones = FindObjectsOfType<BossTriggerZone>();
        
        foreach (BossTriggerZone zone in bossZones)
        {
            // 檢查這個BossTriggerZone是否與當前Boss相關聯
            if (zone.bossObject != null)
            {
                string currentName = gameObject.name.Replace("(Clone)", "");
                string bossObjectName = zone.bossObject.name.Replace("(Clone)", "");
                
                if (currentName == bossObjectName || 
                    (bossObjectName == "Elite" || bossObjectName == "Boss") && 
                    (currentName == "Elite" || currentName == "Boss"))
                {
                    // 使用BossTriggerZone的IsPlayerInZone方法
                    if (zone.IsPlayerInZone())
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }

    // 檢查是否為Boss
    public bool IsBoss()
    {
        // 檢查是否有BossTriggerZone引用這個物件
        BossTriggerZone[] bossZones = FindObjectsOfType<BossTriggerZone>();
        Debug.Log($"[HealthPostureController] {gameObject.name} 檢查是否為 Boss，找到 {bossZones.Length} 個 BossTriggerZone");
        
        foreach (BossTriggerZone zone in bossZones)
        {
            Debug.Log($"[HealthPostureController] 檢查 BossTriggerZone: {zone.name}, bossObject: {zone.bossObject?.name ?? "null"}");
            // 檢查 zone.bossObject 是否為 null
            if (zone.bossObject != null)
            {
                // 檢查是否是同一個 Prefab 的實例
                bool isSamePrefab = false;
                
                // 方法1: 檢查名稱是否匹配（去掉Clone後綴）
                string currentName = gameObject.name.Replace("(Clone)", "");
                string bossObjectName = zone.bossObject.name.Replace("(Clone)", "");
                Debug.Log($"[HealthPostureController] 比較名稱: 當前={currentName}, bossObject={bossObjectName}");
                if (currentName == bossObjectName)
                {
                    isSamePrefab = true;
                    Debug.Log($"[HealthPostureController] 通過名稱匹配確認為同一 Prefab: {currentName}");
                }
                
                // 方法2: 檢查 HealthPostureController 實例比較
                HealthPostureController zoneBossController = zone.bossObject.GetComponent<HealthPostureController>();
                if (zoneBossController == this)
                {
                    isSamePrefab = true;
                    Debug.Log($"[HealthPostureController] 通過實例比較確認為同一 Prefab");
                }
                
                // 方法3: 檢查是否是精確的 Boss 類型匹配（移除過於寬鬆的檢查）
                // 只進行精確的名稱匹配，避免錯誤識別
                
                if (isSamePrefab)
                {
                    Debug.Log($"[HealthPostureController] {gameObject.name} 被確認為 Boss！");
                    return true;
                }
            }
        }
        Debug.Log($"[HealthPostureController] {gameObject.name} 不是 Boss");
        return false;
    }

    // 通知Boss死亡
    private void NotifyBossDeath()
    {
        Debug.Log($"[HealthPostureController] {gameObject.name} 開始通知 Boss 死亡");
        
        // 找到所有Boss觸發區域並通知Boss死亡
        BossTriggerZone[] bossZones = FindObjectsOfType<BossTriggerZone>();
        Debug.Log($"[HealthPostureController] 找到 {bossZones.Length} 個 BossTriggerZone");
        
        bool foundMatchingZone = false;
        foreach (BossTriggerZone zone in bossZones)
        {
            Debug.Log($"[HealthPostureController] 檢查 BossTriggerZone: {zone.name}, bossObject: {zone.bossObject?.name ?? "null"}");
            // 檢查 zone.bossObject 是否為 null
            if (zone.bossObject != null)
            {
                // 檢查是否是同一個 Prefab 的實例
                bool isSamePrefab = false;
                
                // 方法1: 檢查名稱是否匹配（去掉Clone後綴）
                string currentName = gameObject.name.Replace("(Clone)", "");
                string bossObjectName = zone.bossObject.name.Replace("(Clone)", "");
                Debug.Log($"[HealthPostureController] 比較名稱: 當前={currentName}, bossObject={bossObjectName}");
                if (currentName == bossObjectName)
                {
                    isSamePrefab = true;
                    Debug.Log($"[HealthPostureController] 通過名稱匹配確認為同一 Prefab: {currentName}");
                }
                
                // 方法2: 檢查 HealthPostureController 實例比較
                HealthPostureController zoneBossController = zone.bossObject.GetComponent<HealthPostureController>();
                if (zoneBossController == this)
                {
                    isSamePrefab = true;
                    Debug.Log($"[HealthPostureController] 通過實例比較確認為同一 Prefab");
                }
                
                // 方法3: 檢查是否是任何 Boss 類型（包括 Elite）
                if (bossObjectName == "Elite" || bossObjectName == "Boss")
                {
                    // 如果當前物件是 Elite 或 Boss 的實例，也認為是 Boss
                    if (currentName == "Elite" || currentName == "Boss")
                    {
                        isSamePrefab = true;
                        Debug.Log($"[HealthPostureController] 通過 Boss 類型匹配確認為 Boss: {currentName}");
                    }
                }
                
                if (isSamePrefab)
                {
                    Debug.Log($"[HealthPostureController] 找到匹配的 BossTriggerZone: {zone.name}，調用 OnBossDeath");
                    zone.OnBossDeath();
                    foundMatchingZone = true;
                    break;
                }
            }
        }
        
        if (!foundMatchingZone)
        {
            Debug.LogWarning($"[HealthPostureController] {gameObject.name} 死亡時沒有找到對應的 BossTriggerZone！");
        }
    }

    // 延遲隱藏血條的協程
    private IEnumerator HideHealthBarAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 只有當這個敵人仍然是最後一個被攻擊的敵人時才隱藏
        if (lastAttackedEnemy == this)
        {
            HideHealthBar();
        }
    }

    // 死亡
    private void OnDead(object sender, System.EventArgs e)
    {
        Debug.Log($"[HealthPostureController] {gameObject.name} 死亡事件觸發");
        
        // 檢查是否為玩家
        bool isPlayer = GetComponent<PlayerStatus>() != null;

        if (isPlayer)
        {
            Debug.Log($"[HealthPostureController] {gameObject.name} 是玩家，執行玩家死亡邏輯");
            // 設定玩家死亡狀態
            isPlayerDead = true;
            isPlayerDead = true;
            
            // 移除所有道具效果
            RemoveAllItemEffects();
            
            // 玩家死亡時延後一秒顯示死亡 UI
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ShowDeathUIAfterDelay(1f));
            }
        }
        else
        {
            Debug.Log($"[HealthPostureController] {gameObject.name} 是敵人，檢查是否為 Boss");
            // 檢查是否為Boss
            bool isBoss = IsBoss();
            
            if (isBoss)
            {
                Debug.Log($"[HealthPostureController] {gameObject.name} 是 Boss，執行 Boss 死亡邏輯");
                // Boss死亡時隱藏血條並通知Boss觸發區域
                HideHealthBar();
                NotifyBossDeath();
            }
            else
            {
                Debug.Log($"[HealthPostureController] {gameObject.name} 是普通敵人，執行普通敵人死亡邏輯");
                // 普通敵人死亡時隱藏血條
                HideHealthBar();
            }
            
            // 關閉碰撞器
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(DisableColliderAfterEffect());
            }
        }
        if (healthPostureUI != null)
            healthPostureUI.UpdateLifeBalls(live, maxLive);
    }
    // 失衡
    private void OnUnbalance(object sender, System.EventArgs e)
    {
        // 移除玩家操控
        PlayerStatus playerStatus = GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.Sragger();
        }        
        // 移除敵人操控
        EnemyTest enemyTest = GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.RemoveControl();
        }
    }

    // 延遲顯示死亡 UI 的協程
    private IEnumerator ShowDeathUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowDeathUI();
        // 玩家死亡UI顯示後啟動倒數
        if (GetComponent<PlayerStatus>() != null)
        {
            if (deathCountdownCoroutine != null)
                StopCoroutine(deathCountdownCoroutine);
            if (gameObject.activeInHierarchy)
            {
                deathCountdownCoroutine = StartCoroutine(DeathReviveCountdown());
            }
        }
    }

    // 顯示死亡 UI
    private void ShowDeathUI()
    {
        if (deathUI != null)
        {
            deathUI.SetActive(true);
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(StartDeathUIAnimationNextFrame());
            }
            canRevive = true;
        }
    }

    private IEnumerator StartDeathUIAnimationNextFrame()
    {
        yield return null; // 等待一幀，確保物件已啟用
        DeathUI deathUIScript = deathUI.GetComponent<DeathUI>();
        if (deathUIScript != null)
        {
            deathUIScript.StartDeathAnimation();
        }
    }

    // 隱藏死亡 UI
    public void HideDeathUI()
    {
        if (deathUI != null)
        {
            // 獲取DeathUI腳本並停止動畫
            DeathUI deathUIScript = deathUI.GetComponent<DeathUI>();
            if (deathUIScript != null)
            {
                deathUIScript.StopAnimation();
                deathUIScript.ResetUI();
            }
            
            deathUI.SetActive(false);
        }
        canRevive = false;
        if (deathCountdownCoroutine != null)
        {
            StopCoroutine(deathCountdownCoroutine);
            deathCountdownCoroutine = null;
        }
    }

    // 復活玩家
    private void ResurrectPlayer()
    {
        
        // 減少一個復活次數
        live--;

        // 重置生命值系統
        ResetHealth();

        // 隱藏死亡UI
        HideDeathUI();

        // 重置死亡狀態
        isPlayerDead = false;

        // 重新開啟玩家控制器
        PlayerStatus playerStatus = GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.ResetHealth();
        }
        
        // 確保玩家可以正常移動和操作
        TPContraller tpController = GetComponent<TPContraller>();
        if (tpController != null)
        {
            // 重置任何可能影響移動的狀態
            // 這裡可以添加其他需要重置的組件
        }
        
        // 播放復活特效
        PlayReviveEffect();
        playerAnimator.SetBool("Death", false);
        Debug.Log("玩家復活完成，所有狀態已重置");
        if (healthPostureUI != null)
            healthPostureUI.UpdateLifeBalls(live, maxLive);
    }

    // 回到主選單
    private void ReturnToMainMenu()
    {
        Debug.Log("遊戲結束 - 回到主選單");
        SceneManager.LoadScene("Main Menu");
    }

    // 等待武器特效完成後關閉碰撞器
    private IEnumerator DisableColliderAfterEffect()
    {
        // 等待武器特效完成
        yield return new WaitForSeconds(0.1f);

        // 關掉敵人 collider
        DisableCollider();
    }

    // 立即關閉碰撞器
    public void DisableCollider()
    {
        if (colliderDisabled) return; // 避免重複關閉

        // 關閉敵人碰撞器
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null && enemyCollider.enabled) // 如果敵人碰撞器存在且啟用
        {
            enemyCollider.enabled = false;
            colliderDisabled = true;
        }
        
        // 關閉敵人 CharacterController
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null && characterController.enabled)
        {
            characterController.enabled = false;
        }
    }

    // 架勢被打破
    private void OnPostureBroken(object sender, System.EventArgs e)
    {
        // 檢查是否為敵人
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            // 敵人進入失衡狀態
            enemyAI.SwitchState(new StaggerState());
            Debug.Log($"[HealthPostureController] 敵人 {gameObject.name} 架勢滿，進入失衡狀態");
        }
        else
        {
            // 玩家架勢滿的處理（保持原有邏輯）
            OnUnbalance(sender, e);
        }
        
        // 禁用架勢增加
        canIncreasePosture = false;
        // 架勢條緩慢歸零（僅對玩家）
        if (enemyAI == null)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(RestorePostureAfterDelay(0.01f));
                StartCoroutine(RestoreControlAfterDelay(1f));
            }
        }
    }

    // 獲取當前生命值百分比
    public float GetHealthPercentage()
    {
        if (healthPostureSystem == null)
        {
            Debug.LogWarning($"HealthPostureController: healthPostureSystem 為 null，物件: {gameObject.name}");
            return 0f;
        }
        return healthPostureSystem.GetHealthNormalized();
    }

    // 獲取當前架勢值百分比
    public float GetPosturePercentage()
    {
        if (healthPostureSystem == null)
        {
            Debug.LogWarning($"HealthPostureController: healthPostureSystem 為 null，物件: {gameObject.name}");
            return 0f;
        }
        return healthPostureSystem.GetPostureNormalized();
    }

    // 重置生命值系統
    public void ResetHealth()
    {
        // 檢查是否為玩家
        bool isPlayer = GetComponent<PlayerStatus>() != null;

        if (isPlayer)
        {
            // 重新創建生命值系統
            healthPostureSystem = new HealthPostureSystem(maxHealth, maxPosture);

            // 重新設定 UI
            if (healthPostureUI != null)
            {
                healthPostureUI.SetHealthPostureSystem(healthPostureSystem);
                healthPostureUI.UpdateLifeBalls(live, maxLive);
            }

            // 重新訂閱事件
            healthPostureSystem.OnDead += OnDead;
            healthPostureSystem.OnPostureBroken += OnPostureBroken;

            // 玩家重置時不隱藏血條，只停止協程
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
                hideUICoroutine = null;
            }
        }
        else
        {
            // 檢查是否為 Boss
            bool isBoss = IsBoss();
            Debug.Log($"[HealthPostureController] ResetHealth - 物件: {gameObject.name}, 是否為 Boss: {isBoss}");
            
            if (!isBoss)
            {
                // 只有非 Boss 敵人才隱藏血條
                if (healthPostureUI != null)
                {
                    healthPostureUI.gameObject.SetActive(false);
                    Debug.Log($"[HealthPostureController] ResetHealth - 隱藏非 Boss 血條: {gameObject.name}");
                }
            }
            else
            {
                // Boss 血條在初始化時也應該隱藏，只有在玩家進入區域時才顯示
                if (healthPostureUI != null)
                {
                    healthPostureUI.gameObject.SetActive(false);
                    Debug.Log($"[HealthPostureController] ResetHealth - Boss 血條初始化時隱藏: {gameObject.name}");
                }
            }
            
            // 停止隱藏協程
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
                hideUICoroutine = null;
            }
        }

        // 重置碰撞器狀態
        colliderDisabled = false;
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
        
        // 重新啟用 CharacterController
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        if (healthPostureUI != null)
            healthPostureUI.UpdateLifeBalls(live, maxLive);
    }

    // 當物件被銷毀時清理
    private void OnDestroy()
    {
        // 如果這個敵人是最後一個被攻擊的敵人，清除引用
        if (lastAttackedEnemy == this)
        {
            lastAttackedEnemy = null;
        }
    }

    // 延遲恢復控制的協程
    private IEnumerator RestoreControlAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 恢復敵人操控
        EnemyTest enemyTest = GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.RestoreControl();
        }
    }

    // 延遲恢復架勢的協程
    private IEnumerator RestorePostureAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 開始緩慢回復架勢值到 0
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(GraduallyRestorePosture(1f)); // 3秒內緩慢回復到 0
        }
        
        // 重新啟用架勢增加
        canIncreasePosture = true;
    }

    // 緩慢回復架勢值的協程
    private IEnumerator GraduallyRestorePosture(float duration)
    {
        if (healthPostureSystem == null)
        {
            Debug.LogWarning($"HealthPostureController: healthPostureSystem 為 null，無法回復架勢值，物件: {gameObject.name}");
            yield break;
        }
        
        float startPosture = healthPostureSystem.GetPostureNormalized();
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // 使用平滑的插值函數
            float currentPosture = Mathf.Lerp(startPosture, 0f, progress);
            
            // 設定架勢值（需要添加一個方法來直接設定架勢值）
            SetPostureValue(currentPosture);
            
            yield return null;
        }
        
        // 確保最終值為 0
        SetPostureValue(0f);
    }

    // 播放復活特效
    private void PlayReviveEffect()
    {
        if (reviveEffectPrefab != null)
        {
            // 使用指定的特效生成位置，如果沒有設定則使用玩家位置
            Vector3 effectPosition;
            if (reviveEffectPosition != null)
            {
                effectPosition = reviveEffectPosition.position;
            }
            else
            {
                effectPosition = transform.position;
            }
            
            GameObject reviveEffect = Instantiate(reviveEffectPrefab, effectPosition, Quaternion.identity);
            
            // 3秒後自動銷毀特效
            Destroy(reviveEffect, 3f);
            
            Debug.Log("播放復活特效");
        }
        else
        {
            Debug.LogWarning("復活特效Prefab未設定");
        }
    }

    // 設定架勢值的方法
    public void SetPostureValue(float normalizedValue)
    {
        if (healthPostureSystem == null)
        {
            Debug.LogWarning($"HealthPostureController: healthPostureSystem 為 null，無法設定架勢值，物件: {gameObject.name}");
            return;
        }
        healthPostureSystem.SetPostureNormalized(normalizedValue);
    }
    
    // 設定生命值的方法
    public void SetHealthValue(float normalizedValue)
    {
        if (healthPostureSystem == null)
        {
            Debug.LogWarning($"HealthPostureController: healthPostureSystem 為 null，無法設定生命值，物件: {gameObject.name}");
            return;
        }
        healthPostureSystem.SetHealthNormalized(normalizedValue);
    }
    
    // 重置架勢值為0
    public void ResetPosture()
    {
        SetPostureValue(0f);
    }

    // 治療生命值
    public void HealHealth(int healAmount)
    {
        if (healthPostureSystem == null)
        {
            Debug.LogWarning($"HealthPostureController: healthPostureSystem 為 null，無法治療生命值，物件: {gameObject.name}");
            return;
        }
        healthPostureSystem.HealthHeal(healAmount);
    }

    // 移除所有道具效果
    private void RemoveAllItemEffects()
    {
        // 獲取道具系統組件
        ItemSystem itemSystem = GetComponent<ItemSystem>();
        if (itemSystem != null)
        {
            // 重置道具系統狀態（包括停止道具效果協程）
            itemSystem.ResetItemEffects();
            
            Debug.Log("[架勢控制器] 已移除所有道具效果");
        }
    }

    private IEnumerator DeathReviveCountdown()
    {
        canRevive = true;
        float timer = 10f;
        while (timer > 0f)
        {
            if (!isPlayerDead) yield break; // 已復活則結束
            timer -= Time.deltaTime;
            yield return null;
        }
        // 10秒到，鎖定復活
        canRevive = false;
        // 閃紅光
        DeathUI deathUIScript = deathUI.GetComponent<DeathUI>();
        if (deathUIScript != null)
        {
            deathUIScript.StartRedFlash();
        }
        // 5秒後回主選單
        yield return new WaitForSeconds(5f);
        ReturnToMainMenu();
    }
}
