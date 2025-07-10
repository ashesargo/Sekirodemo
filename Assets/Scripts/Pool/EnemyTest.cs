using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ĤH��Ƹ}��
public class EnemyTest : MonoBehaviour
{
    public int maxHP = 50;
    public int currentHP;
    private Animator _animator;
    public bool isDead = false;

    void Start()
    {
        _animator = GetComponent<Animator>();
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"TakeDamage called! HP: {currentHP}, isDead: {isDead}, state: {GetComponent<EnemyAI>()?.CurrentState?.GetType().Name}");
        if (isDead) return; // 死亡後不再受傷

        if (currentHP > 0)
        {
            currentHP -= damage;
            EnemyAI ai = GetComponent<EnemyAI>();
            if (currentHP > 0)
            {
                if (ai != null)
                    ai.SwitchState(new HitState());
            }
            else
            {
                isDead = true;
                if (ai != null)
                    ai.SwitchState(new DieState());
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
