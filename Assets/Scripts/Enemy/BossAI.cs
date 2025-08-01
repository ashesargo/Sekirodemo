using UnityEngine;

public class BossAI : EnemyAI
{
    [Header("Boss 攻擊設定")]
    public int currentComboCount = 0; // 當前連擊次數
    public int maxComboCount = 3; // 最大連擊次數
    
    public bool hasDoneIntroCombo = false; // 是否已完成開場Combo
    public bool hasJustRangedOrRushed = false; // 是否剛做過遠程或突進
    public int pendingIntroComboStep = -1; // -1表示沒有待續的開場Combo
    public bool pendingJumpAttack = false; // 是否有待執行的JumpAttack
    
    [Header("Boss 攻擊動畫")]
    public string[] comboAnimations = { "Combo1", "Combo2", "Combo3" };
    
    [Header("Boss 危攻擊設定")]
    public string[] dangerousAttackAnimations = { "DangerousAttack1", "DangerousAttack2", "DangerousAttack3" };
    public float dangerousAttackChance = 0.2f; // 危攻擊觸發機率
    public float dangerousAttackCooldown = 10f; // 危攻擊冷卻時間
    private float lastDangerousAttackTime = 0f; // 上次危攻擊時間
    
    [Header("危攻擊轉換設定")]
    public bool useDangerousAttackConverter = true; // 是否使用危攻擊轉換器
    public float dangerousAttackDamage = 30f; // 危攻擊傷害
    private DangerousAttackConverter dangerousAttackConverter;
    
    [Header("Combo 動畫持續時間設定")]
    public float combo1Duration = 15f; // Combo1 總持續時間
    public float combo2Duration = 12f; // Combo2 總持續時間  
    public float combo3Duration = 10f; // Combo3 總持續時間
    
    [Header("Boss 防禦設定")]
    [Range(0f, 1f)]
    public float bossDefendChance = 0.8f; // Boss防禦機率，可在Inspector中調整
    
    [Header("Boss 架勢值設定")]
    [Range(0, 100)]
    public int bossDefendPostureIncrease = 15; // Boss防禦時增加的架勢值
    [Range(0, 100)]
    public int bossHitPostureIncrease = 25; // Boss受傷時增加的架勢值

    // === 左右手武器切換動畫事件用 ===
    public GameObject leftWeapon;   // 左手武器（弓/遠程）
    public GameObject rightWeapon;  // 右手武器（刀/近戰）

    // 動畫事件：簡化武器顯示切換（int 版本，1=顯示弓，0=顯示近戰）
    public void ShowRangedWeapon(int show)
    {
        bool isShow = show != 0;
        if (isShow)
        {
            if (rightWeapon != null) rightWeapon.SetActive(false);
            if (leftWeapon != null) leftWeapon.SetActive(true);
        }
        else
        {
            if (rightWeapon != null) rightWeapon.SetActive(true);
            if (leftWeapon != null) leftWeapon.SetActive(false);
        }
    }

    // 動畫事件：切換武器顯示（如需更細緻控制）
    public void ShowWeapon(string hand)
    {
        if (hand == "left")
        {
            if (leftWeapon != null) leftWeapon.SetActive(true);
            if (rightWeapon != null) rightWeapon.SetActive(false);
        }
        else if (hand == "right")
        {
            if (leftWeapon != null) leftWeapon.SetActive(false);
            if (rightWeapon != null) rightWeapon.SetActive(true);
        }
    }
    
    private void Start()
    {
        // Boss特有的初始化
        animator = GetComponent<Animator>();
        // 不再尋找玩家，直接使用基底類別的CachedPlayer
        SwitchState(new BossIdleState());
        
        // 初始化危攻擊轉換器
        if (useDangerousAttackConverter)
        {
            dangerousAttackConverter = GetComponent<DangerousAttackConverter>();
            if (dangerousAttackConverter == null)
            {
                dangerousAttackConverter = gameObject.AddComponent<DangerousAttackConverter>();
            }
            dangerousAttackConverter.SetDangerousAttackDamage(dangerousAttackDamage);
        }
        
        // 調試：檢查 obstacleMask 設置
        Debug.Log($"BossAI Start - obstacleMask: {obstacleMask.value}, avoidDistance: {avoidDistance}");
        
        // 檢查自己的 CharacterController
        CharacterController myController = GetComponent<CharacterController>();
        if (myController == null)
        {
            Debug.LogError($"Boss {gameObject.name} 沒有 CharacterController 組件！Boss移動需要 CharacterController 才能工作。");
        }
        else
        {
            Debug.Log($"Boss {gameObject.name} 有 CharacterController: {myController.GetType().Name}, Layer: {gameObject.layer}");
        }
        
        Debug.Log("BossAI: Boss初始化完成");
    }
    
