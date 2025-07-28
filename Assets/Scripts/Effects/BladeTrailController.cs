using Tiny;
using UnityEngine;

public class BladeTrailController : MonoBehaviour
{
    [Header("Trail 設定")]
    public Trail bladeTrail; // 設定 Mini Trail
    public Animator animator; // 設定 Animator

    private bool isInAttackState = false;

    void Start()
    {
        // 初始化 Trail
        bladeTrail.Clear();
        bladeTrail.enabled = false;
    }

    void Update()
    {
        if (animator == null || bladeTrail == null) return;

        // 檢查是否為攻擊狀態
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        bool isAttack = state.IsTag("Attack");

        if (isAttack && !isInAttackState)
        {
            EnterAttackState();
        }
        else if (!isAttack && isInAttackState)
        {
            ExitAttackState();
        }
    }

    void EnterAttackState()
    {
        isInAttackState = true;
        bladeTrail.Clear();           // 清除 Trail
        bladeTrail.enabled = true;   // 啟用 Trail
    }

    void ExitAttackState()
    {
        isInAttackState = false;
        bladeTrail.enabled = false;  // 停用 Trail
    }
}
