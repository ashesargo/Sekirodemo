using UnityEngine;

public class StaggerStateTest : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode testDefendStaggerKey = KeyCode.F; // 測試防禦後進入失衡
    public KeyCode setMaxPostureKey = KeyCode.M; // 設置最大架勢值
    public KeyCode resetKey = KeyCode.R; // 重置架勢值
    
    private EnemyTest enemyTest;
    private HealthPostureController healthController;
    private EnemyAI enemyAI;
    private BossAI bossAI;
    
    void Start()
    {
        enemyTest = GetComponent<EnemyTest>();
        healthController = GetComponent<HealthPostureController>();
        enemyAI = GetComponent<EnemyAI>();
        bossAI = GetComponent<BossAI>();
        
        if (enemyTest == null)
        {
            Debug.LogError("[StaggerStateTest] 此物件沒有EnemyTest組件！");
            return;
        }
        
        if (healthController == null)
        {
            Debug.LogError("[StaggerStateTest] 此物件沒有HealthPostureController組件！");
            return;
        }
        
        if (enemyAI == null)
        {
            Debug.LogError("[StaggerStateTest] 此物件沒有EnemyAI組件！");
            return;
        }
        
        Debug.Log("[StaggerStateTest] 失衡狀態測試腳本已初始化");
        Debug.Log($"[StaggerStateTest] 按 {testDefendStaggerKey} 測試防禦後進入失衡");
        Debug.Log($"[StaggerStateTest] 按 {setMaxPostureKey} 設置最大架勢值");
        Debug.Log($"[StaggerStateTest] 按 {resetKey} 重置架勢值");
        
        // 顯示當前設置
        ShowCurrentSettings();
    }
    
    void Update()
    {
        if (enemyTest == null || healthController == null || enemyAI == null) return;
        
        // 測試防禦後進入失衡
        if (Input.GetKeyDown(testDefendStaggerKey))
        {
            TestDefendStagger();
        }
        
        // 設置最大架勢值
        if (Input.GetKeyDown(setMaxPostureKey))
        {
            SetMaxPosture();
        }
        
        // 重置架勢值
        if (Input.GetKeyDown(resetKey))
        {
            ResetPosture();
        }
    }
    
    private void ShowCurrentSettings()
    {
        Debug.Log("=== 失衡狀態測試設置 ===");
        Debug.Log($"一般敵人防禦架勢值增加: {enemyTest.defendPostureIncrease}");
        Debug.Log($"一般敵人受傷架勢值增加: {enemyTest.hitPostureIncrease}");
        Debug.Log($"一般敵人防禦機率: {enemyTest.defendChance * 100:F0}%");
        
        if (bossAI != null)
        {
            Debug.Log($"Boss防禦架勢值增加: {bossAI.bossDefendPostureIncrease}");
            Debug.Log($"Boss受傷架勢值增加: {bossAI.bossHitPostureIncrease}");
            Debug.Log($"Boss防禦機率: {bossAI.bossDefendChance * 100:F0}%");
        }
        
        Debug.Log($"當前架勢值: {healthController.GetPosturePercentage() * 100:F1}%");
        Debug.Log($"當前狀態: {enemyAI.CurrentState?.GetType().Name}");
        Debug.Log("==========================");
    }
    
    private void TestDefendStagger()
    {
        Debug.Log("=== 測試防禦後進入失衡狀態 ===");
        
        // 設置架勢值為100%
        healthController.SetPostureValue(1.0f);
        Debug.Log("架勢值已設置為100%");
        
        // 模擬防禦成功
        HitState.shouldDefend = true;
        
        // 進入HitState（模擬防禦）
        enemyAI.SwitchState(new HitState());
        
        Debug.Log("已進入HitState（防禦狀態）");
        Debug.Log("等待防禦動畫結束後檢查是否進入StaggerState...");
        
        ShowCurrentSettings();
    }
    
    private void SetMaxPosture()
    {
        Debug.Log("=== 設置最大架勢值 ===");
        healthController.SetPostureValue(1.0f);
        Debug.Log("架勢值已設置為100%");
        ShowCurrentSettings();
    }
    
    private void ResetPosture()
    {
        Debug.Log("=== 重置架勢值 ===");
        healthController.ResetPosture();
        Debug.Log("架勢值已重置為0");
        ShowCurrentSettings();
    }
    
    // 在Scene視圖中顯示測試信息
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2.5f);
        
        // 顯示測試按鍵信息
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3.5f, 
            $"測試防禦失衡: {testDefendStaggerKey}\n設置最大架勢: {setMaxPostureKey}\n重置: {resetKey}");
        #endif
    }
} 