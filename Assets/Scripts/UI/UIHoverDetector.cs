using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UIHoverSelectionManager.Instance != null)
        {
            RectTransform rt = GetComponent<RectTransform>();
            UIHoverSelectionManager.Instance.ShowIcon(rt);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIHoverSelectionManager.Instance != null)
        {
            UIHoverSelectionManager.Instance.HideIcon();
        }
    }
}
