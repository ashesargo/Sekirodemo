using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 生命值與架勢條控制器
public class HealthPostureController : MonoBehaviour
{
    [SerializeField] private int maxHealth = EnemyTest.maxHP;   // 最大生命值
    [SerializeField] private int maxPosture = 100;  // 最大架勢值
    [SerializeField] private HealthPostureUI healthPostureUI;   // 生命值與架勢 UI 顯示

    private HealthPostureSystem healthPostureSystem;    // 引用生命值與架勢系統
    private Coroutine hideUICoroutine;  // 隱藏 UI 的協程
    private static HealthPostureController lastAttackedEnemy;  // 最後一個被攻擊的敵人
    private bool colliderDisabled = false;  // 標記碰撞器是否已被關閉

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
        
        // 同時增加架勢值（每次受到傷害增加10點架勢）
        healthPostureSystem.PostureIncrease(10);
        
        // 顯示血條並設定為最後一個被攻擊的敵人
        ShowHealthBar();
    }

    // 增加架勢
    public void AddPosture(int amount)
    {
        healthPostureSystem.PostureIncrease(amount);
        
        // 顯示血條並設定為最後一個被攻擊的敵人
        ShowHealthBar();
    }

    // 顯示血條
    private void ShowHealthBar()
    {
        // 檢查血量是否小於等於0，如果是則不顯示血條
        if (healthPostureSystem.GetHealthNormalized() <= 0)
        {
            return;
        }

        // 如果有其他敵人正在顯示血條，先關閉它
        if (lastAttackedEnemy != null && lastAttackedEnemy != this)
        {
            lastAttackedEnemy.HideHealthBar();
        }

        // 設定為最後一個被攻擊的敵人
        lastAttackedEnemy = this;

        // 顯示血條UI
        if (healthPostureUI != null)
        {
            healthPostureUI.gameObject.SetActive(true);
        }

        // 停止之前的隱藏協程（如果有的話）
        if (hideUICoroutine != null)
        {
            StopCoroutine(hideUICoroutine);
        }

        // 開始新的隱藏協程
        hideUICoroutine = StartCoroutine(HideHealthBarAfterDelay(5f));
    }

    // 隱藏血條
    public void HideHealthBar()
    {
        if (healthPostureUI != null)
        {
            healthPostureUI.gameObject.SetActive(false);
        }

        // 停止隱藏協程
        if (hideUICoroutine != null)
        {
            StopCoroutine(hideUICoroutine);
            hideUICoroutine = null;
        }

        // 如果不是最後一個被攻擊的敵人，清除引用
        if (lastAttackedEnemy == this)
        {
            lastAttackedEnemy = null;
        }
    }

    // 延遲隱藏血條的協程
    private IEnumerator HideHealthBarAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 只有當這個敵人仍然是最後一個被攻擊的敵人時才隱藏
        if (lastAttackedEnemy == this)
        {
            HideHealthBar();
        }
    }

    // 架勢恢復
    private void Update()
    {
        // 移除這裡的架勢恢復，讓 HealthPostureUI 來處理
        // healthPostureSystem.HandlePostureRecovery();
    }

    // 死亡
    private void OnDead(object sender, System.EventArgs e)
    {
        Debug.Log($"{gameObject.name} 死亡！");
        
        // 死亡時隱藏血條
        HideHealthBar();
        
        // 不立即關閉碰撞器，等待武器特效完成後由WeaponEffect系統來處理
        // 如果沒有武器特效系統，則使用延遲關閉作為備用方案
        StartCoroutine(DisableColliderAfterEffect());
        
        // 播放死亡動畫
    }

    // 等待武器特效完成後關閉碰撞器
    private IEnumerator DisableColliderAfterEffect()
    {
        // 等待武器特效完成（給予足夠時間讓特效觸發和播放）
        yield return new WaitForSeconds(1.0f);
        
        // 關掉敵人 collider
        DisableCollider();
    }

    // 立即關閉碰撞器（可被外部調用）
    public void DisableCollider()
    {
        if (colliderDisabled) return; // 避免重複關閉
        
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null && enemyCollider.enabled)
        {
            enemyCollider.enabled = false;
            colliderDisabled = true;
            Debug.Log($"{gameObject.name} 碰撞器已關閉");
        }
    }

    // 架勢被打破
    private void OnPostureBroken(object sender, System.EventArgs e)
    {
        Debug.Log($"{gameObject.name} 架勢被打破！");

        // 呼叫 OnDead 事件
        OnDead(sender, e);
        // 播放死亡動畫
    }

    // 獲取當前生命值百分比
    public float GetHealthPercentage()
    {
        return healthPostureSystem.GetHealthNormalized();
    }

    // 獲取當前架勢值百分比
    public float GetPosturePercentage()
    {
        return healthPostureSystem.GetPostureNormalized();
    }

    // 重置生命值系統
    public void ResetHealth()
    {
        // 重新創建生命值系統
        healthPostureSystem = new HealthPostureSystem(maxHealth, maxPosture);
        
        // 重新設定 UI
        if (healthPostureUI != null)
        {
            healthPostureUI.SetHealthPostureSystem(healthPostureSystem);
        }
        
        // 重新訂閱事件
        healthPostureSystem.OnDead += OnDead;
        healthPostureSystem.OnPostureBroken += OnPostureBroken;
        
        // 隱藏血條
        HideHealthBar();
        
        // 重置碰撞器狀態
        colliderDisabled = false;
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
    }

    // 當物件被銷毀時清理
    private void OnDestroy()
    {
        // 如果這個敵人是最後一個被攻擊的敵人，清除引用
        if (lastAttackedEnemy == this)
        {
            lastAttackedEnemy = null;
        }
    }
}
