using Tiny;
using UnityEngine;

public class BladeTrailController : MonoBehaviour
{
    [Header("Trail 設定")]
    public Trail bladeTrail; // 綁定 Mini Trail 組件
    public Animator animator; // 綁定角色 Animator

    private bool isInAttackState = false;

    void Update()
    {
        if (animator == null || bladeTrail == null) return;

        // 檢查目前動畫狀態是否為 Attack
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
        bladeTrail.Clear();           // 清除上次的軌跡
        bladeTrail.enabled = true;   // 啟用拖尾特效
    }

    void ExitAttackState()
    {
        isInAttackState = false;
        bladeTrail.enabled = false;  // 停止拖尾特效
    }
}
