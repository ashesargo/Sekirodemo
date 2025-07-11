using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    public bool isGrapplable = true; // �O�_�i�Q�Ĩ�
    public Renderer pointRenderer; // �Ω󰪫G���

    public void Highlight(bool highlight)
    {
        if (pointRenderer != null)
        {
            // ²�檺���G�ĪG�A�Ҧp���ܧ����C��
            pointRenderer.material.color = highlight ? Color.green : Color.white;
        }
    }
}