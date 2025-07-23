using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 閃爍 UI 圖示
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class BlinkingUIIcon : MonoBehaviour
{
    [Header("Blink Settings")]
    public float blinkSpeed = 1f;  // 閃爍速度
    public float minAlpha = 0.1f;  // 最淡的透明度
    public float maxAlpha = 1f;    // 最亮的透明度

    private CanvasGroup canvasGroup; //引用 CanvasGroup 組件
    private float timer; //計時器

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        timer += Time.deltaTime * blinkSpeed;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(timer, 1f));
        canvasGroup.alpha = alpha;
    }
}
