using UnityEngine;

/// <summary>
/// 讓 UI ICON 跟隨 lockTarget 並顯示在螢幕上方
/// </summary>
public class LockOnIconFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 0);

    private Camera mainCamera;
    private RectTransform rectTransform;

    void Start()
    {
        // 取得主攝影機與 RectTransform
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // 若目標不存在則自動銷毀自己
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        // 世界座標轉螢幕座標
        Vector3 worldPos = target.position + offset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        // 設定 UI 位置（Canvas 為 Screen Space - Overlay 時可直接設 position）
        rectTransform.position = screenPos;
    }
} 