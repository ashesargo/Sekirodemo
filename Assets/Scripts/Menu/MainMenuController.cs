using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// 主選單控制器
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pressAnyButton;    // 初始按鈕面板
    public GameObject mainMenu;          // 主要選單面板

    [Header("淡出效果")]
    public GameObject fadePanel;         // 淡出面板 (黑色UI Image)
    public AudioSource backgroundMusic;  // 背景音樂

    private bool isTransitioning = false;

    void Awake()
    {
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        Screen.fullScreen = true;
    }

    void Start()
    {
        // // 初始化只顯示 Press Any Button 面板
        // if (pressAnyButton != null && mainMenu != null)
        // {
        //     pressAnyButton.SetActive(true);
        //     mainMenu.SetActive(false);
        // }
        // else
        // {
        //     Debug.LogError("UI 幹你娘！");
        // }

        // // 初始化淡出面板
        // if (fadePanel != null)
        // {
        //     fadePanel.SetActive(false);
        // }
    }

    void Update()
    {
        // // 檢測任意輸入
        // if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame ||
        //     Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
        // {
        //     ShowMainMenu();
        // }
    }

    // 顯示主選單
    // private void ShowMainMenu()
    // {
    //     if (pressAnyButton == null || mainMenu == null)
    //     {
    //         Debug.LogError("UI 幹你娘！");
    //         return;
    //     }

    //     pressAnyButton.SetActive(false);
    //     mainMenu.SetActive(true);
    // }

    // 開始新遊戲
    public void OnNewGame()
    {
        StartCoroutine(TransitionToScene("Demo Scene"));
    }

    // 離開遊戲 (打包後才會執行)
    public void OnQuit()
    {
        Application.Quit();
    }

    // 場景切換協程
    private IEnumerator TransitionToScene(string sceneName)
    {
        isTransitioning = true;

        // 等待5秒
        yield return new WaitForSeconds(0.1f);

        // 開始淡出效果
        if (fadePanel != null)
        {
            fadePanel.SetActive(true);
            CanvasGroup fadeGroup = fadePanel.GetComponent<CanvasGroup>();
            if (fadeGroup == null)
            {
                fadeGroup = fadePanel.AddComponent<CanvasGroup>();
            }

            // 淡出畫面 (2秒)
            float fadeTime = 2f;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = elapsedTime / fadeTime;
                fadeGroup.alpha = alpha;
                
                // 同時淡出背景音樂
                if (backgroundMusic != null && backgroundMusic.isPlaying)
                {
                    backgroundMusic.volume = 1f - alpha;
                }
                
                yield return null;
            }

            // 確保完全淡出
            fadeGroup.alpha = 1f;
            if (backgroundMusic != null)
            {
                backgroundMusic.volume = 0f;
            }
        }

        // 切換場景
        SceneManager.LoadScene(sceneName);
    }
}
