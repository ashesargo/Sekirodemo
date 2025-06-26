using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class BlinkingUIIcon : MonoBehaviour
{
    [Header("閃爍設定")]
    public float blinkSpeed = 1f;  // 閃爍速度（越大越快）
    public float minAlpha = 0.1f;  // 最淡的透明度
    public float maxAlpha = 1f;    // 最亮的透明度

    private CanvasGroup canvasGroup;
    private float timer;

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
