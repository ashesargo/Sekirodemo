using UnityEngine;

public class StaggerExecutionTest : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode testStaggerKey = KeyCode.T; // 測試失衡的按鍵
    public KeyCode testExecutionKey = KeyCode.Y; // 測試處決的按鍵
    public float testPostureAmount = 100; // 測試架勢值
    
    private EnemyAI enemyAI;
    private HealthPostureController healthController;
    
    void Start()
    {
        // 獲取組件
        enemyAI = GetComponent<EnemyAI>();
        healthController = GetComponent<HealthPostureController>();
        
        if (enemyAI == null)
        {
            Debug.LogWarning("[StaggerExecutionTest] 此物件沒有EnemyAI組件");
        }
        
        if (healthController == null)
        {
            Debug.LogWarning("[StaggerExecutionTest] 此物件沒有HealthPostureController組件");
        }
        
        Debug.Log("[StaggerExecutionTest] 測試腳本已初始化");
        Debug.Log($"[StaggerExecutionTest] 按 {testStaggerKey} 測試失衡功能");
        Debug.Log($"[StaggerExecutionTest] 按 {testExecutionKey} 測試處決功能");
    }
    
    void Update()
    {
        // 測試失衡功能
        if (Input.GetKeyDown(testStaggerKey))
        {
            TestStagger();
        }
        
        // 測試處決功能
        if (Input.GetKeyDown(testExecutionKey))
        {
            TestExecution();
        }
    }
    
    private void TestStagger()
    {
        if (healthController != null)
        {
            Debug.Log("[StaggerExecutionTest] 測試失衡功能 - 增加架勢值");
            healthController.AddPosture((int)testPostureAmount);
        }
        else
        {
            Debug.LogWarning("[StaggerExecutionTest] 無法測試失衡功能 - 缺少HealthPostureController");
        }
    }
    
    private void TestExecution()
    {
        if (enemyAI != null)
        {
            Debug.Log("[StaggerExecutionTest] 測試處決功能 - 直接進入失衡狀態");
            enemyAI.SwitchState(new StaggerState());
        }
        else
        {
            Debug.LogWarning("[StaggerExecutionTest] 無法測試處決功能 - 缺少EnemyAI");
        }
    }
    
    // 在Scene視圖中顯示測試信息
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // 顯示測試按鍵信息
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
            $"測試失衡: {testStaggerKey}\n測試處決: {testExecutionKey}");
        #endif
    }
} 