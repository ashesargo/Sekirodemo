using UnityEngine;

public class StrongEnemyAI : EnemyAI
{
    [Header("強力小兵設定")]
    public bool useComboAttack = true;
    public string[] comboAnimations = { "Combo1", "Combo2", "Combo3" };
    public float comboAttackRange = 20f; // 強力小兵的攻擊範圍
    public bool hasDoneFirstCombo = false; // 是否已完成首次完整Combo序列
    
    // 覆寫攻擊範圍檢查，讓強力小兵有更遠的攻擊距離
    public override bool IsInAttackRange()
    {
        if (player == null) return false;
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance < comboAttackRange;
        Debug.Log($"StrongEnemyAI.IsInAttackRange - 距離: {distance:F2}, 攻擊範圍: {comboAttackRange}, 是否在範圍內: {inRange}");
        return inRange; // 使用強力小兵的攻擊範圍
    }
    
    // 強力小兵特有的攻擊方法
    public void PerformComboAttack()
    {
        if (useComboAttack)
        {
            SwitchState(new StrongEnemyComboState());
        }
        else
        {
            SwitchState(new AttackState());
        }
    }
    
    // 可以加入其他強力小兵特有的功能
    public void PerformSpecialAttack()
    {
        // 特殊攻擊邏輯
        Debug.Log("StrongEnemyAI: 執行特殊攻擊");
    }
    
    // 覆寫Start方法，加入強力小兵特有的初始化
    protected override void Start()
    {
        // 調用基底類別的Start
        base.Start();
        
        // 強力小兵特有的初始化
        Debug.Log("StrongEnemyAI: 強力小兵初始化完成");
        Debug.Log($"StrongEnemyAI: 攻擊範圍 = {comboAttackRange}, 使用Combo攻擊 = {useComboAttack}");
        Debug.Log($"StrongEnemyAI: 物件名稱 = {gameObject.name}");
    }
} 