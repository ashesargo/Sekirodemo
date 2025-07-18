using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// 主選單控制器
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsMenu;
    public GameObject pressAnyButton;    // 初始按鈕面板
    public GameObject mainMenu;          // 主要選單面板
    public GameObject backPage;          // 返回頁面面板

    private bool isMainMenuActive = false;

    void Awake()
    {
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        Screen.fullScreen = true;
    }

    void Start()
    {
        // 初始化只顯示 Press Any Button 面板
        if (pressAnyButton != null && mainMenu != null)
        {
            pressAnyButton.SetActive(true);
            mainMenu.SetActive(false);
            settingsMenu.SetActive(false);
        }
        else
        {
            Debug.LogError("UI 幹你娘！");
        }
    }

    void Update()
    {
        // 從設定頁回主選單
        if (settingsMenu.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ShowMainMenu();
        }

        // 如果主選單已啟動，不需要檢測輸入
        if (isMainMenuActive)
        {
            return;
        }

        // 檢測任意輸入
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame ||
            Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
        {
            ShowMainMenu();
        }
    }

    // 顯示主選單
    private void ShowMainMenu()
    {
        if (pressAnyButton == null || mainMenu == null)
        {
            Debug.LogError("UI 幹你娘！");
            return;
        }

        pressAnyButton.SetActive(false);
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        isMainMenuActive = true;
    }

    // 顯示設定頁
    private void ShowSettings()
    {
        settingsMenu.SetActive(true);
        mainMenu.SetActive(false);
        pressAnyButton.SetActive(false);
    }

    // 繼續遊戲
    public void OnContinue()
    {
        SceneManager.LoadScene("Demo Scene");
    }

    // 開始新遊戲
    public void OnNewGame()
    {
        SceneManager.LoadScene("Demo Scene");
    }

    // 開啟設定面板
    public void OnSettings()
    {
        settingsMenu.SetActive(true);
    }

    // 離開遊戲 (打包後才會執行)
    public void OnQuit()
    {
        Application.Quit();
    }

    // 回到上一頁（目前無使用）
    public void OnBackPage()
    {
        
    }
}
