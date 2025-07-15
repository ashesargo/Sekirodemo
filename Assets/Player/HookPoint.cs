using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    public bool isGrapplable = true;

    void Start()
    {
        // 檢查 Layer
        if (gameObject.layer == 0)
        {
            Debug.LogError($"GrapplePoint {name} 的 Layer 未設置，應設為 'Grapple'");
        }

        // 檢查 Collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError($"GrapplePoint {name} 缺少 Collider 組件");
        }

        Debug.Log($"GrapplePoint {name} 初始化完成: isGrapplable={isGrapplable}, Layer={LayerMask.LayerToName(gameObject.layer)}");
    }
}