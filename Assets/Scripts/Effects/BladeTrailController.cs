using Tiny;
using UnityEngine;

public class BladeTrailController : MonoBehaviour
{
    [Header("Trail �]�w")]
    public Trail bladeTrail; // �j�w Mini Trail �ե�
    public Animator animator; // �j�w���� Animator

    private bool isInAttackState = false;

    void Update()
    {
        if (animator == null || bladeTrail == null) return;

        // �ˬd�ثe�ʵe���A�O�_�� Attack
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
        bladeTrail.Clear();           // �M���W�����y��
        bladeTrail.enabled = true;   // �ҥΩ���S��
    }

    void ExitAttackState()
    {
        isInAttackState = false;
        bladeTrail.enabled = false;  // �������S��
    }
}
