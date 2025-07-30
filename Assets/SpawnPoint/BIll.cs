using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;
    private Vector3 initialScale;
    private bool isInitialized = false;

    void Start()
    {
        // 獲取主攝影機
        mainCam = Camera.main;

        // 檢查是否成功獲取攝影機
        if (mainCam == null)
        {
            Debug.LogWarning("BillboardUI: 找不到主攝影機！");
            return;
        }

        // 儲存初始縮放
        initialScale = transform.localScale;
        isInitialized = true;
    }

    void LateUpdate()
    {
        // 檢查是否已初始化且攝影機存在
        if (!isInitialized || mainCam == null)
        {
            Debug.LogWarning("BillboardUI: 未初始化或攝影機不存在！");
            return;
        }

        // 讓 UI 面向攝影機，但只旋轉 Y 軸以避免文字翻轉
        Vector3 targetPosition = transform.position + mainCam.transform.forward;
        transform.LookAt(targetPosition, mainCam.transform.up);

        // 修正翻轉問題（將 X 和 Z 軸旋轉設為 0）
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

        // 保持初始縮放（防止距離影響大小）
        transform.localScale = initialScale;
    }
}