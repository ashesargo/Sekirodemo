using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform pointA; // ¤M®Ú
    public Transform pointB; // ¤M¦y
    Vector3 lastA, lastB;
    public LayerMask targetLayer;
    public int damage = 10;
    private bool isAttacking = false;
    private HashSet<Collider> alreadyHit = new HashSet<Collider>();    
    void LateUpdate()
    {
        Vector3 currA = pointA.position;
        Vector3 currB = pointB.position;
        Vector3 prevA = lastA;
        Vector3 prevB = lastB;
        lastA = currA;
        lastB = currB;
        Vector3 center = (currA + currB + prevA + prevB) / 4f;

        float length = Vector3.Distance(currA, currB);
        float movement = Mathf.Max(
            Vector3.Distance(currA, prevA),
            Vector3.Distance(currB, prevB)
        );
        Vector3 halfExtents = new Vector3(0.5f, movement / 2f, length / 2f);
        Vector3 forward = (currA - currB).normalized;
        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);             
        if (!isAttacking) return;
        Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation);
        foreach (var hit in hits)
        {
            if (!alreadyHit.Contains(hit))
            {
                alreadyHit.Add(hit);
                hit.GetComponent<EnemyTest1>()?.TakeDamage(damage);
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
