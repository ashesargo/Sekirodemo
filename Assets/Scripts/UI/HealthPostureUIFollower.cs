using UnityEngine;

public class HealthPostureUIFollower : MonoBehaviour
{
    public Transform target;  // �ĤH�Y��
    public Vector3 offset = new Vector3(0, 2f, 0);  // ������m
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
