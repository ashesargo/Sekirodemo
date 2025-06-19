using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject pressAnyButtonPanel;    // 初始按鈕面板
    public GameObject mainMenuPanel;          // 主要選單面板

    private bool isMainMenuActive = false;

    void Start()
    {
        Debug.Log("MainMenuController Start");
        // 初始化只顯示 Press Any Button 面板
        if (pressAnyButtonPanel != null && mainMenuPanel != null)
        {
            pressAnyButtonPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
            Debug.Log("UI Panels initialized");
        }
        else
        {
            Debug.LogError("UI面板幹你娘！");
        }
    }

    void Update()
    {
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
        if (pressAnyButtonPanel == null || mainMenuPanel == null)
        {
            Debug.LogError("UI panels are null!");
            return;
        }

        pressAnyButtonPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        isMainMenuActive = true;
        Debug.Log("Main menu shown");
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
        settingsPanel.SetActive(true);
    }

    // 離開遊戲 (打包後才會執行)
    public void OnQuit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
