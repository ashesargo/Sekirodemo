using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    [Header("射擊設定")]
    public GameObject projectilePrefab; // 子彈預製體
    public Transform firePoint; // 射擊點
    public float projectileSpeed = 20f; // 子彈速度
    public float projectileDamage = 10f; // 子彈傷害
    
    [Header("射擊音效")]
    public AudioClip shootSound; // 射擊音效
    public AudioClip reloadSound; // 裝彈音效
    
    [Header("視覺效果")]
    public ParticleSystem muzzleFlash; // 槍口火焰效果
    
    [Header("子彈目標（可選）")]
    public Transform projectileTarget; // 可在Inspector指定
    
    private AudioSource audioSource;
    private EnemyAI enemyAI;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        enemyAI = GetComponent<EnemyAI>();
        
        // 如果沒有設定射擊點，使用敵人位置
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    public void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("RangedEnemy: 沒有設定子彈預製體！");
            return;
        }

        // 生成子彈
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // 設定子彈方向（朝向玩家或指定目標）
        Vector3 targetPos;
        if (projectileTarget != null)
        {
            targetPos = projectileTarget.position;
        }
        else if (enemyAI.player != null)
        {
            targetPos = enemyAI.player.position + Vector3.up * 6f; // 預設胸口高度
        }
        else
        {
            targetPos = firePoint.position + firePoint.forward * 10f; // fallback
        }
        Vector3 direction = (targetPos - firePoint.position).normalized;
        projectile.transform.forward = direction;
        
        // 添加子彈移動組件
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            projectileRb.velocity = direction * projectileSpeed;
        }
        
        // 設定子彈傷害
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.damage = projectileDamage;
        }
        
        // 播放射擊音效
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        // 播放槍口火焰效果
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        
        Debug.Log("RangedEnemy: 發射子彈");
    }

    public void PlayReloadSound()
    {
        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
    }

    // 在Inspector中顯示射擊點
    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * 2f);
        }
    }
} 