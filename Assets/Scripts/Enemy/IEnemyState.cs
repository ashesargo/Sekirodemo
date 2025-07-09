public interface IEnemyState
{
    void EnterState(EnemyAI enemy);
    void UpdateState(EnemyAI enemy);
    void ExitState(EnemyAI enemy);
}