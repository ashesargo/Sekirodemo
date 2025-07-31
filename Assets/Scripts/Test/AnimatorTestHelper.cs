using UnityEngine;

public class AnimatorTestHelper : MonoBehaviour
{
    [Header("測試設定")]
    public KeyCode testStaggerKey = KeyCode.Alpha1; // 測試失衡動畫
    public KeyCode testExecutedKey = KeyCode.Alpha2; // 測試被處決動畫
    public KeyCode resetKey = KeyCode.R; // 重置動畫
    
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("[AnimatorTestHelper] 此物件沒有Animator組件！");
            return;
        }
        
        Debug.Log("[AnimatorTestHelper] Animator測試助手已初始化");
        Debug.Log($"[AnimatorTestHelper] 按 {testStaggerKey} 測試失衡動畫");
        Debug.Log($"[AnimatorTestHelper] 按 {testExecutedKey} 測試被處決動畫");
        Debug.Log($"[AnimatorTestHelper] 按 {resetKey} 重置動畫");
        
        // 檢查必要的參數是否存在
        CheckAnimatorParameters();
    }
    
    void Update()
    {
        if (animator == null) return;
        
        // 測試失衡動畫
        if (Input.GetKeyDown(testStaggerKey))
        {
            TestStaggerAnimation();
        }
        
        // 測試被處決動畫
        if (Input.GetKeyDown(testExecutedKey))
        {
            TestExecutedAnimation();
        }
        
        // 重置動畫
        if (Input.GetKeyDown(resetKey))
        {
            ResetAnimations();
        }
    }
    
    private void CheckAnimatorParameters()
    {
        bool hasStagger = false;
        bool hasExecuted = false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == "Stagger" && param.type == AnimatorControllerParameterType.Trigger)
            {
                hasStagger = true;
                Debug.Log("[AnimatorTestHelper] ✓ 找到Stagger參數");
            }
            else if (param.name == "Executed" && param.type == AnimatorControllerParameterType.Trigger)
            {
                hasExecuted = true;
                Debug.Log("[AnimatorTestHelper] ✓ 找到Executed參數");
            }
        }
        
        if (!hasStagger)
        {
            Debug.LogWarning("[AnimatorTestHelper] ⚠ 缺少Stagger參數 (Trigger類型)");
        }
        
        if (!hasExecuted)
        {
            Debug.LogWarning("[AnimatorTestHelper] ⚠ 缺少Executed參數 (Trigger類型)");
        }
        
        if (hasStagger && hasExecuted)
        {
            Debug.Log("[AnimatorTestHelper] ✓ 所有必要參數都已設置");
        }
    }
    
    private void TestStaggerAnimation()
    {
        Debug.Log("[AnimatorTestHelper] 觸發失衡動畫");
        animator.SetTrigger("Stagger");
    }
    
    private void TestExecutedAnimation()
    {
        Debug.Log("[AnimatorTestHelper] 觸發被處決動畫");
        animator.SetTrigger("Executed");
    }
    
    private void ResetAnimations()
    {
        Debug.Log("[AnimatorTestHelper] 重置所有動畫參數");
        animator.ResetTrigger("Stagger");
        animator.ResetTrigger("Executed");
    }
    
    // 在Scene視圖中顯示測試信息
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
        
        // 顯示測試按鍵信息
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, 
            $"測試失衡: {testStaggerKey}\n測試處決: {testExecutedKey}\n重置: {resetKey}");
        #endif
    }
} 