    void Awake()
    {
        if (leftWeapon == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "SM_Bow_02")
                {
                    leftWeapon = t.gameObject;
                    break;
                }
            }
        }
        if (rightWeapon == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Katana")
                {
                    rightWeapon = t.gameObject;
                    break;
                }
            }
        }
    }
    
    // 移除FindBossPlayer方法
    
    // 獲取下一個連擊動畫
    public string GetNextComboAnimation()
    {
        Debug.Log($"BossAI: GetNextComboAnimation - 當前連擊計數: {currentComboCount}, 最大連擊數: {maxComboCount}");
        
        if (currentComboCount < maxComboCount)
        {
            // 前三次按順序播放
            string nextAnimation = comboAnimations[currentComboCount];
            Debug.Log($"BossAI: 按順序播放連擊 {currentComboCount + 1}, 動畫: {nextAnimation}");
            return nextAnimation;
        }
        else
        {
            // 第四次開始隨機選擇
            int randomIndex = Random.Range(0, comboAnimations.Length);
            string randomAnimation = comboAnimations[randomIndex];
            Debug.Log($"BossAI: 隨機選擇連擊動畫, 索引: {randomIndex}, 動畫: {randomAnimation}");
            return randomAnimation;
        }
    }
    
    // 增加連擊計數
    public void IncrementComboCount()
    {
        currentComboCount++;
        Debug.Log($"BossAI: 連擊計數增加到 {currentComboCount}");
    }
    
    // 重置連擊計數
    public void ResetComboCount()
    {
        currentComboCount = 0;
        Debug.Log("BossAI: 連擊計數重置");
    }
    
    // 檢查是否為Boss
    public bool IsBoss()
    {
        return true;
    }
    
    // 根據combo名稱獲取對應的持續時間
    public float GetComboDuration(string comboName)
    {
        switch (comboName)
        {
            case "Combo1":
                return combo1Duration;
            case "Combo2":
                return combo2Duration;
            case "Combo3":
                return combo3Duration;
            default:
                return 15f; // 預設值
        }
    }
    
    // 調試：顯示當前連擊狀態
    public void DebugComboStatus()
    {
        Debug.Log($"BossAI Debug - 當前連擊計數: {currentComboCount}, 最大連擊數: {maxComboCount}");
        Debug.Log($"BossAI Debug - 下一個動畫: {GetNextComboAnimation()}");
        Debug.Log($"BossAI Debug - 動畫陣列: [{string.Join(", ", comboAnimations)}]");
    }

    // 新增：獲取危攻擊動畫
    public string GetDangerousAttackAnimation()
    {
        if (dangerousAttackAnimations.Length > 0)
        {
            // 隨機選擇一個危攻擊動畫
            int randomIndex = Random.Range(0, dangerousAttackAnimations.Length);
            return dangerousAttackAnimations[randomIndex];
        }
        return "DangerousAttack"; // 預設危攻擊動畫
    }

    // 新增：檢查是否可以執行危攻擊
    public bool CanPerformDangerousAttack()
    {
        return Time.time - lastDangerousAttackTime >= dangerousAttackCooldown;
    }

    // 新增：執行危攻擊
    public void PerformDangerousAttack()
    {
        if (CanPerformDangerousAttack())
        {
            lastDangerousAttackTime = Time.time;
            SwitchState(new BossDangerousAttackState());
        }
    }

    // 新增：檢查是否應該觸發危攻擊
    public bool ShouldTriggerDangerousAttack()
    {
        if (!CanPerformDangerousAttack()) return false;
        
        // 根據機率決定是否觸發危攻擊
        return Random.value < dangerousAttackChance;
    }

    // 新增：使用危攻擊轉換器啟用危攻擊
    public void EnableDangerousAttack()
    {
        if (useDangerousAttackConverter && dangerousAttackConverter != null)
        {
            dangerousAttackConverter.EnableDangerousConversion();
            Debug.Log("BossAI: 危攻擊轉換已啟用");
        }
    }

    // 新增：使用危攻擊轉換器禁用危攻擊
    public void DisableDangerousAttack()
    {
        if (useDangerousAttackConverter && dangerousAttackConverter != null)
        {
            dangerousAttackConverter.DisableDangerousConversion();
            Debug.Log("BossAI: 危攻擊轉換已禁用");
        }
    }

    // 新增：設置危攻擊傷害
    public void SetDangerousAttackDamage(float damage)
    {
        dangerousAttackDamage = damage;
        if (useDangerousAttackConverter && dangerousAttackConverter != null)
        {
            dangerousAttackConverter.SetDangerousAttackDamage(damage);
        }
    }
} 