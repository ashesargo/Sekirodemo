using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public static int maxHP = 100;  // 改為靜態變數，與 EnemyTest 保持一致
    private Animator _animator;
    public bool isDeath = false;
    private HealthPostureController healthController;  // 引用生命值控制器
    TPContraller _TPContraller;
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
        // 如果沒有 HealthPostureController，創建一個
        if (healthController == null)
        {
            healthController = gameObject.AddComponent<HealthPostureController>();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDeath) return; // 死亡後不再受傷
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
            if (currentHitState == HitState.Parry)
            {
                _TPContraller.parrySuccess = false;
            }
            else if (currentHitState == HitState.Guard) _animator.SetTrigger("GuardHit");
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
    }
    public void Sragger()
    {

        _animator.SetTrigger("Stagger");
    }
}
