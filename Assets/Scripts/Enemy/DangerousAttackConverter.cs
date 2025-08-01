using UnityEngine;

public class DangerousAttackConverter : MonoBehaviour
{
    [Header("危攻擊轉換設定")]
    public bool enableDangerousConversion = true; // 是否啟用危攻擊轉換
    public float dangerousAttackDamage = 30f; // 危攻擊傷害
    public float dangerousAttackRange = 2.5f; // 危攻擊範圍
    public LayerMask playerLayer = 1 << 7; // Player層
    
    [Header("特效設定")]
    public GameObject dangerousAttackEffect; // 危攻擊特效
    public Transform attackPoint; // 攻擊點位置
    
    [Header("動畫事件設定")]
    public string[] dangerousAttackTriggers = { "DangerousAttack", "Attack" }; // 可以觸發危攻擊的動畫觸發器
    public string[] dangerousAttackStates = { "DangerousAttack", "Attack", "Combo1", "Combo2", "Combo3" }; // 可以轉換為危攻擊的動畫狀態
    
    private EnemyAI enemyAI;
    private bool isDangerousAttackActive = false;
    private bool isInDangerousState = false;

    void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        if (attackPoint == null)
        {
            attackPoint = transform;
        }
    }

    void Update()
    {
        // 檢查是否處於危攻擊狀態
        CheckDangerousAttackState();
    }

    void CheckDangerousAttackState()
    {
        if (!enableDangerousConversion || enemyAI == null) return;

        AnimatorStateInfo stateInfo = enemyAI.animator.GetCurrentAnimatorStateInfo(0);
        
        // 檢查是否正在播放可轉換的動畫
        bool isInConvertibleState = false;
        foreach (string stateName in dangerousAttackStates)
        {
            if (stateInfo.IsName(stateName))
            {
                isInConvertibleState = true;
                break;
            }
        }

        // 如果處於可轉換狀態且啟用了危攻擊轉換
        if (isInConvertibleState && !isInDangerousState)
        {
            isInDangerousState = true;
            // 找到當前動畫狀態名稱
            string currentStateName = "";
            foreach (string stateName in dangerousAttackStates)
            {
                if (stateInfo.IsName(stateName))
                {
                    currentStateName = stateName;
                    break;
                }
            }
            Debug.Log($"DangerousAttackConverter: 進入危攻擊轉換狀態 - {currentStateName}");
        }
        else if (!isInConvertibleState && isInDangerousState)
        {
            isInDangerousState = false;
            isDangerousAttackActive = false;
            Debug.Log("DangerousAttackConverter: 退出危攻擊轉換狀態");
        }
    }

    // === 動畫事件方法 ===

    // 開始危攻擊（可在任何攻擊動畫中調用）
    public void OnDangerousAttackStart()
    {
        if (!enableDangerousConversion) return;
        
        isDangerousAttackActive = true;
        Debug.Log("DangerousAttackConverter: 危攻擊開始！");
        
        // 播放危攻擊特效
        if (dangerousAttackEffect != null)
        {
            Vector3 effectPosition = attackPoint != null ? attackPoint.position : transform.position + transform.forward * 2f;
            Instantiate(dangerousAttackEffect, effectPosition, transform.rotation);
        }
    }

    // 危攻擊傷害判定（可在任何攻擊動畫中調用）
    public void OnDangerousAttackDamage()
    {
        if (!enableDangerousConversion || !isDangerousAttackActive) return;
        
        Debug.Log("DangerousAttackConverter: 執行危攻擊傷害判定");
        
        // 檢測範圍內的玩家
        Collider[] hits = Physics.OverlapSphere(transform.position, dangerousAttackRange, playerLayer);
        
        foreach (var hit in hits)
        {
            // 檢查是否在攻擊角度內
            Vector3 dirToPlayer = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            
            if (angle <= 120f) // 120度攻擊角度
            {
                // 對玩家造成危攻擊傷害
                PlayerStatus playerStatus = hit.GetComponent<PlayerStatus>();
                if (playerStatus != null)
                {
                    Debug.Log($"DangerousAttackConverter: 對玩家造成危攻擊傷害 {dangerousAttackDamage}");
                    playerStatus.TakeDangerousDamage(dangerousAttackDamage);
                }
            }
        }
    }

    // 結束危攻擊（可在任何攻擊動畫中調用）
    public void OnDangerousAttackEnd()
    {
        if (!enableDangerousConversion) return;
        
        isDangerousAttackActive = false;
        Debug.Log("DangerousAttackConverter: 危攻擊結束");
    }

    // === 通用攻擊事件方法（可與現有攻擊動畫共用） ===

    // 開始攻擊（通用）
    public void OnAttackStart()
    {
        Debug.Log("DangerousAttackConverter: 攻擊開始");
        
        // 如果啟用了危攻擊轉換，則轉換為危攻擊
        if (enableDangerousConversion && isInDangerousState)
        {
            OnDangerousAttackStart();
        }
    }

    // 攻擊傷害判定（通用）
    public void OnAttackDamage()
    {
        Debug.Log("DangerousAttackConverter: 執行攻擊傷害判定");
        
        // 如果啟用了危攻擊轉換，則執行危攻擊傷害
        if (enableDangerousConversion && isInDangerousState)
        {
            OnDangerousAttackDamage();
        }
        else
        {
            // 執行一般攻擊傷害邏輯
            PerformNormalAttackDamage();
        }
    }

    // 結束攻擊（通用）
    public void OnAttackEnd()
    {
        Debug.Log("DangerousAttackConverter: 攻擊結束");
        
        // 如果啟用了危攻擊轉換，則結束危攻擊
        if (enableDangerousConversion && isInDangerousState)
        {
            OnDangerousAttackEnd();
        }
    }

    // 一般攻擊傷害邏輯
    private void PerformNormalAttackDamage()
    {
        // 這裡可以添加一般攻擊的傷害邏輯
        // 一般攻擊可以被防禦，所以使用正常的傷害處理
        Debug.Log("DangerousAttackConverter: 執行一般攻擊傷害");
    }

    // === 動態危攻擊控制 ===

    // 啟用危攻擊轉換
    public void EnableDangerousConversion()
    {
        enableDangerousConversion = true;
        Debug.Log("DangerousAttackConverter: 危攻擊轉換已啟用");
    }

    // 禁用危攻擊轉換
    public void DisableDangerousConversion()
    {
        enableDangerousConversion = false;
        isDangerousAttackActive = false;
        isInDangerousState = false;
        Debug.Log("DangerousAttackConverter: 危攻擊轉換已禁用");
    }

    // 設置危攻擊傷害
    public void SetDangerousAttackDamage(float damage)
    {
        dangerousAttackDamage = damage;
        Debug.Log($"DangerousAttackConverter: 危攻擊傷害設置為 {damage}");
    }

    // 檢查是否正在進行危攻擊
    public bool IsDangerousAttackActive()
    {
        return isDangerousAttackActive && enableDangerousConversion;
    }

    // 檢查是否處於危攻擊狀態
    public bool IsInDangerousState()
    {
        return isInDangerousState && enableDangerousConversion;
    }

    // 在Scene視圖中顯示攻擊範圍（僅在選中時顯示）
    void OnDrawGizmosSelected()
    {
        if (!enableDangerousConversion) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerousAttackRange);
        
        // 顯示攻擊角度
        Vector3 forward = transform.forward;
        Vector3 left = Quaternion.Euler(0, -60, 0) * forward;
        Vector3 right = Quaternion.Euler(0, 60, 0) * forward;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, left * dangerousAttackRange);
        Gizmos.DrawRay(transform.position, right * dangerousAttackRange);
    }

    // === 調試方法 ===
    
    [ContextMenu("啟用危攻擊轉換")]
    void EnableDangerousConversionDebug()
    {
        EnableDangerousConversion();
    }
    
    [ContextMenu("禁用危攻擊轉換")]
    void DisableDangerousConversionDebug()
    {
        DisableDangerousConversion();
    }
    
    [ContextMenu("檢查危攻擊狀態")]
    void CheckDangerousAttackStatus()
    {
        Debug.Log($"DangerousAttackConverter 狀態檢查:");
        Debug.Log($"- 轉換啟用: {enableDangerousConversion}");
        Debug.Log($"- 危攻擊活躍: {isDangerousAttackActive}");
        Debug.Log($"- 危攻擊狀態: {isInDangerousState}");
        Debug.Log($"- 當前傷害: {dangerousAttackDamage}");
    }
} 