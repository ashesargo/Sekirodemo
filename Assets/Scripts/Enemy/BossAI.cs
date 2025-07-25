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
    
    [Header("Combo 動畫持續時間設定")]
    public float combo1Duration = 15f; // Combo1 總持續時間
    public float combo2Duration = 12f; // Combo2 總持續時間  
    public float combo3Duration = 10f; // Combo3 總持續時間
    
    // === 武器切換與遠程攻擊動畫事件用 ===
    public GameObject meleeWeapon;   // 指到近戰武器 GameObject
    public GameObject rangedWeapon;  // 指到遠程武器 GameObject

    // === 左右手武器切換動畫事件用 ===
    public GameObject leftWeapon;   // 左手武器
    public GameObject rightWeapon;  // 右手武器

    // 動畫事件：切換武器
    public void SwitchWeapon(string type)
    {
        if (type == "melee")
        {
            if (meleeWeapon != null) meleeWeapon.SetActive(true);
            if (rangedWeapon != null) rangedWeapon.SetActive(false);
        }
        else if (type == "ranged")
        {
            if (meleeWeapon != null) meleeWeapon.SetActive(false);
            if (rangedWeapon != null) rangedWeapon.SetActive(true);
        }
    }

    // 動畫事件：發射遠程攻擊
    public void FireBossProjectile()
    {
        var ranged = GetComponent<BossRangedAttack>();
        if (ranged != null)
            ranged.FireBossProjectile();
    }

    // 動畫事件：簡化武器顯示切換
    public void ShowRangedWeapon(bool show)
    {
        if (show)
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
    
    private void Start()
    {
        // Boss特有的初始化
        animator = GetComponent<Animator>();
        // 不再尋找玩家，直接使用基底類別的CachedPlayer
        SwitchState(new BossIdleState());
        
        // 調試：檢查 obstacleMask 設置
        Debug.Log($"BossAI Start - obstacleMask: {obstacleMask.value}, avoidDistance: {avoidDistance}");
        
        // 檢查自己的 Collider
        Collider myCollider = GetComponent<Collider>();
        if (myCollider == null)
        {
            Debug.LogError($"Boss {gameObject.name} 沒有 Collider 組件！Boss避障需要 Collider 才能工作。");
        }
        else
        {
            Debug.Log($"Boss {gameObject.name} 有 Collider: {myCollider.GetType().Name}, Layer: {gameObject.layer}");
        }
        
        Debug.Log("BossAI: Boss初始化完成");
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
} 