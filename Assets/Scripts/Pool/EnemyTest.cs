using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敵人資料腳本
public class EnemyTest1 : MonoBehaviour
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
                // 延遲回收，讓死亡動畫播放完畢
                StartCoroutine(ReturnToPoolAfterDeath());
            }
        }
    }

    // 死亡後延遲回收
    private IEnumerator ReturnToPoolAfterDeath()
    {
        // 等待死亡動畫播放完畢（約2秒）
        yield return new WaitForSeconds(2f);

        // 回收到物件池
        ObjectPool objectPool = ObjectPool.Instance;
        if (objectPool != null)
        {
            objectPool.ReturnPooledObject(gameObject);
        }

        //// 通知 EnemyPool 敵人死亡
        //EnemyPool enemyPool = FindObjectOfType<EnemyPool>();
        //if (enemyPool != null)
        //{
        //    enemyPool.OnEnemyDeath();
        //}
    }
}
