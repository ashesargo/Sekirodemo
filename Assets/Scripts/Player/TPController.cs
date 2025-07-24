using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class TPContraller : MonoBehaviour
{
    public Transform tpCamera;
    private float moveSpeed;
    public float speed;
    public float rotateSensitivity;
    private Animator _animator;
    private CharacterController _characterController;
    PlayerGrapple _playerGrapple;
    PlayerStatus _playerStatus;
    public float gravity = -9.81f;
    public float jumpForce = 8f;
    private float verticalVelocity;
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 1f;
    public LayerMask Ground;
    private bool isGrounded;

    int comboStep = 0;
    int currentStep = 0;
    bool canCombo = false;
    bool canMove = true;
    bool hit = false;
    private float comboTimer = 0f;
    private float comboWindow = 1f;
    [Header("Dash")]
    public float dashDis;
    private bool isDashing;
    private bool isRunning = false;
    [Header("Lock")]
    public Transform lockTarget;
    private bool isLocked = false;
    public float lockRange = 10.0f;
    public LayerMask enemyLayer;
    public TPCamera TPCamera;

    EnemyAI enemyAI;
    public bool isGuard;
    public bool parrySuccess;
    public float attackRadius = 9f;
    public float attackAngle = 90f;
    public LayerMask targetLayer;

    public void StartAttack()
    {
        currentStep = comboStep;
        canCombo = true;
        comboTimer = comboWindow;
    }
    public void EndAttack()
    {
        if (comboStep > currentStep) return;
        ResetCombo();
    }
    public void ResetCombo()
    {
        comboStep = 0;
        canCombo = false;
        comboTimer = 0f;
        _animator.SetInteger("ComboStep", comboStep);
    }
    IEnumerator Dash(Vector3 dashDirection)
    {
        isDashing = true;
        canMove = false;
        _animator.SetBool("isDashing", isDashing);
        DisableGuard();
        _animator.SetTrigger("Dash");
        transform.rotation = Quaternion.LookRotation(dashDirection);
        float dashTime = 0.2f;
        float elapsed = 0f;
        while (elapsed < dashTime)
        {
            _characterController.Move(dashDirection * dashDis * Time.deltaTime / dashTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canMove = true;
        isDashing = false;
        _animator.SetBool("isDashing", isDashing);
    }
    void FindLockTarget()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, lockRange, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;
        foreach (Collider col in targets)
        {
            // 獲取敵人的 Animator 組件
            Animator animator = col.GetComponent<Animator>();
            if (animator != null)
            {
                // 檢查敵人是否處於 "Death" 動畫狀態
                if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Death"))
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = col.transform;
                    }
                }
            }
        }
        // 更新鎖定目標
        if (closestTarget != null)
        {
            lockTarget = closestTarget;
            isLocked = true;
        }
        else
        {
            // 如果沒有可鎖定的目標，解除鎖定
            lockTarget = null;
            isLocked = false;
        }
        // 檢查當前鎖定目標是否進入 "Death" 狀態
        if (lockTarget != null)
        {
            Animator targetAnimator = lockTarget.GetComponent<Animator>();
            if (targetAnimator != null && targetAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Death"))
            {
                // 如果當前鎖定目標進入 "Death" 狀態，解除鎖定
                lockTarget = null;
                isLocked = false;
            }
        }
    }
    //Guard
    void EnableGurad()
    {
        isGuard = true;
        _animator.SetBool("Guard", isGuard);
    }
    void DisableGuard()
    {
        isGuard = false;
        _animator.SetBool("Guard", isGuard);
    }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _playerGrapple = GetComponent<PlayerGrapple>();
        _playerStatus = GetComponent<PlayerStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsTag("Attack") || stateInfo.IsTag("Hit") || stateInfo.IsTag("Stagger"))
        {
            canMove = false;
        }
        else
        {
            canMove = true;
        }
        //Debug.Log(comboTimer);
        if (_playerGrapple.IsGrappling() || _playerStatus.isDeath == true) return;
        TPCamera.isLock = isLocked;
        TPCamera.lockTarget = lockTarget;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, Ground);
        _animator.SetBool("isGrounded", isGrounded);
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            if (!isLocked)
            {
                FindLockTarget();
                _animator.SetBool("Lock", isLocked);
            }
            else
            {
                isLocked = false;
                lockTarget = null;
                _animator.SetBool("Lock", isLocked);
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) )
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, targetLayer);
            foreach (var hit in hits)
            {
                // 判斷是否在前方角度內
                Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToTarget); // 跟前方夾角
                if (angle <= attackAngle * 0.5f) // 扇形角度範圍內
                {
                    enemyAI = hit.GetComponent<EnemyAI>();
                    parrySuccess = enemyAI.canBeParried;
                    if (parrySuccess)
                    {
                        int parry = UnityEngine.Random.Range(1, 3);
                        _animator.SetTrigger("Parry" + parry);
                    }
                }
            }
            if (!parrySuccess)
            {
                EnableGurad();
            }
        }
        if (Input.GetKey(KeyCode.Mouse1))
        {
            EnableGurad();
        }
        if (Input.GetKeyUp(KeyCode.Mouse1) && isGuard)
        {
            DisableGuard();
        }
        if (canMove)
        {
            float fH = Input.GetAxis("Horizontal");
            float fV = Input.GetAxis("Vertical");
            _animator.SetFloat("Horizontal", fH);
            _animator.SetFloat("Vertical", fV);
            Vector2 inputVector = new Vector2(fH, fV);
            float inputMagnitude = inputVector.magnitude;
            Vector3 moveDirection;
            if (isLocked && lockTarget != null)
            {
                Vector3 directionToTarget = lockTarget.position - transform.position;
                directionToTarget.y = 0;
                Vector3 lockForward = directionToTarget.normalized;
                Vector3 lockRight = Vector3.Cross(Vector3.up, lockForward);
                moveDirection = lockRight * fH + lockForward * fV;
                moveDirection.Normalize();
                if (!isDashing && !isRunning)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lockForward), rotateSensitivity * Time.deltaTime);
                }
                else
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSensitivity * Time.deltaTime);
                }
            }
            else
            {
                Transform camTransform = tpCamera.transform;
                // get move direction
                moveDirection = camTransform.right * fH + camTransform.forward * fV;
                moveDirection.y = 0;
                moveDirection.Normalize();
                if (moveDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSensitivity * Time.deltaTime);
                }
            }
            // Dash �� �]�B
            if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && !isLocked && canMove)
            {
                StartCoroutine(Dash(transform.forward));
                isRunning = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftShift) && lockTarget != null && !isDashing && isLocked && canMove)
            {
                StartCoroutine(Dash(moveDirection));
                isRunning = true;
            }
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                isRunning = false;
            }
            if (inputMagnitude > 0.1f)
            {
                if (isGuard)
                {
                    moveSpeed = speed * 0.6f;
                }
                else if (isRunning)
                {
                    moveSpeed = speed * 2f;
                }
                else
                {
                    moveSpeed = speed;
                }
            }
            else
            {
                moveSpeed = 0;
            }
            if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                DisableGuard();
                verticalVelocity = jumpForce;
                _animator.SetTrigger("Jump");
            }
            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = moveSpeed * moveDirection;
            velocity.y = Mathf.Max(verticalVelocity, -100);
            _characterController.Move(velocity * Time.deltaTime);
            _animator.SetFloat("Speed", moveSpeed);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = Vector3.zero;
            velocity.y = Mathf.Max(verticalVelocity, -100);
            _characterController.Move(velocity * Time.deltaTime);
            _animator.SetFloat("Speed", moveSpeed);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isDashing)
        {
            DisableGuard();
            if (isLocked)
            {
                Vector3 directionToTarget = lockTarget.position - transform.position;
                directionToTarget.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = targetRotation;
            }
            if (canCombo && comboStep < 4)
            {
                comboStep++;
                _animator.SetInteger("ComboStep", comboStep);
                canCombo = false;
            }
            else if (comboStep == 0)
            {
                comboStep = 1;
                _animator.SetInteger("ComboStep", comboStep);
                _animator.SetTrigger("Attack");
            }
        }

    }


    public void TakeDamage(float damage)
    {
        // 調用 PlayerStatus 的 TakeDamage 方法來處理傷害
        if (_playerStatus != null)
        {
            _playerStatus.TakeDamage(damage);
        }
        else
        {
            // 如果沒有 PlayerStatus 組件，至少觸發動畫
            _animator.SetTrigger("Hit");
        }
    }
}
