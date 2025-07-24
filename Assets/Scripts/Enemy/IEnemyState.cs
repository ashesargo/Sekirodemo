using UnityEngine;

public abstract class BaseEnemyState
{
    public virtual bool ShouldUseRootMotion() => true; // 預設為 true，追擊/撤退覆寫為 false

    public virtual void EnterState(EnemyAI enemy)
    {
        enemy.SetRootMotion(ShouldUseRootMotion());
    }
    public abstract void UpdateState(EnemyAI enemy);
    public virtual void ExitState(EnemyAI enemy)
    {
        enemy.SetRootMotion(false); // 離開狀態時預設關閉 RootMotion
    }
}