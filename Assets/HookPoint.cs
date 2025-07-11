using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    public bool isGrapplable = true; // 是否可被勾取
    public Renderer pointRenderer; // 用於高亮顯示

    public void Highlight(bool highlight)
    {
        if (pointRenderer != null)
        {
            // 簡單的高亮效果，例如改變材質顏色
            pointRenderer.material.color = highlight ? Color.green : Color.white;
        }
    }
}