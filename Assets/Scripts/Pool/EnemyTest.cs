using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ĤH��Ƹ}��
public class EnemyTest : MonoBehaviour
{
    public int maxHP = 50;
    public int currentHP;
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (currentHP > 0)
        {
            currentHP -= damage;
            if (currentHP > 0)
            {
                _animator.SetTrigger("Hit");
            }
            else
            {
                _animator.SetTrigger("Death");
                // ����^���A�����`�ʵe���񧹲�
                StartCoroutine(ReturnToPoolAfterDeath());
            }
        }
    }

    // ���`�᩵��^��
    private IEnumerator ReturnToPoolAfterDeath()
    {
        // ���ݦ��`�ʵe���񧹲��]��2���^
        yield return new WaitForSeconds(2f);

        // �^���쪫���
        ObjectPool objectPool = ObjectPool.Instance;
        if (objectPool != null)
        {
            objectPool.ReturnPooledObject(gameObject);
        }

        //// �q�� EnemyPool �ĤH���`
        //EnemyPool enemyPool = FindObjectOfType<EnemyPool>();
        //if (enemyPool != null)
        //{
        //    enemyPool.OnEnemyDeath();
        //}
    }
}
