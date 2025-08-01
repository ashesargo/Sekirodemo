using UnityEngine;
using TMPro;
using System.Collections;

public class DangerFX : MonoBehaviour
{
    [SerializeField] private TextMeshPro dangerText;
    [SerializeField] private float fadeInDuration = 0.2f;    // 淡入時間
    [SerializeField] private float highlightDuration = 0.1f; // 高亮時間
    [SerializeField] private float fadeOutDuration = 0.3f;   // 淡出時間
    [SerializeField] private Color highlightColor = Color.red; // 高亮顏色

    AudioSource audioSource;
    public AudioClip dangerSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (dangerText == null)
        {
            dangerText = GetComponent<TextMeshPro>();
        }
        // 初始設為不可見
        dangerText.color = new Color(dangerText.color.r, dangerText.color.g, dangerText.color.b, 0f);
    }

    // 公開函數，用於觸發「危」特效
    public void TriggerDangerEffect()
    {
        StopAllCoroutines(); // 停止正在進行的協程
        audioSource.PlayOneShot(dangerSound);
        StartCoroutine(DangerEffectCoroutine());
    }

    private IEnumerator DangerEffectCoroutine()
    {
        // 淡入
        float elapsed = 0f;
        Color startColor = dangerText.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            dangerText.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        dangerText.color = targetColor;

        // 高亮
        dangerText.color = highlightColor;
        yield return new WaitForSeconds(highlightDuration);

        // 淡出
        elapsed = 0f;
        startColor = dangerText.color;
        targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            dangerText.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        dangerText.color = targetColor;
    }
}