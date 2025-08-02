using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// 生命值與架勢條控制器
public class HealthPostureController : MonoBehaviour
{
    public int live = 0;   // 復活次數
    [SerializeField] private int maxLive = 0; // 最大生命球數
    [SerializeField] private int maxHealth = 100;   // 最大生命值
    [SerializeField] private int maxPosture = 100;  // 最大架勢值
    [SerializeField] public HealthPostureUI healthPostureUI;   // 生命值與架勢 UI 顯示
    [SerializeField] private GameObject deathUI;   // 死亡UI
    [SerializeField] private GameObject reviveEffectPrefab;   // 復活特效Prefab
    [SerializeField] private Transform reviveEffectPosition;   // 復活特效生成位置

    private HealthPostureSystem healthPostureSystem;    // 引用生命值與架勢系統
    private Coroutine hideUICoroutine;  // 隱藏 UI 的協程
    private static HealthPostureController lastAttackedEnemy;  // 最後一個被攻擊的敵人
    private bool colliderDisabled = false;  // 標記碰撞器是否已被關閉
    private bool isPlayerDead = false;  // 玩家是否已死亡
    [HideInInspector] public bool canIncreasePosture = true;  // 是否可以增加架勢值
    private bool canRevive = false; // 是否可復活
    private Coroutine deathCountdownCoroutine; // 死亡倒數協程
    private Animator playerAnimator;

    void Awake()
    {
        if (GetComponent<PlayerStatus>() != null)
        {
            InitPlayer();
        }
        else
        {
            InitEnemy();
        }
    }

    void Update()
    {
        bool isPlayer = GetComponent<PlayerStatus>() != null;
        if (isPlayer && isPlayerDead && canRevive && Input.GetMouseButtonDown(0))
        {
            if (live > 0)
            {
                ResurrectPlayer();
            }
            else
            {
                ReturnToMainMenu();
            }
        }
    }

    // 初始化玩家血條架勢條
    public void InitPlayer()
    {
        live = maxLive;
        maxHealth = PlayerStatus.maxHP;
        maxPosture = 100;
        
        healthPostureSystem = new HealthPostureSystem(maxHealth, maxPosture);
        
        if (healthPostureUI != null)
        {
            healthPostureUI.SetHealthPostureSystem(healthPostureSystem);
            healthPostureUI.UpdateLifeBalls(live, maxLive);
        }
        
        SubscribeToEvents();
        playerAnimator = GetComponent<Animator>();
        ResetPlayerState();
    }

    // 初始化敵人
    private void InitEnemy()
    {
        SetMaxHealthBasedOnComponent();
        healthPostureSystem = new HealthPostureSystem(maxHealth, maxPosture);
        
        if (maxLive < 0)
        {
            maxLive = 1;
        }
        
        bool isBoss = IsBoss();
        if (isBoss && healthPostureUI == null)
        {
            HealthPostureUI[] healthUIs = GetComponentsInChildren<HealthPostureUI>(true);
            if (healthUIs.Length > 0)
            {
                healthPostureUI = healthUIs[0];
            }
        }

        if (healthPostureUI != null)
        {
            healthPostureUI.SetHealthPostureSystem(healthPostureSystem);
            healthPostureUI.UpdateLifeBalls(live, maxLive);
            
            if (isBoss)
            {
                healthPostureUI.gameObject.SetActive(false);
            }
        }

        SubscribeToEvents();
        live = maxLive;
        playerAnimator = GetComponent<Animator>();
    }

    // 根據組件類型自動設定最大生命值
    private void SetMaxHealthBasedOnComponent()
    {
        if (GetComponent<PlayerStatus>() != null)
        {
            maxHealth = PlayerStatus.maxHP;
        }
        else if (GetComponent<EnemyTest>() != null)
        {
            maxHealth = EnemyTest.maxHP;
        }
    }

    // 訂閱事件
    private void SubscribeToEvents()
    {
        healthPostureSystem.OnDead += OnDead;
        healthPostureSystem.OnPostureBroken += OnPostureBroken;
    }

    // 重置玩家狀態
    private void ResetPlayerState()
    {
        isPlayerDead = false;
        canIncreasePosture = true;
        canRevive = false;
    }

