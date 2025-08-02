using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for Image
using TMPro; // Required for TextMeshPro

public class ShinobiEX : MonoBehaviour
{
    public GameObject bossStatus;
    public EnemyTest boss;
    public GameObject targetObject;
    public Image targetImage;
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI targetText2;
    private bool isFading = false;
    private bool hasFaded = false; // New flag to ensure fade-in only happens once

    AudioSource _audioSource;
    public AudioClip _audioClip;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        bossStatus = GameObject.Find("Boss(Clone)");
        if (bossStatus != null)
        {
            boss = bossStatus.GetComponent<EnemyTest>();
        }
        else
        {
            Debug.LogWarning("ShinobiEX: 找不到名為 'Boss(Clone)' 的遊戲對象");
        }

        if (targetObject != null)
        {
            // Initialize as inactive and fully transparent
            targetObject.SetActive(false);
            if (targetImage != null)
                targetImage.color = new Color(targetImage.color.r, targetImage.color.g, targetImage.color.b, 0f);
            else
                Debug.LogWarning("ShinobiEX: targetImage 未設置");

            if (targetText != null)
                targetText.color = new Color(targetText.color.r, targetText.color.g, targetText.color.b, 0f);
            else
                Debug.LogWarning("ShinobiEX: targetText 未設置");

            if (targetText2 != null)
                targetText2.color = new Color(targetText2.color.r, targetText2.color.g, targetText2.color.b, 0f);
            else
                Debug.LogWarning("ShinobiEX: targetText2 未設置");
        }
        else
        {
            Debug.LogWarning("ShinobiEX: targetObject 未設置");
        }

        if (boss == null)
        {
            Debug.LogWarning("ShinobiEX: 無法獲取 EnemyTest 組件");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bossStatus == null)
        {
            bossStatus = GameObject.Find("Boss(Clone)");
            if (bossStatus != null)
            {
                boss = bossStatus.GetComponent<EnemyTest>();
            }
        }

        if (boss != null && IsBossTrulyDead() && targetObject != null && !isFading && !hasFaded)
        {
            StartCoroutine(FadeInTargetObject());
        }
    }

    // 檢查Boss是否真正死亡（血量<=0且復活次數<=0）
    private bool IsBossTrulyDead()
    {
        if (boss == null) return false;
        
        HealthPostureController healthController = boss.GetComponent<HealthPostureController>();
        if (healthController == null) return false;
        
        // 檢查血量 <= 0 且復活次數 <= 0
        bool isHealthZero = healthController.GetHealthPercentage() <= 0f;
        bool isNoLivesLeft = healthController.live <= 0;
        
        Debug.Log($"[ShinobiEX] Boss血量: {healthController.GetHealthPercentage() * 100:F1}%, 復活次數: {healthController.live}, 真正死亡: {isHealthZero && isNoLivesLeft}");
        
        return isHealthZero && isNoLivesLeft;
    }

    private IEnumerator FadeInTargetObject()
    {
        isFading = true;
        yield return new WaitForSeconds(1f); // 1-second delay
        _audioSource.PlayOneShot(_audioClip);
        if (targetObject != null)
        {
            targetObject.SetActive(true);

            float duration = 1f; // Duration of fade-in effect
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);

                if (targetImage != null)
                    targetImage.color = new Color(targetImage.color.r, targetImage.color.g, targetImage.color.b, alpha);
                if (targetText != null)
                    targetText.color = new Color(targetText.color.r, targetText.color.g, targetText.color.b, alpha);
                if (targetText2 != null)
                    targetText2.color = new Color(targetText2.color.r, targetText2.color.g, targetText2.color.b, alpha);

                yield return null;
            }

            // Ensure final alpha is exactly 1
            if (targetImage != null)
                targetImage.color = new Color(targetImage.color.r, targetImage.color.g, targetImage.color.b, 1f);
            if (targetText != null)
                targetText.color = new Color(targetText.color.r, targetText.color.g, targetText.color.b, 1f);
            if (targetText2 != null)
                targetText2.color = new Color(targetText2.color.r, targetText2.color.g, targetText2.color.b, 1f);
        }
        else
        {
            Debug.LogWarning("ShinobiEX: targetObject 在淡入時為 null");
        }

        isFading = false;
        hasFaded = true; // Mark fade-in as completed
    }
}