using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class DeathUI : MonoBehaviour
{
    [Header("UI 組件")]
    [SerializeField] private TMP_Text[] deathTexts;    // "死"字文字組件陣列（支援中英文）
    [SerializeField] private Image backgroundImage;    // 背景圖片組件
    
    [Header("動畫設定")]
    [SerializeField] private float animationDuration = 10f;    // 動畫持續時間
    [SerializeField] private float flashDuration = 0.5f;       // 閃爍持續時間
    [SerializeField] private int flashCount = 1;               // 閃爍次數
    
    [Header("顏色設定")]
    [SerializeField] private Color startTextColor = Color.white;    // 文字起始顏色（白色）
    [SerializeField] private Color endTextColor = Color.red;        // 文字結束顏色（紅色）
    [SerializeField] private Color startBgColor = new Color(0, 0, 0, 0);  // 背景起始顏色（透明）
    [SerializeField] private Color endBgColor = new Color(0, 0, 0, 0.8f); // 背景結束顏色（半透明黑）
    
    private Coroutine animationCoroutine;
    private Coroutine flashCoroutine;
    
    void Awake()
    {
        // 確保UI一開始是隱藏的
        gameObject.SetActive(false);
        
        // 初始化顏色
        InitializeTextColors();
        
        if (backgroundImage != null)
        {
            backgroundImage.color = startBgColor;
        }
    }
    
    // 初始化所有文字顏色
    private void InitializeTextColors()
    {
        if (deathTexts != null)
        {
            foreach (TMP_Text text in deathTexts)
            {
                if (text != null)
                {
                    text.color = startTextColor;
                }
            }
        }
    }
    
    // 開始死亡動畫
    public void StartDeathAnimation()
    {
        // 停止之前的動畫
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        
        // 重置顏色
        InitializeTextColors();
        
        if (backgroundImage != null)
        {
            backgroundImage.color = startBgColor;
        }
        
        // 開始新的動畫
        animationCoroutine = StartCoroutine(DeathAnimationCoroutine());
    }
    
    // 死亡動畫協程
    private IEnumerator DeathAnimationCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            // 更新文字顏色（白色到紅色）
            UpdateTextColors(progress);
            
            // 更新背景顏色（透明到半透明黑）
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.Lerp(startBgColor, endBgColor, progress);
            }
            
            yield return null;
        }
        
        // 確保最終顏色正確
        SetTextColors(endTextColor);
        
        if (backgroundImage != null)
        {
            backgroundImage.color = endBgColor;
        }
        
        // 不再自動閃爍
        // flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    public void StartRedFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        // 直接把動畫協程也停掉，確保不會被覆蓋
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        // 開始逐漸變透明的協程
        flashCoroutine = StartCoroutine(FadeToTransparentCoroutine());
    }

    private IEnumerator FadeToTransparentCoroutine()
    {
        // 先設為紅色
        SetTextColors(Color.red);
        
        // 5秒內逐漸變透明
        float fadeTime = 5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeTime;
            
            // 從紅色逐漸變透明
            Color transparentRed = new Color(1f, 0f, 0f, 1f - progress);
            SetTextColors(transparentRed);
            
            yield return null;
        }
        
        // 確保完全透明
        SetTextColors(new Color(1f, 0f, 0f, 0f));
    }
    
    // 停止動畫
    public void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        SetTextColors(Color.red); // 停止時也保持紅色
    }
    
    // 重置UI
    public void ResetUI()
    {
        StopAnimation();
        
        InitializeTextColors();
        
        if (backgroundImage != null)
        {
            backgroundImage.color = startBgColor;
        }
        
        // 確保UI完全隱藏
        gameObject.SetActive(false);
        
        Debug.Log("DeathUI 已重置並隱藏");
    }
    
    // 更新所有文字顏色（用於動畫）
    private void UpdateTextColors(float progress)
    {
        if (deathTexts != null)
        {
            Color currentColor = Color.Lerp(startTextColor, endTextColor, progress);
            foreach (TMP_Text text in deathTexts)
            {
                if (text != null)
                {
                    text.color = currentColor;
                }
            }
        }
    }
    
    // 設定所有文字顏色
    private void SetTextColors(Color color)
    {
        if (deathTexts != null)
        {
            foreach (TMP_Text text in deathTexts)
            {
                if (text != null)
                {
                    text.color = color;
                }
            }
        }
    }
} 