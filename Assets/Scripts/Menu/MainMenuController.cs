using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsMenu;
    public GameObject pressAnyButton;    // 初始按鈕面板
    public GameObject mainMenu;          // 主要選單面板
    public GameObject backPage;          // 返回頁面面板

    private bool isMainMenuActive = false;

    void Start()
    {
        Debug.Log("MainMenuController Start");
        // 初始化只顯示 Press Any Button 面板
        if (pressAnyButton != null && mainMenu != null)
        {
            pressAnyButton.SetActive(true);
            mainMenu.SetActive(false);
            settingsMenu.SetActive(false);
            Debug.Log("UI Panels initialized");
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
            Debug.Log("Escape pressed, going back to main menu");
            ShowMainMenu();
        }

        // 如果主選單已啟動，不需要檢測輸入
        if (isMainMenuActive)
        {
            Debug.Log("Main menu is already active");
            return;
        }

        // 檢測任意輸入
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame ||
            Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
        {
            Debug.Log("Input detected");
            ShowMainMenu();
        }
    }

    // 顯示主選單
    private void ShowMainMenu()
    {
        Debug.Log("ShowMainMenu called");
        if (pressAnyButton == null || mainMenu == null)
        {
            Debug.LogError("UI panels are null!");
            return;
        }

        pressAnyButton.SetActive(false);
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        isMainMenuActive = true;
        Debug.Log("Main menu shown");
    }

    // 顯示設定頁
    private void ShowSettings()
    {
        settingsMenu.SetActive(true);
        mainMenu.SetActive(false);
        pressAnyButton.SetActive(false);
        Debug.Log("Settings menu shown");
    }

    // 繼續遊戲
    public void OnContinue()
    {
        Debug.Log("Continue Pressed");
        SceneManager.LoadScene("Demo Scene");
    }

    // 開始新遊戲
    public void OnNewGame()
    {
        Debug.Log("NewGame Pressed");
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
        Debug.Log("Quit");
        Application.Quit();
    }

    // 回到上一頁（無使用）
    public void OnBackPage()
    {
        Debug.Log("BackPage");
    }
}
