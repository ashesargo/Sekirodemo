using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// 主選單控制器
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pressAnyButton;    // 初始按鈕面板
    public GameObject mainMenu;          // 主要選單面板

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
        }
        else
        {
            Debug.LogError("UI 幹你娘！");
        }
    }

    void Update()
    {
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

    // 離開遊戲 (打包後才會執行)
    public void OnQuit()
    {
        Application.Quit();
    }
}
