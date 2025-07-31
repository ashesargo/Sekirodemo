using UnityEngine;

public class DeathAnimationTest : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode testStaggerRecoveryKey = KeyCode.S; // 測試失衡恢復後死亡
    public KeyCode testAttackDeathKey = KeyCode.A; // 測試攻擊時死亡
    public KeyCode testStaggerRecoveryDeathKey = KeyCode.D; // 測試失衡恢復時死亡
    public KeyCode setLowHealthKey = KeyCode.H; // 設置低血量
    public KeyCode resetKey = KeyCode.R; // 重置狀態
    
    private EnemyTest enemyTest;
    private HealthPostureController healthController;
    private EnemyAI enemyAI;
    
    void Start()
    {
        enemyTest = GetComponent<EnemyTest>();
        healthController = GetComponent<HealthPostureController>();
        enemyAI = GetComponent<EnemyAI>();
        
        if (enemyTest == null) { Debug.LogError("[DeathAnimationTest] 此物件沒有EnemyTest組件！"); return; }
        if (healthController == null) { Debug.LogError("[DeathAnimationTest] 此物件沒有HealthPostureController組件！"); return; }
        if (enemyAI == null) { Debug.LogError("[DeathAnimationTest] 此物件沒有EnemyAI組件！"); return; }
        
        Debug.Log("[DeathAnimationTest] 死亡動畫測試腳本已初始化");
        Debug.Log($"[DeathAnimationTest] 按 {testStaggerRecoveryKey} 測試失衡恢復後死亡");
        Debug.Log($"[DeathAnimationTest] 按 {testAttackDeathKey} 測試攻擊時死亡");
        Debug.Log($"[DeathAnimationTest] 按 {testStaggerRecoveryDeathKey} 測試失衡恢復時死亡");
        Debug.Log($"[DeathAnimationTest] 按 {setLowHealthKey} 設置低血量");
        Debug.Log($"[DeathAnimationTest] 按 {resetKey} 重置狀態");
        ShowCurrentStatus();
    }
    
    void Update()
    {
        if (enemyTest == null || healthController == null || enemyAI == null) return;
        
        if (Input.GetKeyDown(testStaggerRecoveryKey)) { TestStaggerRecoveryDeath(); }
        if (Input.GetKeyDown(testAttackDeathKey)) { TestAttackDeath(); }
        if (Input.GetKeyDown(testStaggerRecoveryDeathKey)) { TestStaggerRecoveryDeathDirect(); }
        if (Input.GetKeyDown(setLowHealthKey)) { SetLowHealth(); }
        if (Input.GetKeyDown(resetKey)) { ResetStatus(); }
    }
    
    private void ShowCurrentStatus()
    {
        Debug.Log($"[DeathAnimationTest] 當前狀態:");
        Debug.Log($"  - isDead: {enemyTest.isDead}");
        Debug.Log($"  - isStaggered: {enemyTest.isStaggered}");
        Debug.Log($"  - 當前狀態: {enemyAI.CurrentState?.GetType().Name}");
        Debug.Log($"  - 生命值: {enemyTest.GetCurrentHP()}");
        Debug.Log($"  - 架勢值: {healthController.GetPosturePercentage() * 100:F1}%");
        
        // 檢查動畫狀態
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            string currentAnimation = "Other";
            if (stateInfo.IsName("Attack"))
                currentAnimation = "Attack";
            else if (stateInfo.IsName("Death"))
                currentAnimation = "Death";
            else if (stateInfo.IsName("Stagger"))
                currentAnimation = "Stagger";
            
            Debug.Log($"  - 當前動畫: {currentAnimation}");
            Debug.Log($"  - 動畫進度: {stateInfo.normalizedTime:F2}");
        }
    }
    
    private void TestStaggerRecoveryDeath()
    {
        Debug.Log("[DeathAnimationTest] 測試失衡恢復後死亡");
        
        // 設置低血量
        healthController.SetHealthValue(0.1f);
        
        // 讓敵人進入失衡狀態
        enemyAI.SwitchState(new StaggerState());
        
        // 等待失衡狀態結束（模擬）
        StartCoroutine(WaitForStaggerEnd());
        
        ShowCurrentStatus();
    }
    
    private System.Collections.IEnumerator WaitForStaggerEnd()
    {
        // 等待失衡狀態結束
        yield return new WaitForSeconds(3.5f);
        
        Debug.Log("[DeathAnimationTest] 失衡狀態結束，模擬攻擊導致死亡");
        
        // 模擬攻擊導致死亡
        enemyTest.TakeDamage(20);
        
        ShowCurrentStatus();
    }
    
    private void TestAttackDeath()
    {
        Debug.Log("[DeathAnimationTest] 測試攻擊時死亡");
        
        // 設置低血量
        healthController.SetHealthValue(0.1f);
        
        // 讓敵人進入攻擊狀態
        enemyAI.SwitchState(new AttackState());
        
        // 等待攻擊動畫開始後模擬死亡
        StartCoroutine(WaitForAttackThenDeath());
        
        ShowCurrentStatus();
    }
    
    private System.Collections.IEnumerator WaitForAttackThenDeath()
    {
        // 等待攻擊動畫開始
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("[DeathAnimationTest] 攻擊動畫開始，模擬死亡");
        
        // 模擬攻擊導致死亡
        enemyTest.TakeDamage(20);
        
        ShowCurrentStatus();
    }
    
    private void SetLowHealth()
    {
        Debug.Log("[DeathAnimationTest] 設置低血量");
        
        // 設置低血量（接近死亡）
        healthController.SetHealthValue(0.1f);
        
        ShowCurrentStatus();
    }
    
    private void ResetStatus()
    {
        Debug.Log("[DeathAnimationTest] 重置狀態");
        
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
    
    private void TestStaggerRecoveryDeathDirect()
    {
        Debug.Log("[DeathAnimationTest] 測試失衡恢復時死亡");
        
        // 設置低血量
        healthController.SetHealthValue(0.1f);
        
        // 讓敵人進入失衡狀態
        enemyAI.SwitchState(new StaggerState());
        
        // 等待失衡狀態結束（模擬）
        StartCoroutine(WaitForStaggerRecoveryDeath());
        
        ShowCurrentStatus();
    }
    
    private System.Collections.IEnumerator WaitForStaggerRecoveryDeath()
    {
        // 等待失衡狀態結束
        yield return new WaitForSeconds(3.5f);
        
        Debug.Log("[DeathAnimationTest] 失衡狀態結束，檢查是否進入死亡狀態");
        
        ShowCurrentStatus();
    }
    
    void OnDrawGizmosSelected()
    {
        // 顯示測試範圍
        Gizmos.color = Color.magenta;
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