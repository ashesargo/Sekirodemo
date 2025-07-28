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
    public enum HitState
    {
        Hit = 0,
        Guard = 1,
        Parry = 2,
    }

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
    }

    public void TakeDamage(float damage)
    {
        if (isDeath || _playerGrapple.IsGrappling()) return; // 死亡後不再受傷
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsTag("Parry"))
        {
            currentHitState = HitState.Parry; damage = 0;
        }
        else if (_TPContraller.isGuard) { currentHitState = HitState.Guard; damage = 0; }
        else currentHitState = HitState.Hit;
        // 使用 HealthPostureController 處理傷害
        if (healthController != null)
        {
            healthController.TakeDamage(Mathf.RoundToInt(damage), currentHitState);
        }
        // 檢查是否死亡
        if (GetCurrentHP() <= 0 && !isDeath)
        {
            isDeath = true;
            _animator.SetTrigger("Death");
        }
        else if (GetCurrentHP() > 0)
        {
            if (currentHitState == HitState.Parry) return;
            if (currentHitState == HitState.Guard) _animator.SetTrigger("GuardHit");
            else _animator.SetTrigger("Hit");
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
            _animator.ResetTrigger("Death");
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
