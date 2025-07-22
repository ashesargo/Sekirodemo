using UnityEngine;

public class RetreatState : IEnemyState
{
    private float timer = 0f;
    private Vector3 retreatDir;
    private string retreatAnim = "Retreat";

    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("RetreatState: 進入後退狀態");
        // 隨機選擇撤退動畫
        string[] retreatAnims = { "Retreat", "RetreatR", "RetreatL" };
        int idx = Random.Range(0, retreatAnims.Length);
        retreatAnim = retreatAnims[idx];
        if (enemy.animator.HasState(0, Animator.StringToHash(retreatAnim)))
        {
            enemy.animator.Play(retreatAnim);
        }
        else
        {
            enemy.animator.Play("Idle");
        }
        timer = 0f;
        
        // 計算背向玩家的方向
        retreatDir = (enemy.transform.position - enemy.player.position).normalized;
        retreatDir.y = 0f;
        
        //Debug.Log($"RetreatState: 後退方向 = {retreatDir}");
        enemy.canAutoAttack = false; // 後退時禁用自動攻擊
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        
        // 計算背向玩家的方向，確保方向正確
        Vector3 currentRetreatDir = (enemy.transform.position - enemy.player.position).normalized;
        currentRetreatDir.y = 0f;
        
        // 計算後退力，使用當前計算的方向
        Vector3 retreatForce = currentRetreatDir * enemy.maxSpeed * 0.5f;
        
        // 使用專門的撤退避障方法，檢測後方（撤退專用）
        Vector3 retreatObstacleAvoid = enemy.RetreatObstacleAvoid();
        
        // 調試：檢查避障是否工作（暫時關閉）
        //if (retreatObstacleAvoid.magnitude > 0.1f)
        //{
        //    Debug.Log($"RetreatState: 避障工作正常！避障力: {retreatObstacleAvoid}");
        //}
        //else
        //{
        //    Debug.Log($"RetreatState: 沒有檢測到需要避障的障礙物，避障力: {retreatObstacleAvoid}");
        //}
        
        // 結合後退力和後方避障
        Vector3 totalForce = retreatForce + retreatObstacleAvoid;
        
        // 撤退時面向玩家，保持戰鬥姿態，但確保移動方向正確
        enemy.Move(totalForce, true);
        
        // 詳細的調試信息（暫時關閉）
        //bool shouldShowDebug = enemy.showDebugInfo || true; // 暫時強制顯示調試信息
        //if (shouldShowDebug)
        //{
        //    Debug.Log($"RetreatState: 後退中，時間 = {timer:F2}");
        //    Debug.Log($"  敵人位置: {enemy.transform.position}");
        //    Debug.Log($"  玩家位置: {enemy.player.position}");
        //    Debug.Log($"  後退方向: {currentRetreatDir}");
        //    Debug.Log($"  後退力: {retreatForce}");
        //    Debug.Log($"  後方避障: {retreatObstacleAvoid}");
        //    Debug.Log($"  總力: {totalForce}");
        //    Debug.Log($"  當前速度: {enemy.velocity}");
        //    Debug.Log($"  敵人朝向: {enemy.transform.forward}");
        //    Debug.Log($"  距離玩家: {Vector3.Distance(enemy.transform.position, enemy.player.position):F2}");
        //    
        //    // 檢查力的分量
        //    Debug.Log($"  後退力 X: {retreatForce.x:F3}, Z: {retreatForce.z:F3}");
        //    Debug.Log($"  後方避障力 X: {retreatObstacleAvoid.x:F3}, Z: {retreatObstacleAvoid.z:F3}");
        //    Debug.Log($"  總力 X: {totalForce.x:F3}, Z: {totalForce.z:F3}");
        //    
        //    // 檢查避障力的影響
        //    float retreatMagnitude = retreatForce.magnitude;
        //    float avoidMagnitude = retreatObstacleAvoid.magnitude;
        //    float avoidRatio = avoidMagnitude / retreatMagnitude;
        //    Debug.Log($"  後退力大小: {retreatMagnitude:F3}");
        //    Debug.Log($"  後方避障力大小: {avoidMagnitude:F3}");
        //    Debug.Log($"  避障力比例: {avoidRatio:P1}");
        //    
        //    // 檢查X分量的影響
        //    float retreatX = Mathf.Abs(retreatForce.x);
        //    float avoidX = Mathf.Abs(retreatObstacleAvoid.x);
        //    if (avoidX > 0.01f)
        //    {
        //        Debug.Log($"  警告: 後方避障力X分量較大 ({avoidX:F3})，可能導致右飄");
        //    }
        //    
        //    // 檢查方向一致性
        //    Vector3 retreatBoxDirection = (enemy.transform.position - enemy.player.position).normalized;
        //    retreatBoxDirection.y = 0f;
        //    float directionDot = Vector3.Dot(currentRetreatDir, retreatBoxDirection);
        //    Debug.Log($"  撤退方向與立方體方向一致性: {directionDot:F3} (1.0 = 完全一致)");
        //    
        //    // 檢查是否有異常的X分量
        //    if (Mathf.Abs(totalForce.x) > Mathf.Abs(totalForce.z) * 0.5f)
        //    {
        //        Debug.Log($"  警告: 總力X分量過大 ({totalForce.x:F3})，Z分量 ({totalForce.z:F3})，可能導致側向飄移");
        //    }
        //}
        
        if (timer >= enemy.retreatTime)
        {
            // 檢查是否為Boss
            BossAI bossAI = enemy.GetComponent<BossAI>();
            if (bossAI != null)
            {
                Debug.Log("RetreatState: Boss後退結束，切換到 BossChaseState");
                enemy.canAutoAttack = true; // 後退結束時重新啟用自動攻擊
                enemy.SwitchState(new BossChaseState());
            }
            else
            {
                Debug.Log("RetreatState: 後退結束，切換到 ChaseState");
                enemy.canAutoAttack = true; // 後退結束時重新啟用自動攻擊
                enemy.SwitchState(new ChaseState());
            }
        }
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log("RetreatState: 退出後退狀態");
        enemy.Stop();
    }
    
    // 測試撤退方向的方法
    [ContextMenu("Test Retreat Direction")]
    public void TestRetreatDirection()
    {
        Debug.Log("=== 撤退方向測試 ===");
        
        // 模擬敵人位置和玩家位置
        Vector3 enemyPos = Vector3.zero;
        Vector3 playerPos = new Vector3(5f, 0f, 0f); // 玩家在敵人前方
        
        Vector3 retreatDir = (enemyPos - playerPos).normalized;
        retreatDir.y = 0f;
        
        Debug.Log($"敵人位置: {enemyPos}");
        Debug.Log($"玩家位置: {playerPos}");
        Debug.Log($"撤退方向: {retreatDir}");
        Debug.Log($"撤退方向 X: {retreatDir.x:F3}, Z: {retreatDir.z:F3}");
        
        // 測試不同位置的撤退方向
        Vector3[] testPositions = {
            new Vector3(0f, 0f, 0f),    // 原點
            new Vector3(5f, 0f, 5f),    // 右前方
            new Vector3(-5f, 0f, 5f),   // 左前方
            new Vector3(0f, 0f, 10f),   // 正前方
        };
        
        foreach (var pos in testPositions)
        {
            Vector3 testRetreatDir = (pos - playerPos).normalized;
            testRetreatDir.y = 0f;
            Debug.Log($"位置 {pos} 的撤退方向: {testRetreatDir}");
        }
    }
} 