using UnityEngine;

public class ExecutionSystem : MonoBehaviour
{
    [Header("處決設定")]
    public float executionRange = 2f; // 處決範圍
    public LayerMask enemyLayerMask = 1 << 6; // 敵人層
    
    private TPContraller playerController;
    private Animator playerAnimator;

    AudioSource _audioSource;
    public AudioClip exeAudio;
    void Start()
    {
        playerController = GetComponent<TPContraller>();
        playerAnimator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        // 檢查是否按下滑鼠左鍵
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryExecuteEnemy();
        }
    }
    public void PlayExeSound()
    {
        _audioSource.volume = 1f;
        _audioSource.PlayOneShot(exeAudio);
    }

    private void TryExecuteEnemy()
    {
        // 尋找附近的敵人
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, executionRange, enemyLayerMask);
        
        foreach (var enemyCollider in nearbyEnemies)
        {
            EnemyAI enemyAI = enemyCollider.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                // 檢查敵人是否在失衡狀態
                if (enemyAI.CurrentState is StaggerState)
                {
                    // 檢查是否在玩家前方
                    Vector3 directionToEnemy = (enemyCollider.transform.position - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, directionToEnemy);
                    
                    if (angle <= 60f) // 60度範圍內
                    {
                        ExecuteEnemy(enemyAI);
                        return; // 只處決第一個符合條件的敵人
                    }
                }
            }
        }
    }
    
    private void ExecuteEnemy(EnemyAI enemy)
    {
        Debug.Log($"[ExecutionSystem] 玩家處決敵人: {enemy.name}");
        
        // 播放玩家處決動畫
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Execute");
        }
        
        // 讓敵人進入被處決狀態
        enemy.SwitchState(new ExecutedState());
        
        // 可以添加處決特效
        if (enemy.OnEnemyHitOccurred != null)
        {
            enemy.OnEnemyHitOccurred.Invoke(enemy.transform.position);
        }
    }
    
    // 在Scene視圖中顯示處決範圍（調試用）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, executionRange);
    }
} 