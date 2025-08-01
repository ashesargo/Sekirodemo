using UnityEngine;

public class DangerousAttackConverterTest : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode enableDangerousAttackKey = KeyCode.E;
    public KeyCode disableDangerousAttackKey = KeyCode.D;
    public KeyCode testAttackKey = KeyCode.T;
    
    [Header("測試目標")]
    public EnemyAI testEnemy;
    public BossAI testBoss;
    
    [Header("測試結果")]
    public bool isDangerousAttackEnabled = false;
    public string lastTestMessage = "";
    
    private DangerousAttackConverter converter;
    
    void Start()
    {
        // 自動尋找測試目標
        if (testEnemy == null)
            testEnemy = FindObjectOfType<EnemyAI>();
        
        if (testBoss == null)
            testBoss = FindObjectOfType<BossAI>();
        
        // 獲取或添加DangerousAttackConverter
        if (testEnemy != null)
        {
            converter = testEnemy.GetComponent<DangerousAttackConverter>();
            if (converter == null)
            {
                converter = testEnemy.gameObject.AddComponent<DangerousAttackConverter>();
            }
        }
        
        Debug.Log("DangerousAttackConverterTest: 測試系統已初始化");
        Debug.Log($"找到測試目標 - Enemy: {testEnemy != null}, Boss: {testBoss != null}");
    }
    
    void Update()
    {
        // 啟用危攻擊轉換
        if (Input.GetKeyDown(enableDangerousAttackKey))
        {
            EnableDangerousAttack();
        }
        
        // 禁用危攻擊轉換
        if (Input.GetKeyDown(disableDangerousAttackKey))
        {
            DisableDangerousAttack();
        }
        
        // 測試攻擊
        if (Input.GetKeyDown(testAttackKey))
        {
            TestAttack();
        }
    }
    
    void EnableDangerousAttack()
    {
        if (converter != null)
        {
            converter.EnableDangerousConversion();
            isDangerousAttackEnabled = true;
            lastTestMessage = "危攻擊轉換已啟用";
            Debug.Log("DangerousAttackConverterTest: 危攻擊轉換已啟用");
        }
        else if (testBoss != null)
        {
            testBoss.EnableDangerousAttack();
            isDangerousAttackEnabled = true;
            lastTestMessage = "Boss危攻擊已啟用";
            Debug.Log("DangerousAttackConverterTest: Boss危攻擊已啟用");
        }
    }
    
    void DisableDangerousAttack()
    {
        if (converter != null)
        {
            converter.DisableDangerousConversion();
            isDangerousAttackEnabled = false;
            lastTestMessage = "危攻擊轉換已禁用";
            Debug.Log("DangerousAttackConverterTest: 危攻擊轉換已禁用");
        }
        else if (testBoss != null)
        {
            testBoss.DisableDangerousAttack();
            isDangerousAttackEnabled = false;
            lastTestMessage = "Boss危攻擊已禁用";
            Debug.Log("DangerousAttackConverterTest: Boss危攻擊已禁用");
        }
    }
    
    void TestAttack()
    {
        if (testEnemy != null)
        {
            // 切換到攻擊狀態來測試
            testEnemy.SwitchState(new AttackState());
            lastTestMessage = "執行測試攻擊";
            Debug.Log("DangerousAttackConverterTest: 執行測試攻擊");
        }
        else if (testBoss != null)
        {
            // 切換到Boss連擊狀態來測試
            testBoss.SwitchState(new BossComboState());
            lastTestMessage = "執行Boss測試攻擊";
            Debug.Log("DangerousAttackConverterTest: 執行Boss測試攻擊");
        }
    }
    
    void OnGUI()
    {
        // 顯示測試信息
        GUILayout.BeginArea(new Rect(10, 10, 350, 250));
        GUILayout.Label("危攻擊轉換器測試", GUI.skin.box);
        GUILayout.Label($"按 {enableDangerousAttackKey} 啟用危攻擊轉換");
        GUILayout.Label($"按 {disableDangerousAttackKey} 禁用危攻擊轉換");
        GUILayout.Label($"按 {testAttackKey} 測試攻擊");
        GUILayout.Label($"危攻擊狀態: {(isDangerousAttackEnabled ? "啟用" : "禁用")}");
        GUILayout.Label($"最後操作: {lastTestMessage}");
        
        // 顯示轉換器狀態
        if (converter != null)
        {
            GUILayout.Label($"轉換器狀態: {(converter.IsDangerousAttackActive() ? "活躍" : "非活躍")}");
            GUILayout.Label($"危攻擊狀態: {(converter.IsInDangerousState() ? "是" : "否")}");
        }
        
        GUILayout.EndArea();
    }
    
    // 在Inspector中顯示測試信息
    [ContextMenu("啟用危攻擊轉換")]
    void EnableDangerousAttackDebug()
    {
        EnableDangerousAttack();
    }
    
    [ContextMenu("禁用危攻擊轉換")]
    void DisableDangerousAttackDebug()
    {
        DisableDangerousAttack();
    }
    
    [ContextMenu("測試攻擊")]
    void TestAttackDebug()
    {
        TestAttack();
    }
    
    [ContextMenu("檢查轉換器狀態")]
    void CheckConverterStatus()
    {
        if (converter != null)
        {
            Debug.Log("=== DangerousAttackConverter 狀態檢查 ===");
            Debug.Log($"- 轉換啟用: {converter.IsDangerousAttackActive()}");
            Debug.Log($"- 危攻擊狀態: {converter.IsInDangerousState()}");
            Debug.Log($"- 傷害值: {converter.dangerousAttackDamage}");
        }
        else
        {
            Debug.LogWarning("沒有找到DangerousAttackConverter組件");
        }
    }
    
    // 動態設置危攻擊傷害
    [ContextMenu("設置危攻擊傷害為50")]
    void SetDangerousAttackDamage50()
    {
        if (converter != null)
        {
            converter.SetDangerousAttackDamage(50f);
            Debug.Log("危攻擊傷害已設置為50");
        }
    }
    
    [ContextMenu("設置危攻擊傷害為30")]
    void SetDangerousAttackDamage30()
    {
        if (converter != null)
        {
            converter.SetDangerousAttackDamage(30f);
            Debug.Log("危攻擊傷害已設置為30");
        }
    }
} 