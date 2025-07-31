using UnityEngine;

public class DeathPriorityTest : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode testLowHealthKey = KeyCode.L; // 測試低血量死亡
    public KeyCode testHighPostureKey = KeyCode.H; // 測試高架勢值
    public KeyCode testBothKey = KeyCode.B; // 測試同時低血量和架勢值
    public KeyCode resetKey = KeyCode.R; // 重置狀態
    
    private EnemyTest enemyTest;
    private HealthPostureController healthController;
    private EnemyAI enemyAI;
    
    void Start()
    {
        enemyTest = GetComponent<EnemyTest>();
        healthController = GetComponent<HealthPostureController>();
        enemyAI = GetComponent<EnemyAI>();
        
        if (enemyTest == null) { Debug.LogError("[DeathPriorityTest] 此物件沒有EnemyTest組件！"); return; }
        if (healthController == null) { Debug.LogError("[DeathPriorityTest] 此物件沒有HealthPostureController組件！"); return; }
        if (enemyAI == null) { Debug.LogError("[DeathPriorityTest] 此物件沒有EnemyAI組件！"); return; }
        
        Debug.Log("[DeathPriorityTest] 死亡優先級測試腳本已初始化");
        Debug.Log($"[DeathPriorityTest] 按 {testLowHealthKey} 測試低血量死亡");
        Debug.Log($"[DeathPriorityTest] 按 {testHighPostureKey} 測試高架勢值");
        Debug.Log($"[DeathPriorityTest] 按 {testBothKey} 測試同時低血量和架勢值");
        Debug.Log($"[DeathPriorityTest] 按 {resetKey} 重置狀態");
        ShowCurrentStatus();
    }
    
    void Update()
    {
        if (enemyTest == null || healthController == null || enemyAI == null) return;
        
        if (Input.GetKeyDown(testLowHealthKey)) { TestLowHealthDeath(); }
        if (Input.GetKeyDown(testHighPostureKey)) { TestHighPosture(); }
        if (Input.GetKeyDown(testBothKey)) { TestBothConditions(); }
        if (Input.GetKeyDown(resetKey)) { ResetStatus(); }
    }
    
    private void ShowCurrentStatus()
    {
        Debug.Log($"[DeathPriorityTest] 當前狀態:");
        Debug.Log($"  - isDead: {enemyTest.isDead}");
        Debug.Log($"  - isStaggered: {enemyTest.isStaggered}");
        Debug.Log($"  - 當前狀態: {enemyAI.CurrentState?.GetType().Name}");
        Debug.Log($"  - 生命值: {enemyTest.GetCurrentHP()}");
        Debug.Log($"  - 架勢值: {healthController.GetPosturePercentage() * 100:F1}%");
    }
    
    private void TestLowHealthDeath()
    {
        Debug.Log("[DeathPriorityTest] 測試低血量死亡");
        
        // 設置低血量（接近死亡）
        healthController.SetHealthValue(0.1f);
        
        // 模擬攻擊（應該進入死亡狀態）
        enemyTest.TakeDamage(20);
        
        ShowCurrentStatus();
    }
    
    private void TestHighPosture()
    {
        Debug.Log("[DeathPriorityTest] 測試高架勢值");
        
        // 設置高架勢值（接近100%）
        healthController.SetPostureValue(0.85f);
        
        // 模擬攻擊（應該進入失衡狀態）
        enemyTest.TakeDamage(20);
        
        ShowCurrentStatus();
    }
    
    private void TestBothConditions()
    {
        Debug.Log("[DeathPriorityTest] 測試同時低血量和架勢值");
        
        // 設置低血量和架勢值
        healthController.SetHealthValue(0.1f);
        healthController.SetPostureValue(0.85f);
        
        // 模擬攻擊（應該優先進入死亡狀態）
        enemyTest.TakeDamage(20);
        
        ShowCurrentStatus();
    }
    
    private void ResetStatus()
    {
        Debug.Log("[DeathPriorityTest] 重置狀態");
        
        // 重置架勢值和生命值
        healthController.ResetPosture();
        healthController.SetHealthValue(1.0f);
        
        // 重置標記
        enemyTest.isStaggered = false;
        enemyTest.isDead = false;
        
        // 切換到閒置狀態
        enemyAI.SwitchState(new IdleState());
        
        ShowCurrentStatus();
    }
    
    void OnDrawGizmosSelected()
    {
        // 顯示測試範圍
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        // 顯示死亡狀態
        if (enemyTest != null && enemyTest.isDead)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, 3f);
        }
        // 顯示失衡狀態
        else if (enemyTest != null && enemyTest.isStaggered)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 3f);
        }
    }
} 