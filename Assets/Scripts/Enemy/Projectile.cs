using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("子彈設定")]
    public float damage = 10f;
    public float lifetime = 10f; // 子彈存活時間
    public float speed = 20f;
    
    [Header("視覺效果")]
    public GameObject hitEffect; // 命中效果
    public AudioClip hitSound; // 命中音效
    
    [Header("碰撞設定")]
    public LayerMask targetLayers = 7; // 可以命中的層
    public bool destroyOnHit = true; // 命中後是否銷毀
    
    private float timer = 0f;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        // 檢查存活時間
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 檢查是否命中目標層
        if (((7 << other.gameObject.layer) & targetLayers) != 0)
        {
            if (other.CompareTag("Player"))
            {
                var status = other.GetComponent<PlayerStatus>();
                if (status != null)
                {
                    status.TakeDamage(damage);
                    Debug.Log($"Projectile: 對玩家造成 {damage} 點傷害");
                }
            }
            // 播放命中效果
            PlayHitEffect();
            // 播放命中音效
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
            // 銷毀子彈
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 檢查是否命中目標層
        if (((7 << collision.gameObject.layer) & targetLayers) != 0)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                var status = collision.gameObject.GetComponent<PlayerStatus>();
                if (status != null)
                {
                    status.TakeDamage(damage);
                    Debug.Log($"Projectile: 對玩家造成 {damage} 點傷害");
                }
            }
            // 播放命中效果
            PlayHitEffect();
            // 播放命中音效
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
            // 銷毀子彈
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }

    private void PlayHitEffect()
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);
            Destroy(effect, 2f); // 2秒後銷毀效果
        }
    }
} 