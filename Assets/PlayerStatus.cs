 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    // Start is called before the first frame update
    public float maxHp;
    float currentHp;
    Animator _animator;
    public bool isDeath= false;
    void Start()
    {
        _animator = GetComponent<Animator>();
        currentHp = maxHp;
    }
    public void TakeDamage(float damage)
    {
        if (isDeath) return;
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0 );
        _animator.SetTrigger("Hit");
        if ( currentHp == 0 )
        {
            _animator.SetTrigger("Death");
            isDeath = true;
        }
    }
}
