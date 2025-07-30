using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;
    private Vector3 initialScale;
    private bool isInitialized = false;

    void Start()
    {
        // ����D��v��
        mainCam = Camera.main;

        // �ˬd�O�_���\�����v��
        if (mainCam == null)
        {
            Debug.LogWarning("BillboardUI: �䤣��D��v���I");
            return;
        }

        // �x�s��l�Y��
        initialScale = transform.localScale;
        isInitialized = true;
    }

    void LateUpdate()
    {
        // �ˬd�O�_�w��l�ƥB��v���s�b
        if (!isInitialized || mainCam == null)
        {
            Debug.LogWarning("BillboardUI: ����l�Ʃ���v�����s�b�I");
            return;
        }

        // �� UI ���V��v���A���u���� Y �b�H�קK��r½��
        Vector3 targetPosition = transform.position + mainCam.transform.forward;
        transform.LookAt(targetPosition, mainCam.transform.up);

        // �ץ�½����D�]�N X �M Z �b����]�� 0�^
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

        // �O����l�Y��]����Z���v�T�j�p�^
        transform.localScale = initialScale;
    }
}