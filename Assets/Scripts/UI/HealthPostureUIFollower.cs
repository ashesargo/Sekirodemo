using UnityEngine;

public class HealthPostureUIFollower : MonoBehaviour
{
    public Transform target;  // 敵人頭部
    public Vector3 offset = new Vector3(0, 2f, 0);  // 偏移位置
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null || mainCamera == null) return;

        Vector3 worldPos = target.position + offset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        transform.position = screenPos;
    }
}
