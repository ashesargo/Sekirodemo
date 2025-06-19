using UnityEngine;
using UnityEngine.UI;

// 控制選擇圖示根據滑鼠懸停在按鈕上時移動
public class UIHoverSelectionManager : MonoBehaviour
{
    public static UIHoverSelectionManager Instance;

    [Header("光標圖示")]
    public RectTransform selectIcon;

    private void Awake()
    {
        Instance = this;
        if (selectIcon != null)
            selectIcon.gameObject.SetActive(false);
    }

    // 顯示光標
    public void ShowIcon(RectTransform target)
    {
        if (selectIcon == null || target == null) return;

        selectIcon.gameObject.SetActive(true);
        selectIcon.position = target.position;
    }

    // 隱藏光標
    public void HideIcon()
    {
        if (selectIcon != null)
            selectIcon.gameObject.SetActive(false);
    }
}
