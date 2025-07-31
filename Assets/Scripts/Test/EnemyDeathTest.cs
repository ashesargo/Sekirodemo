using UnityEngine;

public class EnemyDeathTest : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode testExecutionKey = KeyCode.E; // 測試處決
    public KeyCode testNormalDeathKey = KeyCode.D; // 測試正常死亡
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
        
        if (enemyTest == null)
        {
            Debug.LogError("[EnemyDeathTest] 此物件沒有EnemyTest組件！");
            return;
        }
        
        if (healthController == null)
        {
            Debug.LogError("[EnemyDeathTest] 此物件沒有HealthPostureController組件！");
            return;
        }
        
        if (enemyAI == null)
        {
            Debug.LogError("[EnemyDeathTest] 此物件沒有EnemyAI組件！");
            return;
        }
        
        Debug.Log("[EnemyDeathTest] 敵人死亡測試腳本已初始化");
        Debug.Log($"[EnemyDeathTest] 按 {testExecutionKey} 測試處決死亡");
        Debug.Log($"[EnemyDeathTest] 按 {testNormalDeathKey} 測試正常死亡");
        Debug.Log($"[EnemyDeathTest] 按 {resetKey} 重置狀態");
        
        // 顯示當前設置
        ShowCurrentStatus();
    }
    
    void Update()
    {
        if (enemyTest == null || healthController == null || enemyAI == null) return;
        
        // 測試處決死亡
        if (Input.GetKeyDown(testExecutionKey))
        {
            TestExecutionDeath();
        }
        
        // 測試正常死亡
        if (Input.GetKeyDown(testNormalDeathKey))
        {
            TestNormalDeath();
        }
        
        // 重置狀態
        if (Input.GetKeyDown(resetKey))
        {
            ResetStatus();
        }
    }
    
    private void ShowCurrentStatus()
    {
        Debug.Log("=== 敵人死亡狀態測試 ===");
        Debug.Log($"當前生命值: {healthController.GetHealthPercentage() * 100:F1}%");
        Debug.Log($"當前架勢值: {healthController.GetPosturePercentage() * 100:F1}%");
        Debug.Log($"死亡狀態: {enemyTest.isDead}");
        Debug.Log($"當前狀態: {enemyAI.CurrentState?.GetType().Name}");
        Debug.Log($"AI組件啟用: {enemyAI.enabled}");
        Debug.Log($"碰撞器啟用: {GetComponent<Collider>()?.enabled}");
        Debug.Log("========================");
    }
    
    private void TestExecutionDeath()
    {
        Debug.Log("=== 測試處決死亡 ===");
        
        // 設置架勢值為100%，進入失衡狀態
        healthController.SetPostureValue(1.0f);
        enemyAI.SwitchState(new StaggerState());
        
        Debug.Log("敵人已進入失衡狀態");
        Debug.Log("等待玩家按Q鍵處決...");
        
        ShowCurrentStatus();
    }
    
    private void TestNormalDeath()
    {
        Debug.Log("=== 測試正常死亡 ===");
        
        // 設置生命值為0
        healthController.SetHealthValue(0f);
        
        Debug.Log("敵人生命值已設置為0");
        Debug.Log("應該進入死亡狀態...");
        
        ShowCurrentStatus();
    }
    
    private void ResetStatus()
    {
        Debug.Log("=== 重置狀態 ===");
        
        // 重置生命值和架勢值
        healthController.SetHealthValue(1.0f);
        healthController.SetPostureValue(0f);
        
        // 重置死亡狀態
        enemyTest.isDead = false;
        
        // 重新啟用組件
        enemyAI.enabled = true;
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // 切換到閒置狀態
        enemyAI.SwitchState(new IdleState());
        
        Debug.Log("狀態已重置");
        ShowCurrentStatus();
    }
    
    // 在Scene視圖中顯示測試信息
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, 3f);
        
        // 顯示測試按鍵信息
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 4f, 
            $"測試處決: {testExecutionKey}\n測試正常死亡: {testNormalDeathKey}\n重置: {resetKey}");
        #endif
    }
} 