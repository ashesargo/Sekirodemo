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
    private bool isAttacking = false; // 新增：防止重複攻擊
    private float lastAttackTime = 0f; // 新增：記錄上次攻擊時間
    private const float ATTACK_COOLDOWN = 0.1f; // 新增：攻擊冷卻時間
    
    public AudioClip AttackSwingSound;
    AudioSource _audioSource;
    
    private void Update()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    public void PerformFanAttack()
    {
        // 防止重複攻擊
        if (isAttacking || Time.time - lastAttackTime < ATTACK_COOLDOWN)
        {
            Debug.Log($"[Weapon] PerformFanAttack - 忽略重複攻擊，距離上次攻擊: {Time.time - lastAttackTime:F3}秒");
            return;
        }
        
        lastAttackTime = Time.time;
        isAttacking = true;
        
        alreadyHit.Clear();
        Debug.Log($"[Weapon] PerformFanAttack - 開始攻擊，時間: {Time.time:F3}");
        
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
                Debug.Log($"[Weapon] 對敵人 {hit.name} 造成 {damage} 點傷害");
            }
        }
        
        if (_audioSource != null && AttackSwingSound != null)
        {
            _audioSource.PlayOneShot(AttackSwingSound);
        }
        
        // 延遲重置攻擊狀態
        StartCoroutine(ResetAttackState());
    }
    
    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(ATTACK_COOLDOWN);
        isAttacking = false;
        Debug.Log("[Weapon] 攻擊狀態已重置");
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



