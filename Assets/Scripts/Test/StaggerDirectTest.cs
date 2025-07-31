using UnityEngine;

public class StaggerDirectTest : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode testDefendStaggerKey = KeyCode.D; // 測試防禦後直接失衡
    public KeyCode testHitStaggerKey = KeyCode.H; // 測試受傷後直接失衡
    public KeyCode setHighPostureKey = KeyCode.P; // 設置高架勢值
    public KeyCode resetKey = KeyCode.R; // 重置狀態
    
    private EnemyTest enemyTest;
    private HealthPostureController healthController;
    private EnemyAI enemyAI;
    
    void Start()
    {
        enemyTest = GetComponent<EnemyTest>();
        healthController = GetComponent<HealthPostureController>();
        enemyAI = GetComponent<EnemyAI>();
        
        if (enemyTest == null) { Debug.LogError("[StaggerDirectTest] 此物件沒有EnemyTest組件！"); return; }
        if (healthController == null) { Debug.LogError("[StaggerDirectTest] 此物件沒有HealthPostureController組件！"); return; }
        if (enemyAI == null) { Debug.LogError("[StaggerDirectTest] 此物件沒有EnemyAI組件！"); return; }
        
        Debug.Log("[StaggerDirectTest] 直接失衡測試腳本已初始化");
        Debug.Log($"[StaggerDirectTest] 按 {testDefendStaggerKey} 測試防禦後直接失衡");
        Debug.Log($"[StaggerDirectTest] 按 {testHitStaggerKey} 測試受傷後直接失衡");
        Debug.Log($"[StaggerDirectTest] 按 {setHighPostureKey} 設置高架勢值");
        Debug.Log($"[StaggerDirectTest] 按 {resetKey} 重置狀態");
        ShowCurrentStatus();
    }
    
    void Update()
    {
        if (enemyTest == null || healthController == null || enemyAI == null) return;
        
        if (Input.GetKeyDown(testDefendStaggerKey)) { TestDefendStagger(); }
        if (Input.GetKeyDown(testHitStaggerKey)) { TestHitStagger(); }
        if (Input.GetKeyDown(setHighPostureKey)) { SetHighPosture(); }
        if (Input.GetKeyDown(resetKey)) { ResetStatus(); }
    }
    
    private void ShowCurrentStatus()
    {
        Debug.Log($"[StaggerDirectTest] 當前狀態:");
        Debug.Log($"  - isDead: {enemyTest.isDead}");
        Debug.Log($"  - isStaggered: {enemyTest.isStaggered}");
        Debug.Log($"  - 當前狀態: {enemyAI.CurrentState?.GetType().Name}");
        Debug.Log($"  - 生命值: {enemyTest.GetCurrentHP()}");
        Debug.Log($"  - 架勢值: {healthController.GetPosturePercentage() * 100:F1}%");
    }
    
    private void TestDefendStagger()
    {
        Debug.Log("[StaggerDirectTest] 測試防禦後直接失衡");
        
        // 設置高架勢值（接近100%）
        healthController.SetPostureValue(0.85f);
        
        // 模擬防禦攻擊（應該直接進入失衡狀態）
        enemyTest.TakeDamage(20);
        
        ShowCurrentStatus();
    }
    
    private void TestHitStagger()
    {
        Debug.Log("[StaggerDirectTest] 測試受傷後直接失衡");
        
        // 設置高架勢值（接近100%）
        healthController.SetPostureValue(0.85f);
        
        // 模擬受傷攻擊（應該直接進入失衡狀態）
        enemyTest.TakeDamage(20);
        
        ShowCurrentStatus();
    }
    
    private void SetHighPosture()
    {
        Debug.Log("[StaggerDirectTest] 設置高架勢值");
        
        // 設置架勢值為85%
        healthController.SetPostureValue(0.85f);
        
        ShowCurrentStatus();
    }
    
    private void ResetStatus()
    {
        Debug.Log("[StaggerDirectTest] 重置狀態");
        
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        // 顯示失衡狀態
        if (enemyTest != null && enemyTest.isStaggered)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 3f);
        }
    }
} 