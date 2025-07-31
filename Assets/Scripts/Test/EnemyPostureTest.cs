using UnityEngine;

public class EnemyPostureTest : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode testDefendKey = KeyCode.D; // 測試防禦
    public KeyCode testHitKey = KeyCode.H; // 測試受傷
    public KeyCode resetKey = KeyCode.R; // 重置架勢值
    public KeyCode setMaxPostureKey = KeyCode.M; // 設置最大架勢值
    
    private EnemyTest enemyTest;
    private HealthPostureController healthController;
    private BossAI bossAI;
    
    void Start()
    {
        enemyTest = GetComponent<EnemyTest>();
        healthController = GetComponent<HealthPostureController>();
        bossAI = GetComponent<BossAI>();
        
        if (enemyTest == null)
        {
            Debug.LogError("[EnemyPostureTest] 此物件沒有EnemyTest組件！");
            return;
        }
        
        if (healthController == null)
        {
            Debug.LogError("[EnemyPostureTest] 此物件沒有HealthPostureController組件！");
            return;
        }
        
        Debug.Log("[EnemyPostureTest] 敵人架勢值測試腳本已初始化");
        Debug.Log($"[EnemyPostureTest] 按 {testDefendKey} 測試防禦架勢值增加");
        Debug.Log($"[EnemyPostureTest] 按 {testHitKey} 測試受傷架勢值增加");
        Debug.Log($"[EnemyPostureTest] 按 {resetKey} 重置架勢值");
        Debug.Log($"[EnemyPostureTest] 按 {setMaxPostureKey} 設置最大架勢值");
        
        // 顯示當前設置
        ShowCurrentSettings();
    }
    
    void Update()
    {
        if (enemyTest == null || healthController == null) return;
        
        // 測試防禦
        if (Input.GetKeyDown(testDefendKey))
        {
            TestDefendPosture();
        }
        
        // 測試受傷
        if (Input.GetKeyDown(testHitKey))
        {
            TestHitPosture();
        }
        
        // 重置架勢值
        if (Input.GetKeyDown(resetKey))
        {
            ResetPosture();
        }
        
        // 設置最大架勢值
        if (Input.GetKeyDown(setMaxPostureKey))
        {
            SetMaxPosture();
        }
    }
    
    private void ShowCurrentSettings()
    {
        Debug.Log("=== 敵人架勢值設置 ===");
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
        Debug.Log("========================");
    }
    
    private void TestDefendPosture()
    {
        Debug.Log("=== 測試防禦架勢值增加 ===");
        
        // 模擬防禦成功
        int postureIncrease = bossAI != null ? bossAI.bossDefendPostureIncrease : enemyTest.defendPostureIncrease;
        float currentPosture = healthController.GetPosturePercentage();
        
        Debug.Log($"防禦前架勢值: {currentPosture * 100:F1}%");
        Debug.Log($"防禦架勢值增加量: {postureIncrease}");
        
        // 直接調用AddPosture模擬防禦
        healthController.AddPosture(postureIncrease, false);
        
        float newPosture = healthController.GetPosturePercentage();
        Debug.Log($"防禦後架勢值: {newPosture * 100:F1}%");
        
        if (newPosture >= 1.0f)
        {
            Debug.Log("✓ 架勢值已達到100%，應該進入失衡狀態！");
        }
        else
        {
            Debug.Log($"⚠ 架勢值未達到100%，當前: {newPosture * 100:F1}%");
        }
        
        ShowCurrentSettings();
    }
    
    private void TestHitPosture()
    {
        Debug.Log("=== 測試受傷架勢值增加 ===");
        
        // 模擬受傷
        float currentPosture = healthController.GetPosturePercentage();
        
        Debug.Log($"受傷前架勢值: {currentPosture * 100:F1}%");
        
        // 使用TakeDamage模擬受傷
        healthController.TakeDamage(10, PlayerStatus.HitState.Hit);
        
        float newPosture = healthController.GetPosturePercentage();
        Debug.Log($"受傷後架勢值: {newPosture * 100:F1}%");
        
        if (newPosture >= 1.0f)
        {
            Debug.Log("✓ 架勢值已達到100%，應該進入失衡狀態！");
        }
        else
        {
            Debug.Log($"⚠ 架勢值未達到100%，當前: {newPosture * 100:F1}%");
        }
        
        ShowCurrentSettings();
    }
    
    private void ResetPosture()
    {
        Debug.Log("=== 重置架勢值 ===");
        healthController.ResetPosture();
        Debug.Log("架勢值已重置為0");
        ShowCurrentSettings();
    }
    
    private void SetMaxPosture()
    {
        Debug.Log("=== 設置最大架勢值 ===");
        healthController.SetPostureValue(1.0f);
        Debug.Log("架勢值已設置為100%");
        ShowCurrentSettings();
    }
    
    // 在Scene視圖中顯示測試信息
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        // 顯示測試按鍵信息
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, 
            $"測試防禦: {testDefendKey}\n測試受傷: {testHitKey}\n重置: {resetKey}\n最大架勢: {setMaxPostureKey}");
        #endif
    }
} 