using UnityEngine;
using System.Collections;

/// <summary>
/// 讓 UI ICON 跟隨 lockTarget 並顯示在螢幕上方
/// </summary>
public class LockOnIconFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 0);

    private Camera mainCamera;
    private RectTransform rectTransform;
    private UnityEngine.UI.Image iconImage;
    private Vector3 originalScale;
    private Color originalColor;
    private Coroutine effectCoroutine;

    void Start()
    {
        // 取得主攝影機與 RectTransform
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        iconImage = GetComponent<UnityEngine.UI.Image>();
        
        // 儲存原始大小和顏色
        originalScale = rectTransform.localScale;
        if (iconImage != null)
        {
            originalColor = iconImage.color;
        }
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

    /// <summary>
    /// 觸發架勢滿效果：變紅色並變大2倍持續3秒
    /// </summary>
    public void TriggerPostureFullEffect()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }
        effectCoroutine = StartCoroutine(PostureFullEffectCoroutine());
    }

    private IEnumerator PostureFullEffectCoroutine()
    {
        if (iconImage == null) yield break;

        // 變紅色並變大5倍
        iconImage.color = Color.red;
        rectTransform.localScale = originalScale * 5f;

        // 等待3秒
        yield return new WaitForSeconds(3f);

        // 恢復原始顏色和大小
        iconImage.color = originalColor;
        rectTransform.localScale = originalScale;

        effectCoroutine = null;
    }
} 