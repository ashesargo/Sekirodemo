using UnityEngine;
using UnityEngine.UI;

// 生命值與架勢系統測試 UI
public class HealthPostureTest : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100; // 最大生命值
    [SerializeField] private int maxPosture = 100; // 最大架勢值
    [SerializeField] private HealthPostureUI healthPostureUI; // 生命值與架勢 UI 顯示器
    
    [Header("按鈕")]
    [SerializeField] private Button damageButton; // 受到攻擊按鈕
    [SerializeField] private Button healButton; // 回覆血量按鈕
    [SerializeField] private Button increasePostureButton; // 增加架勢按鈕
    [SerializeField] private Button decreasePostureButton; // 減少架勢按鈕
    
    [Header("數值設定")]
    [SerializeField] private int damageAmount = 10; // 每次受到的傷害
    [SerializeField] private int healAmount = 10; // 每次恢復的生命值
    [SerializeField] private int postureIncreaseAmount = 10; // 每次增加的架勢值
    [SerializeField] private int postureDecreaseAmount = 10; // 每次減少的架勢值
    
    // 生命與架勢系統
    private HealthPostureSystem healthPostureSystem;
    
    private void Start()
    {
        // 初始化生命與架勢系統
        healthPostureSystem = new HealthPostureSystem(maxHealth, maxPosture);
        
        // 初始化 HealthPostureUI
        if (healthPostureUI != null)
        {
            healthPostureUI.SetHealthPostureSystem(healthPostureSystem);
        }
        
        // 設定按鈕事件
        SetupButtons();
        
        // 訂閱事件
        SubscribeToEvents();
        
        Debug.Log("生命值與架勢測試系統初始化");
        Debug.Log($"初始生命值: {maxHealth}, 初始架勢值: 0");
    }
    
    // 設定按鈕事件
    private void SetupButtons()
    {
        // 受到攻擊按鈕
        if (damageButton != null)
        {
            damageButton.onClick.AddListener(OnDamageButtonClicked);
        }
        
        // 回覆血量按鈕
        if (healButton != null)
        {
            healButton.onClick.AddListener(OnHealButtonClicked);
        }
        
        // 增加架勢按鈕
        if (increasePostureButton != null)
        {
            increasePostureButton.onClick.AddListener(OnIncreasePostureButtonClicked);
        }
        
        // 減少架勢按鈕
        if (decreasePostureButton != null)
        {
            decreasePostureButton.onClick.AddListener(OnDecreasePostureButtonClicked);
        }
    }
    

    // 系統事件
    private void SubscribeToEvents()
    {
        if (healthPostureSystem != null)
        {
            healthPostureSystem.OnHealthChanged += OnHealthChanged;
            healthPostureSystem.OnPostureChanged += OnPostureChanged;
            healthPostureSystem.OnDead += OnDead;
            healthPostureSystem.OnPostureBroken += OnPostureBroken;
        }
    }
    
    // 受到攻擊按鈕點擊
    private void OnDamageButtonClicked()
    {
        healthPostureSystem.HealthDamage(damageAmount);
        Debug.Log($"受到 {damageAmount} 點傷害！當前生命值: {healthPostureSystem.GetHealthNormalized() * 100:F1}%");
    }
    
    // 回覆血量按鈕點擊
    private void OnHealButtonClicked()
    {
        healthPostureSystem.HealthHeal(healAmount);
        Debug.Log($"恢復 {healAmount} 點生命值！當前生命值: {healthPostureSystem.GetHealthNormalized() * 100:F1}%");
    }
    
    // 增加架勢按鈕點擊
    private void OnIncreasePostureButtonClicked()
    {
        healthPostureSystem.PostureIncrease(postureIncreaseAmount);
        Debug.Log($"增加 {postureIncreaseAmount} 點架勢值！當前架勢值: {healthPostureSystem.GetPostureNormalized() * 100:F1}%");
    }
    
    // 減少架勢按鈕點擊
    private void OnDecreasePostureButtonClicked()
    {
        healthPostureSystem.PostureDecrease(postureDecreaseAmount);
        Debug.Log($"減少 {postureDecreaseAmount} 點架勢值！當前架勢值: {healthPostureSystem.GetPostureNormalized() * 100:F1}%");
    }
    
    // 當生命值改變時
    private void OnHealthChanged(object sender, System.EventArgs e)
    {
        Debug.Log($"生命值已更新: {healthPostureSystem.GetHealthNormalized() * 100:F1}%");
    }
    
    // 當架勢值改變時
    private void OnPostureChanged(object sender, System.EventArgs e)
    {
        Debug.Log($"架勢值已更新: {healthPostureSystem.GetPostureNormalized() * 100:F1}%");
    }
    
    // 當死亡時
    private void OnDead(object sender, System.EventArgs e)
    {
        Debug.Log("角色死亡！");
    }
    
    // 當架勢被擊破時
    private void OnPostureBroken(object sender, System.EventArgs e)
    {
        Debug.Log("架勢被擊破！");
    }
    
    // 重置系統
    public void ResetSystem()
    {
        // 重新創建系統
        healthPostureSystem = new HealthPostureSystem(maxHealth, maxPosture);
        
        // 重新設定 UI
        if (healthPostureUI != null)
        {
            healthPostureUI.SetHealthPostureSystem(healthPostureSystem);
        }
        
        // 重新訂閱事件
        SubscribeToEvents();
        
        Debug.Log("系統已重置！");
    }
    
    // 獲取當前系統狀態
    public HealthPostureSystem GetHealthPostureSystem()
    {
        return healthPostureSystem;
    }
    
    private void OnDestroy()
    {
        // 取消訂閱事件
        if (healthPostureSystem != null)
        {
            healthPostureSystem.OnHealthChanged -= OnHealthChanged;
            healthPostureSystem.OnPostureChanged -= OnPostureChanged;
            healthPostureSystem.OnDead -= OnDead;
            healthPostureSystem.OnPostureBroken -= OnPostureBroken;
        }
    }
}