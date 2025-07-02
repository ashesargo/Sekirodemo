using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform pointA; // ¤M®Ú
    public Transform pointB; // ¤M¦y
    public float radius = 0.2f;
    public LayerMask targetLayer;
    public int damage = 10;

    private bool isAttacking = false;
    private HashSet<Collider> alreadyHit = new HashSet<Collider>();

    void Update()
    {
        if (!isAttacking) return;

        Collider[] hits = Physics.OverlapCapsule(pointA.position, pointB.position, radius, targetLayer);
        foreach (var hit in hits)
        {
            if (!alreadyHit.Contains(hit))
            {
                alreadyHit.Add(hit);
                hit.GetComponent<EnemyTest>()?.TakeDamage(damage);
            }
        }
    }
    public void WeaponStartAttack()
    {
        isAttacking = true;  
        alreadyHit.Clear();
    }
    public void WeaponEndAttack()
    {
        isAttacking = false;
    }
}
