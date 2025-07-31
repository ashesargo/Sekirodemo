using UnityEngine;

public class StaggerState : BaseEnemyState
{
    private float staggerDuration = 3f; // 失衡持續時間
    private float staggerTimer = 0f;
    private bool staggerAnimationStarted = false;

    public override void EnterState(EnemyAI enemy)
    {
        base.EnterState(enemy);
        
        // 重置計時器
        staggerTimer = 0f;
        staggerAnimationStarted = false;
        
        // 停止移動
        enemy.Stop();
        
        // 禁用防禦和攻擊能力
        enemy.canBeParried = false;
        enemy.canAutoAttack = false;
        
        // 禁用架勢增加能力
        HealthPostureController healthController = enemy.GetComponent<HealthPostureController>();
        if (healthController != null)
        {
            healthController.canIncreasePosture = false;
        }
        
        // 設置失衡標記，防止被攻擊
        EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.isStaggered = true;
        }
        
        Debug.Log($"[StaggerState] 敵人 {enemy.name} 進入失衡狀態");
    }

    public override void UpdateState(EnemyAI enemy)
    {
        // 播放失衡動畫（只播放一次）
        if (!staggerAnimationStarted)
        {
            enemy.animator.SetTrigger("Stagger");
            staggerAnimationStarted = true;
            Debug.Log($"[StaggerState] 敵人 {enemy.name} 播放失衡動畫");
        }
        
        // 計時器
        staggerTimer += Time.deltaTime;
        
        // 失衡時間結束後，恢復正常狀態
        if (staggerTimer >= staggerDuration)
        {
            // 恢復防禦和攻擊能力
            enemy.canBeParried = true;
            enemy.canAutoAttack = true;
            
            // 恢復架勢增加能力
            HealthPostureController healthController = enemy.GetComponent<HealthPostureController>();
            if (healthController != null)
            {
                healthController.canIncreasePosture = true;
                // 重置架勢值
                healthController.ResetPosture();
            }
            
            // 清除失衡標記
            EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
            if (enemyTest != null)
            {
                enemyTest.isStaggered = false;
            }
            
            // 切換到追擊狀態（如果能看到玩家）或閒置狀態
            if (enemy.CanSeePlayer())
            {
                enemy.SwitchState(new ChaseState());
                Debug.Log($"[StaggerState] 敵人 {enemy.name} 失衡狀態結束，開始追擊玩家");
            }
            else
            {
                enemy.SwitchState(new IdleState());
                Debug.Log($"[StaggerState] 敵人 {enemy.name} 失衡狀態結束，恢復閒置狀態");
            }
        }
    }

    public override void ExitState(EnemyAI enemy)
    {
        base.ExitState(enemy);
        
        // 確保動畫參數被重置
        enemy.animator.ResetTrigger("Stagger");
        
        // 確保失衡標記被清除
        EnemyTest enemyTest = enemy.GetComponent<EnemyTest>();
        if (enemyTest != null)
        {
            enemyTest.isStaggered = false;
        }
        
        Debug.Log($"[StaggerState] 敵人 {enemy.name} 退出失衡狀態");
    }

    public override bool ShouldUseRootMotion() => true;
} 