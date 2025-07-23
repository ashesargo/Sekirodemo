using UnityEngine;

// BillboardUI 永遠面向鏡頭，不會轉背，並且不會隨著距離調整大小
public class BillboardUI : MonoBehaviour
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

        // 讓 UI 面向攝影機
        transform.forward = -mainCam.transform.forward;
        
        // 保持初始縮放（防止距離影響大小）
        transform.localScale = initialScale;
    }
}