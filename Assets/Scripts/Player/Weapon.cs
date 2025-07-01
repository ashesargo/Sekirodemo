using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform pointA; // �M�ڡ]�ΤM�����q�^
    public Transform pointB; // �M�y
    public float radius = 0.1f;
    public int damage = 10;
    private Vector3 lastPointA, lastPointB;
    private bool isAttacking = false;
    private HashSet<Collider> alreadyHit = new HashSet<Collider>(); // �R���L���ĤH
    void Start()
    {
        lastPointA = pointA.position;
        lastPointB = pointB.position;
    }
    void Update()
    {
        if (!isAttacking) return;

        Vector3 currA = pointA.position;
        Vector3 currB = pointB.position;

        // �Q�� CapsuleCastBetweenPoints �ˬd���M�y��
        CheckCapsuleSweep(lastPointA, lastPointB, currA, currB);

        lastPointA = currA;
        lastPointB = currB;
    }
    void CheckCapsuleSweep(Vector3 fromA, Vector3 fromB, Vector3 toA, Vector3 toB)
    {
        Vector3 dirA = toA - fromA;
        Vector3 dirB = toB - fromB;
        float distA = dirA.magnitude;
        float distB = dirB.magnitude;
        // ���y�����]�i�H��@���M���L����m�^
        if (distA > 0)
        {
            RaycastHit[] hits = Physics.CapsuleCastAll(
                fromA, fromB, radius,
                ((toA + toB) - (fromA + fromB)).normalized,
                Mathf.Max(distA, distB)
            );
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Enemy") && !alreadyHit.Contains(hit.collider))
                {
                    Debug.Log("hit");
                    hit.collider.GetComponent<EnemyTest>().TakeDamage(damage);
                    alreadyHit.Add(hit.collider); // �[�J�R���M��
                }
            }
        }
    }
    public void WeaponStartAttack()
    {
        isAttacking = true;
        lastPointA = pointA.position;
        lastPointB = pointB.position;
        alreadyHit.Clear();
    }
    public void WeaponEndAttack()
    {
        isAttacking = false;
    }
}
