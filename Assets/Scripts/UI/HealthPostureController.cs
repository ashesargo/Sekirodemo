using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// 生命值與架勢條控制器
public class HealthPostureController : MonoBehaviour
{
    public int live = 2;   // 復活次數
    [SerializeField] private int maxHealth = 100;   // 最大生命值
    [SerializeField] private int maxPosture = 100;  // 最大架勢值
    [SerializeField] private HealthPostureUI healthPostureUI;   // 生命值與架勢 UI 顯示
    [SerializeField] private GameObject deathUI;   // 死亡UI

    private HealthPostureSystem healthPostureSystem;    // 引用生命值與架勢系統
    private Coroutine hideUICoroutine;  // 隱藏 UI 的協程
    private static HealthPostureController lastAttackedEnemy;  // 最後一個被攻擊的敵人
    private bool colliderDisabled = false;  // 標記碰撞器是否已被關閉
    private bool isPlayerDead = false;  // 玩家是否已死亡

    void Awake()
    {
        // 自動檢測並設定最大生命值
        SetMaxHealthBasedOnComponent();

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

    void Update()
    {
        // 檢查是否為玩家且已死亡
        bool isPlayer = GetComponent<PlayerStatus>() != null;
        if (isPlayer && isPlayerDead)
        {
            // 檢測滑鼠左鍵點擊
            if (Input.GetMouseButtonDown(0))
            {
                // 檢查是否還有復活次數
                if (live > 0)
                {
                    ResurrectPlayer();
                }
                else
                {
                    // 沒有復活次數了，回到主選單
                    ReturnToMainMenu();
                }
            }
        }
    }

    // 根據組件類型自動設定最大生命值
    private void SetMaxHealthBasedOnComponent()
    {
        // 檢查是否有 PlayerStatus 組件（玩家）
        if (GetComponent<PlayerStatus>() != null)
        {
            maxHealth = PlayerStatus.maxHP;
        }
        // 檢查是否有 EnemyTest 組件（敵人）
        else if (GetComponent<EnemyTest>() != null)
        {
            maxHealth = EnemyTest.maxHP;
        }
        // 如果都沒有，使用預設值
    }

    // 受到傷害
    public void TakeDamage(int amount, int hitState)
    {
        healthPostureSystem.HealthDamage(amount);

        // 同時增加架勢值（每次受到傷害增加 10 點架勢）
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
        // 檢查血量是否小於等於 0，如果是則不顯示血條
        if (healthPostureSystem.GetHealthNormalized() <= 0)
        {
            return;
        }

        // 檢查是否為玩家
        bool isPlayer = GetComponent<PlayerStatus>() != null;

        if (isPlayer)
        {
            // 玩家血條始終顯示，不需要隱藏邏輯
            if (healthPostureUI != null)
            {
                healthPostureUI.gameObject.SetActive(true);
            }
        }
        else
        {
            // 敵人血條邏輯：隱藏其他敵人的血條
            if (lastAttackedEnemy != null && lastAttackedEnemy != this)
            {
                lastAttackedEnemy.HideHealthBar();
            }

            // 設定為最後一個被攻擊的敵人
            lastAttackedEnemy = this;

            // 顯示血條 UI
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
    }

    // 隱藏血條
    public void HideHealthBar()
    {
        // 檢查是否為玩家
        bool isPlayer = GetComponent<PlayerStatus>() != null;

        if (isPlayer)
        {
            // 玩家血條不隱藏，只停止協程
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
                hideUICoroutine = null;
            }
            return;
        }

        // 敵人血條隱藏邏輯
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

    // 死亡
    private void OnDead(object sender, System.EventArgs e)
    {
        // 檢查是否為玩家
        bool isPlayer = GetComponent<PlayerStatus>() != null;

        if (isPlayer)
        {
            // 設定玩家死亡狀態
            isPlayerDead = true;
            // 玩家死亡時延後一秒顯示死亡 UI
            StartCoroutine(ShowDeathUIAfterDelay(1f));
        }
        else
        {
            // 敵人死亡時隱藏血條
            HideHealthBar();
            // 關閉碰撞器
            StartCoroutine(DisableColliderAfterEffect());
        }
    }

    // 延遲顯示死亡 UI 的協程
    private IEnumerator ShowDeathUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowDeathUI();
    }

    // 顯示死亡 UI
    private void ShowDeathUI()
    {
        if (deathUI != null)
        {
            deathUI.SetActive(true);
        }
    }

    // 隱藏死亡 UI
    public void HideDeathUI()
    {
        if (deathUI != null)
        {
            deathUI.SetActive(false);
        }
    }

    // 復活玩家
    private void ResurrectPlayer()
    {
        // 減少一個復活次數
        live--;

        // 重置生命值系統
        ResetHealth();

        // 隱藏死亡UI
        HideDeathUI();

        // 重置死亡狀態
        isPlayerDead = false;

        // 重新開啟玩家控制器
        PlayerStatus playerStatus = GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.ResetHealth();
        }
    }

    // 回到主選單
    private void ReturnToMainMenu()
    {
        Debug.Log("遊戲結束 - 回到主選單");
        SceneManager.LoadScene("Main Menu");
    }

    // 等待武器特效完成後關閉碰撞器
    private IEnumerator DisableColliderAfterEffect()
    {
        // 等待武器特效完成
        yield return new WaitForSeconds(0.1f);

        // 關掉敵人 collider
        DisableCollider();
    }

    // 立即關閉碰撞器
    public void DisableCollider()
    {
        if (colliderDisabled) return; // 避免重複關閉

        // 關閉敵人碰撞器
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null && enemyCollider.enabled) // 如果敵人碰撞器存在且啟用
        {
            enemyCollider.enabled = false;
            colliderDisabled = true;
        }
    }

    // 架勢被打破
    private void OnPostureBroken(object sender, System.EventArgs e)
    {
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
        // 檢查是否為玩家
        bool isPlayer = GetComponent<PlayerStatus>() != null;

        if (isPlayer)
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
        }
        else
        {
            // 隱藏血條
            HideHealthBar();
        }

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
