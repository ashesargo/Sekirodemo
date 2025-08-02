using UnityEngine;

public class ReviveSystemTest : MonoBehaviour
{
    [Header("測試設定")]
    public int testLiveCount = 2; // 測試用的復活次數
    public KeyCode testReviveKey = KeyCode.R; // 測試復活的按鍵
    public KeyCode testExecuteKey = KeyCode.E; // 測試處決的按鍵
    public KeyCode testDeathKey = KeyCode.D; // 測試死亡的按鍵
    public KeyCode resetKey = KeyCode.T; // 重置按鍵

    private HealthPostureController healthController;
    private EnemyAI enemyAI;
    private EnemyTest enemyTest;

    void Start()
    {
        healthController = GetComponent<HealthPostureController>();
        enemyAI = GetComponent<EnemyAI>();
        enemyTest = GetComponent<EnemyTest>();

        if (healthController != null)
        {
            // 設置測試用的復活次數
            healthController.live = testLiveCount;
            healthController.maxLive = testLiveCount;
            
            if (healthController.healthPostureUI != null)
            {
                healthController.healthPostureUI.UpdateLifeBalls(healthController.live, healthController.maxLive);
            }
        }

        Debug.Log($"[ReviveSystemTest] 測試腳本已初始化，復活次數: {testLiveCount}");
        Debug.Log($"[ReviveSystemTest] 按 {testExecuteKey} 測試處決");
        Debug.Log($"[ReviveSystemTest] 按 {testDeathKey} 測試死亡");
        Debug.Log($"[ReviveSystemTest] 按 {testReviveKey} 測試復活");
        Debug.Log($"[ReviveSystemTest] 按 {resetKey} 重置狀態");
    }

    void Update()
    {
        if (Input.GetKeyDown(testExecuteKey))
        {
            TestExecute();
        }
        
        if (Input.GetKeyDown(testDeathKey))
        {
            TestDeath();
        }
        
        if (Input.GetKeyDown(testReviveKey))
        {
            TestRevive();
        }
        
        if (Input.GetKeyDown(resetKey))
        {
            ResetTest();
        }
    }

    void TestExecute()
    {
        if (enemyAI != null)
        {
            Debug.Log($"[ReviveSystemTest] 測試處決，當前復活次數: {healthController?.live ?? 0}");
            enemyAI.SwitchState(new ExecutedState());
        }
    }

    void TestDeath()
    {
        if (enemyAI != null)
        {
            Debug.Log($"[ReviveSystemTest] 測試死亡，當前復活次數: {healthController?.live ?? 0}");
            enemyAI.SwitchState(new DieState());
        }
    }

    void TestRevive()
    {
        if (healthController != null && enemyAI != null)
        {
            Debug.Log($"[ReviveSystemTest] 測試復活，當前復活次數: {healthController.live}");
            enemyAI.SwitchState(new ReviveState());
        }
    }

    void ResetTest()
    {
        if (healthController != null)
        {
            healthController.live = testLiveCount;
            healthController.ResetHealth();
            healthController.ResetPosture();
            
            if (healthController.healthPostureUI != null)
            {
                healthController.healthPostureUI.UpdateLifeBalls(healthController.live, healthController.maxLive);
            }
        }

        if (enemyTest != null)
        {
            enemyTest.RestoreControl();
        }

        if (enemyAI != null)
        {
            enemyAI.enabled = true;
            enemyAI.canAutoAttack = true;
            enemyAI.canBeParried = true;
            enemyAI.SwitchState(new IdleState());
        }

        Debug.Log($"[ReviveSystemTest] 狀態已重置，復活次數: {healthController?.live ?? 0}");
    }

    void OnGUI()
    {
        if (healthController != null)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"復活次數: {healthController.live}/{healthController.maxLive}");
            GUI.Label(new Rect(10, 30, 300, 20), $"生命值: {healthController.GetHealthPercentage():P0}");
            GUI.Label(new Rect(10, 50, 300, 20), $"架勢值: {healthController.GetPosturePercentage():P0}");
        }
        
        GUI.Label(new Rect(10, 90, 300, 20), $"按 {testExecuteKey} 測試處決");
        GUI.Label(new Rect(10, 110, 300, 20), $"按 {testDeathKey} 測試死亡");
        GUI.Label(new Rect(10, 130, 300, 20), $"按 {testReviveKey} 測試復活");
        GUI.Label(new Rect(10, 150, 300, 20), $"按 {resetKey} 重置狀態");
    }
} 