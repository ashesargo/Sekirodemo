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
            _animator.SetTrigger("Hit");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        currentHP = maxHP;
    }
    // Update is called once per frame
    void Update()
    {
        if (currentHP <= 0)
        {
            _animator.SetTrigger("Death");
        }
    }
}
