using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    public bool isGrapplable = true;

    void Start()
    {
        // �ˬd Layer
        if (gameObject.layer == 0)
        {
            Debug.LogError($"GrapplePoint {name} �� Layer ���]�m�A���]�� 'Grapple'");
        }

        // �ˬd Collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError($"GrapplePoint {name} �ʤ� Collider �ե�");
        }

        Debug.Log($"GrapplePoint {name} ��l�Ƨ���: isGrapplable={isGrapplable}, Layer={LayerMask.LayerToName(gameObject.layer)}");
    }
}