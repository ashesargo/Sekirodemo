using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public float grappleRange = 10f; // �����˴��d��
    public float maxGrappleDistance = 15f; // �̤j����Z��
    public float grappleSpeed = 10f; // ���겾�ʳt��
    public LayerMask grappleLayer; // �����I���h
    public Camera mainCamera; // �D��v��
    public Animator animator; // ���a�ʵe���
    public CharacterController controller; // ���ⱱ�
    public LineRenderer grappleRope; // ÷����LineRenderer

    private List<HookPoint> nearbyPoints = new List<HookPoint>();
    private bool isGrappling = false;

    void Start()
    {
        // ��l��÷���ĪG
        if (grappleRope != null)
        {
            grappleRope.enabled = false;
        }
    }

    void Update()
    {
        // �˴��d�򤺪������I
        UpdateNearbyPoints();

        // ���UE��i�����
        if (Input.GetKeyDown(KeyCode.E) && !isGrappling)
        {
            TryGrapple();
        }
    }

    void UpdateNearbyPoints()
    {
        // �M�����e�����G
        foreach (var point in nearbyPoints)
        {
            point.Highlight(false);
        }
        nearbyPoints.Clear();

        // �˴��d�򤺪������I
        Collider[] colliders = Physics.OverlapSphere(transform.position, grappleRange, grappleLayer);
        foreach (var collider in colliders)
        {
            HookPoint point = collider.GetComponent<HookPoint>();
            if (point != null && point.isGrapplable)
            {
                point.Highlight(true);
                nearbyPoints.Add(point);
            }
        }
    }

    void TryGrapple()
    {
        // �q��v���o�g�g�u
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGrappleDistance, grappleLayer))
        {
            HookPoint targetPoint = hit.collider.GetComponent<HookPoint>();
            if (targetPoint != null && targetPoint.isGrapplable)
            {
                StartGrapple(targetPoint);
            }
        }
    }

    void StartGrapple(HookPoint target)
    {
        isGrappling = true;

        // �������ʵe
        if (animator != null)
        {
            animator.SetTrigger("Grapple");
        }

        // �T��CharacterController���q�{�欰�]�i��A�ھڧA���ݨD�^
        if (controller != null)
        {
            controller.enabled = false; // �{�ɸT�ΥH���������
        }

        // �Ұ�÷���ĪG
        if (grappleRope != null)
        {
            grappleRope.enabled = true;
            StartCoroutine(UpdateRope(target.transform.position));
        }

        // �}�l���ʨ�����I
        StartCoroutine(MoveToHookPoint(target.transform.position));
    }

    IEnumerator MoveToHookPoint(Vector3 targetPos)
    {
        // ���Ʋ��ʨ�ؼ��I
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            Vector3 move = direction * grappleSpeed * Time.deltaTime;

            // �ϥ�CharacterController.Move�i�沾��
            if (controller != null)
            {
                transform.position += move; // �����ק��m�]�]��controller�w�T�Ρ^
            }

            yield return null;
        }

        // ��_CharacterController
        if (controller != null)
        {
            controller.enabled = true;
        }

        isGrappling = false;
    }

    IEnumerator UpdateRope(Vector3 targetPos)
    {
        // ��s÷�����_�I�M���I
        while (isGrappling)
        {
            grappleRope.SetPosition(0, transform.position); // ÷���_�I
            grappleRope.SetPosition(1, targetPos); // ÷�����I
            yield return null;
        }

        // ����÷��
        grappleRope.enabled = false;
    }

    // �ոաG�i�����˴��d��
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, grappleRange);
    }

    // ���ѥ~���X�ݡA�ˬd�O�_���b����
    public bool IsGrappling()
    {
        return isGrappling;
    }
}