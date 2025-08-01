using UnityEngine;

public class DangerousAttackTest : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode testDangerousAttackKey = KeyCode.T;
    public KeyCode testBossDangerousAttackKey = KeyCode.Y;
    public KeyCode resetTestKey = KeyCode.R;
    
    [Header("測試目標")]
    public EnemyAI testEnemy;
    public BossAI testBoss;
    public PlayerStatus testPlayer;
    
    [Header("測試結果")]
    public bool lastTestResult = false;
    public string lastTestMessage = "";
    
    void Start()
    {
        // 自動尋找測試目標
        if (testEnemy == null)
            testEnemy = FindObjectOfType<EnemyAI>();
        
        if (testBoss == null)
            testBoss = FindObjectOfType<BossAI>();
        
        if (testPlayer == null)
            testPlayer = FindObjectOfType<PlayerStatus>();
        
        Debug.Log("DangerousAttackTest: 測試系統已初始化");
        Debug.Log($"找到測試目標 - Enemy: {testEnemy != null}, Boss: {testBoss != null}, Player: {testPlayer != null}");
    }
    
    void Update()
    {
        // 測試一般敵人的危攻擊
        if (Input.GetKeyDown(testDangerousAttackKey))
        {
            TestDangerousAttack();
        }
        
        // 測試Boss的危攻擊
        if (Input.GetKeyDown(testBossDangerousAttackKey))
        {
            TestBossDangerousAttack();
        }
        
        // 重置測試
        if (Input.GetKeyDown(resetTestKey))
        {
            ResetTest();
        }
    }
    
    void TestDangerousAttack()
    {
        Debug.Log("=== 測試一般敵人危攻擊 ===");
        
        if (testEnemy == null)
        {
            Debug.LogError("沒有找到測試敵人！");
            lastTestResult = false;
            lastTestMessage = "沒有找到測試敵人";
            return;
        }
        
        // 檢查是否有DangerousAttackHandler組件
        DangerousAttackHandler handler = testEnemy.GetComponent<DangerousAttackHandler>();
        if (handler == null)
        {
            Debug.LogWarning("敵人沒有DangerousAttackHandler組件，正在添加...");
            handler = testEnemy.gameObject.AddComponent<DangerousAttackHandler>();
        }
        
        // 檢查是否有AnimationEventHandler組件
        AnimationEventHandler eventHandler = testEnemy.GetComponent<AnimationEventHandler>();
        if (eventHandler == null)
        {
            Debug.LogWarning("敵人沒有AnimationEventHandler組件，正在添加...");
            eventHandler = testEnemy.gameObject.AddComponent<AnimationEventHandler>();
        }
        
        // 切換到危攻擊狀態
        testEnemy.SwitchState(new DangerousAttackState());
        
        lastTestResult = true;
        lastTestMessage = "一般敵人危攻擊測試完成";
        Debug.Log("一般敵人危攻擊測試完成");
    }
    
    void TestBossDangerousAttack()
    {
        Debug.Log("=== 測試Boss危攻擊 ===");
        
        if (testBoss == null)
        {
            Debug.LogError("沒有找到測試Boss！");
            lastTestResult = false;
            lastTestMessage = "沒有找到測試Boss";
            return;
        }
        
        // 檢查Boss是否可以執行危攻擊
        bool canPerform = testBoss.CanPerformDangerousAttack();
        Debug.Log($"Boss是否可以執行危攻擊: {canPerform}");
        
        if (canPerform)
        {
            // 執行危攻擊
            testBoss.PerformDangerousAttack();
            lastTestResult = true;
            lastTestMessage = "Boss危攻擊測試完成";
            Debug.Log("Boss危攻擊測試完成");
        }
        else
        {
            Debug.LogWarning("Boss危攻擊仍在冷卻中");
            lastTestResult = false;
            lastTestMessage = "Boss危攻擊仍在冷卻中";
        }
    }
    
    void TestPlayerDangerousDamage()
    {
        Debug.Log("=== 測試玩家危攻擊傷害 ===");
        
        if (testPlayer == null)
        {
            Debug.LogError("沒有找到測試玩家！");
            return;
        }
        
        // 記錄玩家當前血量
        int currentHP = testPlayer.GetCurrentHP();
        Debug.Log($"玩家當前血量: {currentHP}");
        
        // 對玩家造成危攻擊傷害
        testPlayer.TakeDangerousDamage(30f);
        
        // 檢查血量變化
        int newHP = testPlayer.GetCurrentHP();
        Debug.Log($"玩家受傷後血量: {newHP}");
        
        if (newHP < currentHP)
        {
            Debug.Log("危攻擊傷害測試成功！");
        }
        else
        {
            Debug.LogWarning("危攻擊傷害測試失敗，血量沒有減少");
        }
    }
    
    void ResetTest()
    {
        Debug.Log("=== 重置測試 ===");
        
        // 重置敵人狀態
        if (testEnemy != null)
        {
            testEnemy.SwitchState(new IdleState());
        }
        
        if (testBoss != null)
        {
            testBoss.SwitchState(new BossIdleState());
        }
        
        // 重置玩家血量
        if (testPlayer != null)
        {
            testPlayer.ResetHealth();
        }
        
        lastTestResult = false;
        lastTestMessage = "測試已重置";
        Debug.Log("測試已重置");
    }
    
    void OnGUI()
    {
        // 顯示測試信息
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("危攻擊系統測試", GUI.skin.box);
        GUILayout.Label($"按 {testDangerousAttackKey} 測試一般敵人危攻擊");
        GUILayout.Label($"按 {testBossDangerousAttackKey} 測試Boss危攻擊");
        GUILayout.Label($"按 {resetTestKey} 重置測試");
        GUILayout.Label($"最後測試結果: {(lastTestResult ? "成功" : "失敗")}");
        GUILayout.Label($"最後測試信息: {lastTestMessage}");
        GUILayout.EndArea();
    }
    
    // 在Inspector中顯示測試信息
    [ContextMenu("測試危攻擊系統")]
    void TestDangerousAttackSystem()
    {
        Debug.Log("=== 開始危攻擊系統測試 ===");
        
        TestDangerousAttack();
        TestBossDangerousAttack();
        TestPlayerDangerousDamage();
        
        Debug.Log("=== 危攻擊系統測試完成 ===");
    }
    
    [ContextMenu("檢查系統組件")]
    void CheckSystemComponents()
    {
        Debug.Log("=== 檢查危攻擊系統組件 ===");
        
        if (testEnemy != null)
        {
            Debug.Log($"敵人組件檢查:");
            Debug.Log($"- DangerousAttackHandler: {testEnemy.GetComponent<DangerousAttackHandler>() != null}");
            Debug.Log($"- AnimationEventHandler: {testEnemy.GetComponent<AnimationEventHandler>() != null}");
            Debug.Log($"- Animator: {testEnemy.GetComponent<Animator>() != null}");
        }
        
        if (testBoss != null)
        {
            Debug.Log($"Boss組件檢查:");
            Debug.Log($"- BossAI: {testBoss != null}");
            Debug.Log($"- DangerousAttackHandler: {testBoss.GetComponent<DangerousAttackHandler>() != null}");
            Debug.Log($"- AnimationEventHandler: {testBoss.GetComponent<AnimationEventHandler>() != null}");
        }
        
        if (testPlayer != null)
        {
            Debug.Log($"玩家組件檢查:");
            Debug.Log($"- PlayerStatus: {testPlayer != null}");
            Debug.Log($"- HealthPostureController: {testPlayer.GetComponent<HealthPostureController>() != null}");
        }
        
        Debug.Log("=== 組件檢查完成 ===");
    }
} 