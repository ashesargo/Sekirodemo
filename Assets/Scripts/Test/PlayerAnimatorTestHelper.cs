using UnityEngine;

public class PlayerAnimatorTestHelper : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode testExecuteKey = KeyCode.E; // 測試處決動畫
    public KeyCode resetKey = KeyCode.R; // 重置動畫
    
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("[PlayerAnimatorTestHelper] 此物件沒有Animator組件！");
            return;
        }
        
        Debug.Log("[PlayerAnimatorTestHelper] 玩家Animator測試助手已初始化");
        Debug.Log($"[PlayerAnimatorTestHelper] 按 {testExecuteKey} 測試處決動畫");
        Debug.Log($"[PlayerAnimatorTestHelper] 按 {resetKey} 重置動畫");
        
        // 檢查必要的參數是否存在
        CheckAnimatorParameters();
    }
    
    void Update()
    {
        if (animator == null) return;
        
        // 測試處決動畫
        if (Input.GetKeyDown(testExecuteKey))
        {
            TestExecuteAnimation();
        }
        
        // 重置動畫
        if (Input.GetKeyDown(resetKey))
        {
            ResetAnimations();
        }
    }
    
    private void CheckAnimatorParameters()
    {
        bool hasExecute = false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == "Execute" && param.type == AnimatorControllerParameterType.Trigger)
            {
                hasExecute = true;
                Debug.Log("[PlayerAnimatorTestHelper] ✓ 找到Execute參數");
            }
        }
        
        if (!hasExecute)
        {
            Debug.LogWarning("[PlayerAnimatorTestHelper] ⚠ 缺少Execute參數 (Trigger類型)");
        }
        else
        {
            Debug.Log("[PlayerAnimatorTestHelper] ✓ 所有必要參數都已設置");
        }
        
        // 檢查現有參數
        Debug.Log("[PlayerAnimatorTestHelper] 現有參數列表：");
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            Debug.Log($"[PlayerAnimatorTestHelper] - {param.name} ({param.type})");
        }
    }
    
    private void TestExecuteAnimation()
    {
        Debug.Log("[PlayerAnimatorTestHelper] 觸發處決動畫");
        animator.SetTrigger("Execute");
    }
    
    private void ResetAnimations()
    {
        Debug.Log("[PlayerAnimatorTestHelper] 重置所有動畫參數");
        animator.ResetTrigger("Execute");
        
        // 重置其他可能的Trigger參數
        animator.ResetTrigger("Parry1");
        animator.ResetTrigger("Parry2");
        animator.ResetTrigger("Dash");
    }
    
    // 在Scene視圖中顯示測試信息
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
        
        // 顯示測試按鍵信息
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, 
            $"測試處決: {testExecuteKey}\n重置: {resetKey}");
        #endif
    }
} 