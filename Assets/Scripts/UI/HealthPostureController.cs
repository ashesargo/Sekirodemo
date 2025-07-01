using UnityEngine;
using UnityEngine.UI;

// 生命值與架勢條控制器
public class HealthPostureController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;   // 最大生命值
    [SerializeField] private int maxPosture = 100;  // 最大架勢值
    [SerializeField] private HealthPostureUI healthPostureUI;   // 生命值與架勢 UI 顯示

    private HealthPostureSystem healthPostureSystem;    // 引用生命值與架勢系統

    private void Awake()
    {
        // 初始化生命值與架勢系統
        healthPostureSystem = new HealthPostureSystem(maxHealth, maxPosture);

        // 初始化 HealthPostureUI
        if (healthPostureUI != null)
        {
            healthPostureUI.SetHealthPostureSystem(healthPostureSystem);
        }

        // 訂閱事件
        healthPostureSystem.OnDead += OnDead;
        healthPostureSystem.OnPostureBroken += OnPostureBroken;
    }

    // 受到傷害
    public void TakeDamage(int amount)
    {
        healthPostureSystem.HealthDamage(amount);
    }

    // 增加架勢
    public void AddPosture(int amount)
    {
        healthPostureSystem.PostureIncrease(amount);
    }

    // 架勢恢復
    private void Update()
    {
        healthPostureSystem.HandlePostureRecovery();
    }

    // 死亡
    private void OnDead(object sender, System.EventArgs e)
    {
        Debug.Log($"{gameObject.name} 死亡！");
        // 播放死亡動畫
    }

    // 架勢被打破
    private void OnPostureBroken(object sender, System.EventArgs e)
    {
        Debug.Log($"{gameObject.name} 架勢被打破！");

        // 呼叫 OnDead 事件
        OnDead(sender, e);
        // 播放死亡動畫
    }
}
