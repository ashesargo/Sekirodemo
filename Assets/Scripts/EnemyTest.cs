using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    public int maxHP = 50;
    public int currentHP;
    private Animator _animator;
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
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        currentHP = maxHP;
    }
}