    // 受到傷害
    public void TakeDamage(int amount, PlayerStatus.HitState hitState)
    {
        if (healthPostureSystem == null) return;

        healthPostureSystem.HealthDamage(amount);

        if (canIncreasePosture)
        {
            float postureReductionRate = 1f;
            ItemSystem itemSystem = GetComponent<ItemSystem>();
            if (itemSystem != null)
            {
                postureReductionRate = itemSystem.GetPostureReductionRate();
            }
            
            int basePostureAmount = 0;
            bool isParry = (hitState == PlayerStatus.HitState.Parry);
            
            switch (hitState)
            {
                case PlayerStatus.HitState.Hit:
                    basePostureAmount = Mathf.RoundToInt(healthPostureSystem.GetMaxPosture() * 0.3f); 
                    break;
                case PlayerStatus.HitState.Guard:
                    basePostureAmount = Mathf.RoundToInt(healthPostureSystem.GetMaxPosture() * 0.1f); 
                    break;
                case PlayerStatus.HitState.Parry:
                    basePostureAmount = 0; 
                    break;
            }
            
            int adjustedPostureAmount = Mathf.RoundToInt(basePostureAmount * postureReductionRate);
            healthPostureSystem.PostureIncrease(adjustedPostureAmount, isParry);
        }
        
        ShowHealthBar();
    }

    // 增加架勢
    public void AddPosture(int amount, bool isParry = false)
    {
        if (healthPostureSystem == null || !canIncreasePosture) return;

        healthPostureSystem.PostureIncrease(amount, isParry);
        ShowHealthBar();
    }

    // 顯示血條
    private void ShowHealthBar()
    {
        if (healthPostureSystem == null || healthPostureSystem.GetHealthNormalized() <= 0) return;
        
        bool isPlayer = GetComponent<PlayerStatus>() != null;
        
        if (isPlayer)
        {
            HandlePlayerHealthBar();
        }
        else
        {
            HandleEnemyHealthBar();
        }
    }

    // 處理玩家血條顯示
    private void HandlePlayerHealthBar()
    {
        float posturePercentage = healthPostureSystem.GetPostureNormalized();
        if (posturePercentage > 0.5f)
        {
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
                hideUICoroutine = null;
            }
        }
        
