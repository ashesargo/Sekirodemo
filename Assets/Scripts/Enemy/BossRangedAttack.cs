using UnityEngine;

public class BossRangedAttack : MonoBehaviour
{
    [Header("Boss遠程攻擊設定")]
    public GameObject projectilePrefab; // Boss子彈預製體
    public Transform firePoint; // 發射點
    public float projectileSpeed = 25f; // 子彈速度
    public float projectileDamage = 30f; // 子彈傷害
    public AudioClip shootSound; // 發射音效
    public ParticleSystem muzzleFlash; // 發射特效

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
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    public void FireBossProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("BossRangedAttack: 沒有設定子彈預製體！");
            return;
        }
        // 生成子彈
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        // 設定子彈方向（朝向玩家）
        if (enemyAI.player != null)
        {
            Vector3 direction = (enemyAI.player.position - firePoint.position).normalized;
            projectile.transform.forward = direction;
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
        }
        // 播放發射音效
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        // 播放發射特效
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        Debug.Log("BossRangedAttack: Boss發射遠程攻擊");
    }
} 