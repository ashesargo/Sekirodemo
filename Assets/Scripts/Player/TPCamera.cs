using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPCamera : MonoBehaviour
{
    public Transform mFollowPoint;
    public Transform mFollowPointRef;

    public float mFollowDistance;
    public float mMinFollowDistance;
    public float mMaxFollowDistance;

    private float mVerticalDegree;
    public float mVerticalLimitUp;
    public float mVerticalLimitDown;

    private Vector3 mHorizontalVector;
    public float mMouseRotateSensitivity = 1.0f;
    public float followSpeed = 10.0f;
    private Vector3 mCurrentVel = Vector3.zero;
    public LayerMask mCheckLayer;
    private bool wasLock;
    [Header("Âê©w")]
    public bool isLock;
    public Transform lockTarget;
    public float lockCameraHeight = 1.5f;
    // Start is called before the first frame update
    void Start()
    {
        mFollowPoint.position = mFollowPointRef.position;
        mFollowPoint.rotation = mFollowPointRef.rotation;
        transform.position = mFollowPoint.position - mFollowDistance * mFollowPoint.forward;
        Vector3 vDir = transform.position - mFollowPoint.position;
        mHorizontalVector = vDir;
        mHorizontalVector.y = 0.0f;
        mHorizontalVector.Normalize();
    }
    public void UpdateCameraTransform()
    {
        if (!isLock)
        {
            float fMX = Input.GetAxis("Mouse X");
            float fMY = Input.GetAxis("Mouse Y");
            mHorizontalVector = Quaternion.AngleAxis(fMX * mMouseRotateSensitivity, Vector3.up) * mHorizontalVector;
            Vector3 rotationAxis = Vector3.Cross(mHorizontalVector, Vector3.up);
            mVerticalDegree -= fMY * mMouseRotateSensitivity;
            if (mVerticalDegree < -mVerticalLimitUp)
            {
                mVerticalDegree = -mVerticalLimitUp;
            }
            else if (mVerticalDegree > mVerticalLimitDown)
            {
                mVerticalDegree = mVerticalLimitDown;
            }
            Vector3 vFinalDir = Quaternion.AngleAxis(mVerticalDegree, rotationAxis) * mHorizontalVector;
            vFinalDir.Normalize();
            mFollowPoint.position = Vector3.Lerp(mFollowPoint.position, mFollowPointRef.position, followSpeed * Time.deltaTime);
            Vector3 vFinalPosition = mFollowPoint.position + vFinalDir * mFollowDistance;
            Vector3 vDir = mFollowPoint.position - vFinalPosition;
            vDir.Normalize();

            RaycastHit rh;
            Ray r = new Ray(mFollowPoint.position, -vDir);

            if (Physics.SphereCast(r, 0.1f, out rh, mFollowDistance, mCheckLayer))
            {
                vFinalPosition = mFollowPoint.position - vDir * (rh.distance - 0.1f);
            }

            //if (Physics.Linecast(mLookAtPoint.position, vFinalPosition, out rh, mCheckLayer))
            //{
            //    Vector3 vHit = rh.point + vDir * 0.1f;
            //    vFinalPosition = vHit;
            //}
            transform.position = Vector3.Lerp(transform.position, vFinalPosition, 1.0f);
            // transform.position = Vector3.SmoothDamp(transform.position, vFinalPosition, ref mCurrentVel, 0.01f, 10.0f);
            //transform.position = vFinalPosition;
            vDir = mFollowPoint.position - transform.position;
            transform.forward = vDir;
        }
        else if (lockTarget != null)
        {
            mFollowPoint.position = Vector3.Lerp(mFollowPoint.position, mFollowPointRef.position, followSpeed * Time.deltaTime);
            Vector3 lockDirection = lockTarget.position - mFollowPoint.position;
            lockDirection.y = 0;
            lockDirection.Normalize();
            Vector3 vFinalDir = lockDirection;
            Vector3 offset = Vector3.up * lockCameraHeight;
            Vector3 vFinalPosition = mFollowPoint.position + offset - vFinalDir * mFollowDistance;
            transform.position = Vector3.Lerp(transform.position, vFinalPosition, Time.deltaTime * followSpeed);
            Vector3 lookTarget = lockTarget.position + Vector3.up * lockCameraHeight;
            transform.forward = (lookTarget - transform.position).normalized;
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (wasLock != isLock)
        {
            mFollowPoint.position = mFollowPointRef.position;
            mFollowPoint.rotation = mFollowPointRef.rotation;
            transform.position = mFollowPoint.position - mFollowDistance * mFollowPoint.forward;
            Vector3 vDir = transform.position - mFollowPoint.position;
            mHorizontalVector = vDir;
            mHorizontalVector.y = 0.0f;
            mHorizontalVector.Normalize();
        }
        UpdateCameraTransform();
        wasLock = isLock;
    }    
}
