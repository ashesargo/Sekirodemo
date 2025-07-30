using UnityEngine;

/// <summary>
/// 敵人模糊效果測試腳本
/// 用於驗證敵人模糊系統是否正常工作
/// </summary>
public class EnemyPointBlurTest : MonoBehaviour
{
    [Header("測試設置")]
    public KeyCode testEnemyHitKey = KeyCode.H; // 測試敵人受傷
    public KeyCode testEnemyGuardKey = KeyCode.G; // 測試敵人防禦
    public KeyCode testEnemyParryKey = KeyCode.P; // 測試敵人Parry
    public KeyCode testEnemyAttackKey = KeyCode.A; // 測試敵人攻擊成功
    
    [Header("測試目標")]
    public EnemyAI testEnemy; // 測試用的敵人
    public EnemyPointBlur enemyPointBlur; // 敵人模糊效果組件
    
    void Start()
    {
        // 自動尋找測試組件
        if (testEnemy == null)
        {
            testEnemy = FindObjectOfType<EnemyAI>();
        }
        
        if (enemyPointBlur == null)
        {
            enemyPointBlur = FindObjectOfType<EnemyPointBlur>();
        }
        
        Debug.Log("[EnemyPointBlurTest] 測試腳本已初始化");
        Debug.Log("[EnemyPointBlurTest] 測試敵人: " + (testEnemy != null ? testEnemy.name : "未找到"));
        Debug.Log("[EnemyPointBlurTest] 敵人模糊效果: " + (enemyPointBlur != null ? "已找到" : "未找到"));
    }
    
    void Update()
    {
        // 測試敵人受傷事件
        if (Input.GetKeyDown(testEnemyHitKey))
        {
            TestEnemyHit();
        }
        
        // 測試敵人防禦事件
        if (Input.GetKeyDown(testEnemyGuardKey))
        {
            TestEnemyGuard();
        }
        
        // 測試敵人Parry事件
        if (Input.GetKeyDown(testEnemyParryKey))
        {
            TestEnemyParry();
        }
        
        // 測試敵人攻擊成功事件
        if (Input.GetKeyDown(testEnemyAttackKey))
        {
            TestEnemyAttack();
        }
    }
    
    /// <summary>
    /// 測試敵人受傷事件
    /// </summary>
    void TestEnemyHit()
    {
        if (testEnemy != null && testEnemy.OnEnemyHitOccurred != null)
        {
            Vector3 testPosition = testEnemy.transform.position + testEnemy.transform.forward * 2f;
            testEnemy.OnEnemyHitOccurred.Invoke(testPosition);
            Debug.Log("[EnemyPointBlurTest] 觸發敵人受傷測試，位置: " + testPosition);
        }
        else
        {
            Debug.LogWarning("[EnemyPointBlurTest] 無法測試敵人受傷：testEnemy為null或事件未訂閱");
        }
    }
    
    /// <summary>
    /// 測試敵人防禦事件
    /// </summary>
    void TestEnemyGuard()
    {
        if (testEnemy != null && testEnemy.OnEnemyGuardSuccess != null)
        {
            Vector3 testPosition = testEnemy.transform.position + testEnemy.transform.forward * 2f;
            testEnemy.OnEnemyGuardSuccess.Invoke(testPosition);
            Debug.Log("[EnemyPointBlurTest] 觸發敵人防禦測試，位置: " + testPosition);
        }
        else
        {
            Debug.LogWarning("[EnemyPointBlurTest] 無法測試敵人防禦：testEnemy為null或事件未訂閱");
        }
    }
    
    /// <summary>
    /// 測試敵人攻擊成功事件
    /// </summary>
    void TestEnemyAttack()
    {
        if (testEnemy != null && testEnemy.OnEnemyAttackSuccess != null)
        {
            Vector3 testPosition = testEnemy.transform.position + testEnemy.transform.forward * 1.5f;
            testEnemy.OnEnemyAttackSuccess.Invoke(testPosition);
            Debug.Log("[EnemyPointBlurTest] 觸發敵人攻擊成功測試，位置: " + testPosition);
        }
        else
        {
            Debug.LogWarning("[EnemyPointBlurTest] 無法測試敵人攻擊成功：testEnemy為null或事件未訂閱");
        }
    }
    
    /// <summary>
    /// 測試敵人Parry事件
    /// </summary>
    void TestEnemyParry()
    {
        if (testEnemy != null && testEnemy.OnEnemyParrySuccess != null)
        {
            Vector3 testPosition = testEnemy.transform.position;
            testEnemy.OnEnemyParrySuccess.Invoke(testPosition);
            Debug.Log("[EnemyPointBlurTest] 觸發敵人Parry測試，位置: " + testPosition);
        }
        else
        {
            Debug.LogWarning("[EnemyPointBlurTest] 無法測試敵人Parry：testEnemy為null或事件未訂閱");
        }
    }
    
    /// <summary>
    /// 顯示測試說明
    /// </summary>
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("敵人模糊效果測試", GUI.skin.box);
        GUILayout.Label("按鍵說明:");
        GUILayout.Label(testEnemyHitKey + " - 測試敵人受傷");
        GUILayout.Label(testEnemyGuardKey + " - 測試敵人防禦");
        GUILayout.Label(testEnemyParryKey + " - 測試敵人Parry");
        GUILayout.Label(testEnemyAttackKey + " - 測試敵人攻擊成功");
        GUILayout.Label("狀態:");
        GUILayout.Label("測試敵人: " + (testEnemy != null ? "已設置" : "未設置"));
        GUILayout.Label("模糊效果: " + (enemyPointBlur != null ? "已設置" : "未設置"));
        GUILayout.EndArea();
    }
} 