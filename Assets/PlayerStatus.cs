using UnityEditor;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public static int maxHP = 100;  // 改為靜態變數，與 EnemyTest 保持一致
    private Animator _animator;
    public bool isDeath = false;
    private HealthPostureController healthController;  // 引用生命值控制器
    TPContraller _TPContraller;
    PlayerGrapple _playerGrapple;
    public HitState currentHitState;
    AnimatorStateInfo stateInfo;
    public enum HitState
    {
        Hit = 0,
        Guard = 1,
        Parry = 2,
        DangerousHit = 3, // 新增：危攻擊傷害狀態
    }

    // 新增：受傷事件
    public System.Action<Vector3> OnHitOccurred; // 當受傷時觸發，參數為受傷位置

    void Start()
    {
        _animator = GetComponent<Animator>();
        _TPContraller = GetComponent<TPContraller>();
        // 獲取 HealthPostureController 組件
        healthController = GetComponent<HealthPostureController>();
        _playerGrapple = GetComponent<PlayerGrapple>();
        // 如果沒有 HealthPostureController，創建一個
        if (healthController == null)
        {
            healthController = gameObject.AddComponent<HealthPostureController>();
        }
        stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

    }

    public void TakeDamage(float damage)
    {
        if (isDeath || _playerGrapple.IsGrappling() || stateInfo.IsTag("Execution")) return; // 死亡後不再受傷
        if (stateInfo.IsTag("Parry"))
        {
            currentHitState = HitState.Parry; damage = 0;
            
            // 注意：當玩家處於Parry狀態時，表示玩家正在Parry敵人
            // 此時不應該增加玩家自己的架勢值，只有被Parry的敵人會增加架勢值
            // 玩家被敵人Parry的情況會在敵人的攻擊邏輯中處理
            
            // 移除：不觸發玩家Parry的特效事件，避免與 TPController 的 OnParrySuccess 事件衝突
            // 特效應該由 TPController 的 OnParrySuccess 事件統一處理
        }
        else if (_TPContraller.isGuard) { currentHitState = HitState.Guard; damage = 0; }
        else currentHitState = HitState.Hit;
        
        // 觸發受傷事件（只在非 Parry 狀態下觸發）
        if (currentHitState == HitState.Hit && OnHitOccurred != null)
        {
            Vector3 hitPosition = transform.position + transform.forward * 2f; // 在玩家前方生成特效
            OnHitOccurred.Invoke(hitPosition);
        }
        
        // 觸發防禦事件（當在防禦狀態下受到攻擊時）
        if (currentHitState == HitState.Guard && _TPContraller != null && _TPContraller.OnGuardSuccess != null)
        {
            Vector3 guardPosition = transform.position + transform.forward * 2f; // 在玩家前方生成特效
            _TPContraller.OnGuardSuccess.Invoke(guardPosition);
        }
        
        // 使用 HealthPostureController 處理傷害
        if (healthController != null)
        {
            healthController.TakeDamage(Mathf.RoundToInt(damage), currentHitState);
        }
        // 檢查是否死亡
        if (GetCurrentHP() <= 0 && !isDeath)
        {
            isDeath = true;
            _animator.Play("Death", 0, 0f); // 直接播放 Death 動畫
            _animator.Update(0f);
            _animator.SetBool("Death", true); // 設置 Death 參數，確保動畫控制器保持死亡狀態
        }
        else if (GetCurrentHP() > 0)
        {
            if (currentHitState == HitState.Parry) return;
            if (currentHitState == HitState.Guard) _animator.SetTrigger("GuardHit");
            else _animator.SetTrigger("Hit");
        }
    }
    private void Update()
    {
        stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (isDeath && !stateInfo.IsTag("Death"))
        {
            _animator.Play("Death", 0, 0f); // 直接播放 Death 動畫
            _animator.SetBool("Death", true); // 設置 Death 參數，確保動畫控制器保持死亡狀態
        }
    }

    // 新增：處理危攻擊傷害（無視防禦）
    public void TakeDangerousDamage(float damage)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (isDeath || _playerGrapple.IsGrappling() || stateInfo.IsTag("Execution")) return; // 死亡後不再受傷
        
        // 危攻擊無視防禦和Parry狀態
        currentHitState = HitState.DangerousHit;
        
        Debug.Log($"PlayerStatus: 受到危攻擊傷害 {damage}，無視防禦！");
        
        // 觸發危攻擊受傷事件
        if (OnHitOccurred != null)
        {
            Vector3 hitPosition = transform.position + transform.forward * 2f; // 在玩家前方生成特效
            OnHitOccurred.Invoke(hitPosition);
        }
        
        // 使用 HealthPostureController 處理傷害
        if (healthController != null)
        {
            healthController.TakeDamage(Mathf.RoundToInt(damage), currentHitState);
        }

        // 檢查是否死亡
        if (GetCurrentHP() <= 0 && !isDeath)
        {
            isDeath = true;
            _animator.Play("Death", 0, 0f); // 直接播放 Death 動畫
            _animator.SetBool("Death", true); // 設置 Death 參數，確保動畫控制器保持死亡狀態
        }
        else if (GetCurrentHP() > 0)
        {
            // 播放危攻擊受傷動畫
            _animator.SetTrigger("Hit");
        }
    }

    // 獲取當前生命值
    public int GetCurrentHP()
    {
        if (healthController != null)
        {
            return Mathf.RoundToInt(healthController.GetHealthPercentage() * maxHP);
        }
        return 0;
    }

    // 獲取當前生命值百分比
    public float GetHealthPercentage()
    {
        if (healthController != null)
        {
            return healthController.GetHealthPercentage();
        }
        return 0f;
    }

    // 獲取當前架勢值百分比
    public float GetPosturePercentage()
    {
        if (healthController != null)
        {
            return healthController.GetPosturePercentage();
        }
        return 0f;
    }

    // 重置生命值系統
    public void ResetHealth()
    {
        if (healthController != null)
        {
            healthController.ResetHealth();
        }
        isDeath = false;
        
        // 重置動畫狀態，確保玩家可以正常操作
        if (_animator != null)
        {
            // 重置所有可能影響操作的動畫參數
            _animator.ResetTrigger("Hit");
            _animator.ResetTrigger("GuardHit");
            _animator.ResetTrigger("Stagger");            
            // 確保動畫回到正常狀態
            _animator.SetBool("isGrounded", true);
        }
    }
    public void Sragger()
    {

        _animator.SetTrigger("Stagger");
    }
}
