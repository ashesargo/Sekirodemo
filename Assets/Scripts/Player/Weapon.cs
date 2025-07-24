using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float attackRadius = 3f; // 攻擊範圍半徑
    public float attackAngle = 90f; // 攻擊角度
    public LayerMask targetLayer; // 敵人層級
    public int damage = 10;    
    private HashSet<Collider> alreadyHit = new HashSet<Collider>();
    
    public AudioClip AttackSwingSound;
    AudioSource _audioSource;
    private void Update()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    public void PerformFanAttack()
    {
        alreadyHit.Clear();
        // 找出半徑內的所有敵人
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, targetLayer);
        foreach (var hit in hits)
        {
            if (alreadyHit.Contains(hit)) continue;
            // 判斷是否在前方角度內
            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget); // 跟前方夾角

            if (angle <= attackAngle * 0.5f) // 扇形角度範圍內
            {
                hit.GetComponent<EnemyTest>()?.TakeDamage(damage);
                alreadyHit.Add(hit);
            }
        }
        _audioSource.PlayOneShot(AttackSwingSound);
        
    }

    // 可視化顯示範圍
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // 劃出扇形邊界
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * attackRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * attackRadius);
    } 
}



