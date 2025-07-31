using UnityEngine;

public class StaggerImmunityTest : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode testStaggerKey = KeyCode.S; // 測試進入失衡狀態
    public KeyCode testAttackKey = KeyCode.A; // 測試攻擊敵人
    public KeyCode testExecutionKey = KeyCode.E; // 測試處決
    public KeyCode resetKey = KeyCode.R; // 重置狀態
    
    private EnemyTest enemyTest;
    private HealthPostureController healthController;
    private EnemyAI enemyAI;
    private ExecutionSystem playerExecutionSystem;
    
    void Start()
    {
        enemyTest = GetComponent<EnemyTest>();
        healthController = GetComponent<HealthPostureController>();
        enemyAI = GetComponent<EnemyAI>();
        
        // 尋找玩家的ExecutionSystem
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerExecutionSystem = player.GetComponent<ExecutionSystem>();
        }
        
        if (enemyTest == null) { Debug.LogError("[StaggerImmunityTest] 此物件沒有EnemyTest組件！"); return; }
        if (healthController == null) { Debug.LogError("[StaggerImmunityTest] 此物件沒有HealthPostureController組件！"); return; }
        if (enemyAI == null) { Debug.LogError("[StaggerImmunityTest] 此物件沒有EnemyAI組件！"); return; }
        
        Debug.Log("[StaggerImmunityTest] 失衡免疫測試腳本已初始化");
        Debug.Log($"[StaggerImmunityTest] 按 {testStaggerKey} 測試進入失衡狀態");
        Debug.Log($"[StaggerImmunityTest] 按 {testAttackKey} 測試攻擊敵人");
        Debug.Log($"[StaggerImmunityTest] 按 {testExecutionKey} 測試處決");
        Debug.Log($"[StaggerImmunityTest] 按 {resetKey} 重置狀態");
        ShowCurrentStatus();
    }
    
    void Update()
    {
        if (enemyTest == null || healthController == null || enemyAI == null) return;
        
        if (Input.GetKeyDown(testStaggerKey)) { TestStaggerState(); }
        if (Input.GetKeyDown(testAttackKey)) { TestAttackEnemy(); }
        if (Input.GetKeyDown(testExecutionKey)) { TestExecution(); }
        if (Input.GetKeyDown(resetKey)) { ResetStatus(); }
    }
    
    private void ShowCurrentStatus()
    {
        Debug.Log($"[StaggerImmunityTest] 當前狀態:");
        Debug.Log($"  - isDead: {enemyTest.isDead}");
        Debug.Log($"  - isStaggered: {enemyTest.isStaggered}");
        Debug.Log($"  - 當前狀態: {enemyAI.CurrentState?.GetType().Name}");
        Debug.Log($"  - 生命值: {enemyTest.GetCurrentHP()}");
        Debug.Log($"  - 架勢值: {healthController.GetPosturePercentage() * 100:F1}%");
    }
    
    private void TestStaggerState()
    {
        Debug.Log("[StaggerImmunityTest] 測試進入失衡狀態");
        
        // 設置最大架勢值
        healthController.SetPostureValue(1.0f);
        
        // 手動切換到失衡狀態
        enemyAI.SwitchState(new StaggerState());
        
        ShowCurrentStatus();
    }
    
    private void TestAttackEnemy()
    {
        Debug.Log("[StaggerImmunityTest] 測試攻擊敵人");
        
        // 模擬玩家攻擊
        enemyTest.TakeDamage(20);
        
        ShowCurrentStatus();
    }
    
    private void TestExecution()
    {
        Debug.Log("[StaggerImmunityTest] 測試處決");
        
        if (playerExecutionSystem != null)
        {
            // 如果敵人在失衡狀態，手動觸發處決
            if (enemyTest.isStaggered)
            {
                enemyAI.SwitchState(new ExecutedState());
                Debug.Log("[StaggerImmunityTest] 手動觸發處決狀態");
            }
            else
            {
                Debug.Log("[StaggerImmunityTest] 敵人不在失衡狀態，無法處決");
            }
        }
        else
        {
            Debug.LogError("[StaggerImmunityTest] 找不到玩家的ExecutionSystem！");
        }
        
        ShowCurrentStatus();
    }
    
    private void ResetStatus()
    {
        Debug.Log("[StaggerImmunityTest] 重置狀態");
        
        // 重置架勢值
        healthController.ResetPosture();
        
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        // 顯示失衡狀態
        if (enemyTest != null && enemyTest.isStaggered)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 3f);
        }
    }
} 