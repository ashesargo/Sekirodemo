using UnityEngine;

public class AlertState : IEnemyState
{
    private float alertTime = 1.5f;
    private float timer = 0f;

    public void EnterState(EnemyAI enemy)
    {
        enemy.animator.Play("Alert");
        enemy.Stop();
        timer = 0f;
        enemy.canAutoAttack = true; // 啟用自動攻擊
    }

    public void UpdateState(EnemyAI enemy)
    {
        timer += Time.deltaTime;
        if (timer > alertTime)
        {
            if (enemy.IsInAttackRange())
            {
                // 檢查是否為遠程兵種
                var rangedEnemy = enemy.GetComponent<RangedEnemy>();
                if (rangedEnemy != null)
                {
                    enemy.SwitchState(new AimState());
                }
                else
                {
                    enemy.SwitchState(new AttackState());
                }
            }
            else
                enemy.SwitchState(new ChaseState());
        }
    }

    public void ExitState(EnemyAI enemy) { }
}