        if (healthPostureUI != null)
        {
            healthPostureUI.gameObject.SetActive(true);
        }
    }

    // 處理敵人血條顯示
    private void HandleEnemyHealthBar()
    {
        bool isBoss = IsBoss();
        
        if (isBoss)
        {
            HandleBossHealthBar();
        }
        else
        {
            HandleNormalEnemyHealthBar();
        }
    }

    // 處理Boss血條顯示
    private void HandleBossHealthBar()
    {
        bool isPlayerInBossZone = IsPlayerInBossZone();
        
        if (isPlayerInBossZone)
        {
            if (healthPostureUI != null)
            {
                healthPostureUI.gameObject.SetActive(true);
            }
            
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
                hideUICoroutine = null;
            }
            
            lastAttackedEnemy = this;
        }
        else
        {
            if (healthPostureUI != null)
            {
                healthPostureUI.gameObject.SetActive(false);
            }
        }
    }

    // 處理普通敵人血條顯示
    private void HandleNormalEnemyHealthBar()
    {
        if (lastAttackedEnemy != null && lastAttackedEnemy != this)
        {
            bool isPreviousEnemyBoss = lastAttackedEnemy.IsBoss();
            if (!isPreviousEnemyBoss)
            {
                lastAttackedEnemy.HideHealthBar();
            }
        }

        lastAttackedEnemy = this;

        if (healthPostureUI != null)
        {
            healthPostureUI.gameObject.SetActive(true);
        }

        if (hideUICoroutine != null)
        {
            StopCoroutine(hideUICoroutine);
        }

        if (gameObject.activeInHierarchy)
        {
            hideUICoroutine = StartCoroutine(HideHealthBarAfterDelay(5f));
        }
    }

    // 隱藏血條
    public void HideHealthBar()
    {
        bool isPlayer = GetComponent<PlayerStatus>() != null;

        if (isPlayer)
        {
            HandlePlayerHealthBarHide();
            return;
        }

        if (healthPostureUI != null)
        {
            healthPostureUI.gameObject.SetActive(false);
        }

        if (hideUICoroutine != null)
        {
            StopCoroutine(hideUICoroutine);
            hideUICoroutine = null;
        }

        if (lastAttackedEnemy == this)
        {
            lastAttackedEnemy = null;
        }
    }

    // 處理玩家血條隱藏
    private void HandlePlayerHealthBarHide()
    {
        if (healthPostureSystem == null) return;
        
        float posturePercentage = healthPostureSystem.GetPostureNormalized();
        if (posturePercentage > 0.5f)
        {
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
                hideUICoroutine = null;
            }
            return;
        }
        
        if (hideUICoroutine != null)
        {
            StopCoroutine(hideUICoroutine);
            hideUICoroutine = null;
        }
    }

    // 強制顯示血條（用於Boss）
    public void ForceShowHealthBar()
    {
        if (healthPostureUI == null)
        {
            HealthPostureUI[] healthUIs = GetComponentsInChildren<HealthPostureUI>(true);
            if (healthUIs.Length > 0)
            {
                healthPostureUI = healthUIs[0];
            }
            else
            {
                return;
            }
        }
        
        EnsureGameObjectHierarchyActive(gameObject);
        EnsureGameObjectHierarchyActive(healthPostureUI.gameObject);
        
        Transform current = healthPostureUI.transform;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }
            current = current.parent;
        }
        
        healthPostureUI.gameObject.SetActive(true);
        
        if (IsBoss())
        {
            if (healthPostureUI != null)
            {
                healthPostureUI.gameObject.SetActive(true);
            }
        }
        else if (gameObject.activeInHierarchy)
        {
            StartCoroutine(CheckHealthBarStateAfterFrame());
        }
    }
    
    // 確保 GameObject 及其所有父物件都是啟用的
    private void EnsureGameObjectHierarchyActive(GameObject targetObject)
    {
        if (targetObject == null) return;
        
        Transform current = targetObject.transform;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }
            current = current.parent;
        }
    }
    
    private IEnumerator CheckHealthBarStateAfterFrame()
    {
        yield return null;
        
        if (healthPostureUI != null && !healthPostureUI.gameObject.activeInHierarchy)
        {
            healthPostureUI.gameObject.SetActive(true);
            yield return null;
        }
    }

    // 檢查玩家是否在Boss區域內
    private bool IsPlayerInBossZone()
    {
        BossTriggerZone[] bossZones = FindObjectsOfType<BossTriggerZone>();
        
        foreach (BossTriggerZone zone in bossZones)
        {
            if (zone.bossObject != null)
            {
                string currentName = gameObject.name.Replace("(Clone)", "");
                string bossObjectName = zone.bossObject.name.Replace("(Clone)", "");
                
                if (currentName == bossObjectName || 
                    (bossObjectName == "Elite" || bossObjectName == "Boss") && 
                    (currentName == "Elite" || currentName == "Boss"))
                {
                    if (zone.IsPlayerInZone())
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }

    // 檢查是否為Boss
    public bool IsBoss()
    {
        BossTriggerZone[] bossZones = FindObjectsOfType<BossTriggerZone>();
        
        foreach (BossTriggerZone zone in bossZones)
        {
            if (zone.bossObject != null)
            {
                bool isSamePrefab = false;
                
                string currentName = gameObject.name.Replace("(Clone)", "");
                string bossObjectName = zone.bossObject.name.Replace("(Clone)", "");
                
                if (currentName == bossObjectName)
                {
                    isSamePrefab = true;
                }
                
                HealthPostureController zoneBossController = zone.bossObject.GetComponent<HealthPostureController>();
                if (zoneBossController == this)
                {
                    isSamePrefab = true;
                }
                
                if (isSamePrefab)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // 通知Boss死亡
    private void NotifyBossDeath()
    {
        BossTriggerZone[] bossZones = FindObjectsOfType<BossTriggerZone>();
        
        foreach (BossTriggerZone zone in bossZones)
        {
            if (zone.bossObject != null)
            {
                bool isSamePrefab = false;
                
                string currentName = gameObject.name.Replace("(Clone)", "");
                string bossObjectName = zone.bossObject.name.Replace("(Clone)", "");
                
                if (currentName == bossObjectName)
                {
                    isSamePrefab = true;
                }
                
                HealthPostureController zoneBossController = zone.bossObject.GetComponent<HealthPostureController>();
                if (zoneBossController == this)
                {
                    isSamePrefab = true;
                }
                
                if (bossObjectName == "Elite" || bossObjectName == "Boss")
                {
                    if (currentName == "Elite" || currentName == "Boss")
                    {
                        isSamePrefab = true;
                    }
                }
                
                if (isSamePrefab)
                {
                    zone.OnBossDeath();
                    break;
                }
            }
        }
    }

    // 延遲隱藏血條的協程
    private IEnumerator HideHealthBarAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (lastAttackedEnemy == this)
        {
            HideHealthBar();
        }
    }

    // 死亡
    private void OnDead(object sender, System.EventArgs e)
    {
        bool isPlayer = GetComponent<PlayerStatus>() != null;

        if (isPlayer)
        {
            HandlePlayerDeath();
        }
        else
        {
            HandleEnemyDeath();
        }
        
        if (healthPostureUI != null)
            healthPostureUI.UpdateLifeBalls(live, maxLive);
    }

    // 處理玩家死亡
    private void HandlePlayerDeath()
    {
        isPlayerDead = true;
        RemoveAllItemEffects();
        
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ShowDeathUIAfterDelay(1f));
        }
    }

    // 處理敵人死亡
    private void HandleEnemyDeath()
    {
        bool isBoss = IsBoss();
        
        if (live > 0)
        {
            ResurrectEnemy();
        }
        else
        {
            if (isBoss)
            {
                HideHealthBar();
                NotifyBossDeath();
            }
            else
            {
                HideHealthBar();
            }
            
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(DisableColliderAfterEffect());
            }
        }
    }

    // 失衡
    private void OnUnbalance(object sender, System.EventArgs e)
    {
        PlayerStatus playerStatus = GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.Sragger();
        }        
        
        EnemyTest enemyTest = GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.RemoveControl();
        }
    }

    // 延遲顯示死亡 UI 的協程
    private IEnumerator ShowDeathUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowDeathUI();
        
        if (GetComponent<PlayerStatus>() != null)
        {
            if (deathCountdownCoroutine != null)
                StopCoroutine(deathCountdownCoroutine);
            if (gameObject.activeInHierarchy)
            {
                deathCountdownCoroutine = StartCoroutine(DeathReviveCountdown());
            }
        }
    }

    // 顯示死亡 UI
    private void ShowDeathUI()
    {
        if (deathUI != null)
        {
            deathUI.SetActive(true);
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(StartDeathUIAnimationNextFrame());
            }
            canRevive = true;
        }
    }

    private IEnumerator StartDeathUIAnimationNextFrame()
    {
        yield return null;
        DeathUI deathUIScript = deathUI.GetComponent<DeathUI>();
        if (deathUIScript != null)
        {
            deathUIScript.StartDeathAnimation();
        }
    }

    // 隱藏死亡 UI
    public void HideDeathUI()
    {
        if (deathUI != null)
        {
            DeathUI deathUIScript = deathUI.GetComponent<DeathUI>();
            if (deathUIScript != null)
            {
                deathUIScript.StopAnimation();
                deathUIScript.ResetUI();
            }
            
            deathUI.SetActive(false);
        }
        canRevive = false;
        if (deathCountdownCoroutine != null)
        {
            StopCoroutine(deathCountdownCoroutine);
            deathCountdownCoroutine = null;
        }
    }

    // 復活玩家
    private void ResurrectPlayer()
    {
        live--;
        ResetHealth();
        HideDeathUI();
        isPlayerDead = false;

        PlayerStatus playerStatus = GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.ResetHealth();
        }
        
        PlayReviveEffect();
        
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("Death", false);
        }
        
        if (healthPostureUI != null)
            healthPostureUI.UpdateLifeBalls(live, maxLive);
    }

    // 復活敵人
    private void ResurrectEnemy()
    {
        if (live <= 0) return;
        
        live--;
        ResetHealth();
        ResetPosture();
        canIncreasePosture = true;
        RemoveAllItemEffects();
        
        ResetEnemyComponents();
        
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(DelayedAttackRestore(1f));
        }
        
        PlayReviveEffect();
        
        if (IsBoss())
        {
            // Boss的血條顯示邏輯由BossTriggerZone控制
        }
        else
        {
            ShowHealthBar();
        }
        
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("Death", false);
            playerAnimator.SetBool("Stagger", false);
            playerAnimator.SetBool("Hit", false);
            playerAnimator.SetTrigger("Idle");
        }
        
        if (healthPostureUI != null)
            healthPostureUI.UpdateLifeBalls(live, maxLive);
        
        // 確保敵人處於正常狀態
        if (healthPostureSystem != null && healthPostureSystem.GetHealthNormalized() <= 0f)
        {
            healthPostureSystem.SetHealthNormalized(0.1f);
        }
    }

    // 重置敵人組件
    private void ResetEnemyComponents()
    {
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            // 重新啟用AI組件（如果被禁用）
            if (!enemyAI.enabled)
            {
                enemyAI.enabled = true;
            }
            
            try
            {
                var idleState = new IdleState();
                enemyAI.SwitchState(idleState);
            }
            catch
            {
                ResetEnemyAIState(enemyAI);
            }
        }
        
        EnemyTest enemyTest = GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.RestoreControl();
        }
        
        if (enemyAI != null)
        {
            enemyAI.canAutoAttack = true;
            enemyAI.canBeParried = true;
        }
        
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null && !enemyCollider.enabled)
        {
            enemyCollider.enabled = true;
            colliderDisabled = false;
        }
        
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null && !characterController.enabled)
        {
            characterController.enabled = true;
        }
        
        if (characterController != null)
        {
            characterController.Move(Vector3.zero);
        }
        
        if (transform.position.y < -10f)
        {
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        }
    }

    // 回到主選單
    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    // 等待武器特效完成後關閉碰撞器
    private IEnumerator DisableColliderAfterEffect()
    {
        yield return new WaitForSeconds(0.1f);
        DisableCollider();
    }

    // 立即關閉碰撞器
    public void DisableCollider()
    {
        if (colliderDisabled) return;

        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null && enemyCollider.enabled)
        {
            enemyCollider.enabled = false;
            colliderDisabled = true;
        }
        
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null && characterController.enabled)
        {
            characterController.enabled = false;
        }
    }

    // 架勢被打破
    private void OnPostureBroken(object sender, System.EventArgs e)
    {
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.SwitchState(new StaggerState());
        }
        else
        {
            OnUnbalance(sender, e);
        }
        
        canIncreasePosture = false;
        
        if (enemyAI == null)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(RestorePostureAfterDelay(0.01f));
                StartCoroutine(RestoreControlAfterDelay(1f));
            }
        }
    }

    // 獲取當前生命值百分比
    public float GetHealthPercentage()
    {
        if (healthPostureSystem == null) return 0f;
        return healthPostureSystem.GetHealthNormalized();
    }

    // 獲取當前架勢值百分比
    public float GetPosturePercentage()
    {
        if (healthPostureSystem == null) return 0f;
        return healthPostureSystem.GetPostureNormalized();
    }

    // 重置生命值系統
    public void ResetHealth()
    {
        bool isPlayer = GetComponent<PlayerStatus>() != null;

        healthPostureSystem = new HealthPostureSystem(maxHealth, maxPosture);

        if (healthPostureUI != null)
        {
            healthPostureUI.SetHealthPostureSystem(healthPostureSystem);
            healthPostureUI.UpdateLifeBalls(live, maxLive);
        }

        SubscribeToEvents();
        
        if (isPlayer)
        {
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
                hideUICoroutine = null;
            }
        }
        else
        {
            bool isBoss = IsBoss();
            if (!isBoss && healthPostureUI != null)
            {
                healthPostureUI.gameObject.SetActive(false);
            }
            else if (isBoss && healthPostureUI != null)
            {
                healthPostureUI.gameObject.SetActive(false);
            }
            
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
                hideUICoroutine = null;
            }
        }

        colliderDisabled = false;
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
        
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        if (healthPostureUI != null)
            healthPostureUI.UpdateLifeBalls(live, maxLive);
    }

    // 當物件被銷毀時清理
    private void OnDestroy()
    {
        if (lastAttackedEnemy == this)
        {
            lastAttackedEnemy = null;
        }
    }

    // 延遲恢復控制的協程
    private IEnumerator RestoreControlAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnemyTest enemyTest = GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.RestoreControl();
        }
    }

    // 延遲恢復架勢的協程
    private IEnumerator RestorePostureAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(GraduallyRestorePosture(1f));
        }
        
        canIncreasePosture = true;
    }

    // 緩慢回復架勢值的協程
    private IEnumerator GraduallyRestorePosture(float duration)
    {
        if (healthPostureSystem == null) yield break;
        
        float startPosture = healthPostureSystem.GetPostureNormalized();
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float currentPosture = Mathf.Lerp(startPosture, 0f, progress);
            SetPostureValue(currentPosture);
            yield return null;
        }
        
        SetPostureValue(0f);
    }

    // 播放復活特效
    private void PlayReviveEffect()
    {
        if (reviveEffectPrefab != null)
        {
            Vector3 effectPosition = reviveEffectPosition != null ? 
                reviveEffectPosition.position : transform.position;
            
            GameObject reviveEffect = Instantiate(reviveEffectPrefab, effectPosition, Quaternion.identity);
            Destroy(reviveEffect, 3f);
        }
    }
    
    // 播放自訂復活特效（可指定特效Prefab）
    public void PlayCustomReviveEffect(GameObject customEffectPrefab, Vector3? customPosition = null, float destroyDelay = 3f)
    {
        if (customEffectPrefab != null)
        {
            Vector3 effectPosition = customPosition ?? transform.position;
            GameObject reviveEffect = Instantiate(customEffectPrefab, effectPosition, Quaternion.identity);
            Destroy(reviveEffect, destroyDelay);
        }
    }

    // 設定架勢值的方法
    public void SetPostureValue(float normalizedValue)
    {
        if (healthPostureSystem == null) return;
        healthPostureSystem.SetPostureNormalized(normalizedValue);
    }
    
    // 設定生命值的方法
    public void SetHealthValue(float normalizedValue)
    {
        if (healthPostureSystem == null) return;
        healthPostureSystem.SetHealthNormalized(normalizedValue);
    }
    
    // 重置架勢值為0
    public void ResetPosture()
    {
        SetPostureValue(0f);
    }

    // 治療生命值
    public void HealHealth(int healAmount)
    {
        if (healthPostureSystem == null) return;
        healthPostureSystem.HealthHeal(healAmount);
    }

    // 移除所有道具效果
    private void RemoveAllItemEffects()
    {
        ItemSystem itemSystem = GetComponent<ItemSystem>();
        if (itemSystem != null)
        {
            itemSystem.ResetItemEffects();
        }
    }

    private IEnumerator DeathReviveCountdown()
    {
        canRevive = true;
        float timer = 10f;
        while (timer > 0f)
        {
            if (!isPlayerDead) yield break;
            timer -= Time.deltaTime;
            yield return null;
        }
        
        canRevive = false;
        DeathUI deathUIScript = deathUI.GetComponent<DeathUI>();
        if (deathUIScript != null)
        {
            deathUIScript.StartRedFlash();
        }
        
        yield return new WaitForSeconds(5f);
        ReturnToMainMenu();
    }
    
    // 延遲恢復攻擊能力協程
    private IEnumerator DelayedAttackRestore(float delay)
    {
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.canAutoAttack = false;
        }
        
        yield return new WaitForSeconds(delay);
        
        if (enemyAI != null)
        {
            enemyAI.canAutoAttack = true;
        }
    }
    
    // 備用AI狀態重置方法
    private void ResetEnemyAIState(EnemyAI enemyAI)
    {
        if (enemyAI == null) return;
        
        // 重新啟用AI組件（如果被禁用）
        if (!enemyAI.enabled)
        {
            enemyAI.enabled = true;
        }
        
        enemyAI.canAutoAttack = true;
        enemyAI.canBeParried = true;
        
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("Stagger", false);
            playerAnimator.SetBool("Death", false);
            playerAnimator.SetBool("Hit", false);
            playerAnimator.SetTrigger("Idle");
        }
        
        EnemyTest enemyTest = GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.RestoreControl();
        }
        
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
            colliderDisabled = false;
        }
        
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
}
