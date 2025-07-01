using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform pointA; // 刀根（或刀身中段）
    public Transform pointB; // 刀尖
    public float radius = 0.1f;
    public int damage = 10;
    private Vector3 lastPointA, lastPointB;
    private bool isAttacking = false;
    private HashSet<Collider> alreadyHit = new HashSet<Collider>(); // 命中過的敵人
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

        // 利用 CapsuleCastBetweenPoints 檢查揮刀軌跡
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
        // 掃描兩條邊（可以當作整把刀揮過的位置）
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
                    alreadyHit.Add(hit.collider); // 加入命中清單
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
