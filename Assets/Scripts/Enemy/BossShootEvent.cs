using UnityEngine;

public class BossShootEvent : MonoBehaviour
{
    [Header("射擊設定")]
    public GameObject arrowPrefab; // 箭矢預製體
    public Transform firePoint; // 發射點
    public float arrowSpeed = 25f; // 箭矢速度
    public float arrowDamage = 30f; // 箭矢傷害
    
    [Header("箭矢方向修正")]
    public Vector3 arrowRotationOffset = Vector3.zero; // 箭矢旋轉偏移
    public bool useCustomRotation = false; // 是否使用自定義旋轉
    
    [Header("音效和特效")]
    public AudioClip shootSound; // 發射音效
    public ParticleSystem shootEffect; // 發射特效
    
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
        
        // 自動尋找發射點
        if (firePoint == null)
        {
            // 嘗試尋找弓或武器物件
            Transform bow = FindChildRecursive(transform, "SM_Bow_02");
            if (bow != null)
            {
                firePoint = bow;
                Debug.Log("BossShootEvent: 找到弓物件作為發射點");
            }
            else
            {
                // 如果找不到弓，使用Boss位置但加上高度偏移
                firePoint = transform;
                Debug.Log("BossShootEvent: 使用Boss位置作為發射點（請手動設定firePoint）");
            }
        }
        
        Debug.Log($"BossShootEvent: 射擊事件系統初始化完成，發射點: {firePoint.name}");
    }
    
    // 遞迴尋找子物件
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent.name == childName)
            return parent;
            
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            Transform result = FindChildRecursive(child, childName);
            if (result != null)
                return result;
        }
        
        return null;
    }
    
    // 確保在任何狀態下都能找到玩家
    private Transform GetPlayerTarget()
    {
        // 優先使用EnemyAI的快取玩家
        if (enemyAI != null && enemyAI.player != null)
        {
            return enemyAI.player;
        }
        
        // 如果EnemyAI沒有玩家，嘗試直接尋找
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            return playerObj.transform;
        }
        
        Debug.LogWarning("BossShootEvent: 找不到玩家目標");
        return null;
    }

    // 基本射擊事件（動畫事件調用）
    public void ShootArrow()
    {
        Transform playerTarget = GetPlayerTarget();
        if (playerTarget == null)
        {
            return;
        }
        
        if (arrowPrefab == null)
        {
            Debug.LogWarning("BossShootEvent: 沒有設定箭矢預製體！");
            return;
        }
        
        // 計算發射位置（避免卡進地板）
        Vector3 spawnPosition = firePoint.position;
        if (firePoint == transform)
        {
            // 如果發射點是Boss本身，加上高度偏移避免卡進地板
            spawnPosition += Vector3.up * 1.5f;
        }
        
        // 生成箭矢
        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);
        
        // 計算方向（瞄準玩家上方6單位）
        Vector3 targetPos = playerTarget.position + Vector3.up * 6f;
        Vector3 direction = (targetPos - spawnPosition).normalized;
        
        // 設定箭矢方向（修正箭頭朝向）
        if (useCustomRotation)
        {
            // 使用自定義旋轉
            arrow.transform.rotation = Quaternion.Euler(arrowRotationOffset);
        }
        else
        {
            // 使用LookRotation並應用偏移
            arrow.transform.rotation = Quaternion.LookRotation(direction);
            if (arrowRotationOffset != Vector3.zero)
            {
                arrow.transform.Rotate(arrowRotationOffset);
            }
        }
        
        // 設定箭矢速度
        Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();
        if (arrowRb != null)
        {
            // 確保Rigidbody是啟用的
            arrowRb.isKinematic = false;
            arrowRb.useGravity = true;
            arrowRb.velocity = direction * arrowSpeed;
            
            Debug.Log($"BossShootEvent: 箭矢速度設定為 {direction * arrowSpeed}");
        }
        else
        {
            Debug.LogError("BossShootEvent: 箭矢沒有Rigidbody組件！");
        }
        
        // 設定箭矢傷害
        Projectile projectileScript = arrow.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.damage = arrowDamage;
        }
        
        // 播放射擊音效
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        // 播放射擊特效
        if (shootEffect != null)
        {
            shootEffect.Play();
        }
        
        Debug.Log($"BossShootEvent: Boss射擊箭矢 - 發射位置: {spawnPosition}, 目標位置: {targetPos}, 方向: {direction}");
    }
} 