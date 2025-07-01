using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class TPContraller : MonoBehaviour
{
    public Transform tpCamera;
    private float moveSpeed;
    public float speed;
    public float rotateSensitivity;
    private Animator _animator;
    private CharacterController _characterController;
    public float gravity = -9.81f;
    public float jumpForce = 8f;
    private float verticalVelocity;
    [Header("Ground Check")]
    public Transform groundCheck;             // 設在腳底
    public float groundDistance = 1f;       // 偵測距離
    public LayerMask Ground;              // 設定為地面圖層
    private bool isGrounded;
    //攻擊
    bool lastIsGround;
    int comboStep = 0;
    int currentStep = 0;
    bool canCombo = false;
    bool canMove = true;
    //衝刺 跑步
    [Header("Dash")]
    public float dashDis;
    private bool isDashing;
    private bool isRunning = false;
    //視角鎖定
    [Header("Lock")]
    public Transform lockTarget;
    private bool isLocked = false;
    public float lockRange = 10.0f;
    public LayerMask enemyLayer;
    public TPCamera TPCamera;
    //防禦
    public bool isGuard;
    public bool parrySuccess;
    //攻擊combo
    public void EnableCombo()
    {
        currentStep = comboStep;
        canCombo = true;
    }
    public void DisableCombo()
    {
        if (currentStep < comboStep)
        {
            canCombo = false;
        }
        else
        {
            comboStep = 0;
            _animator.SetInteger("comboStep", comboStep);
            canMove = true;
            canCombo = false;
        }
    }
    //攻擊中不能移動
    public void StartAttack()
    {
        canMove = false;
    }
    public void EndAttack()
    {
        canMove = true;
        comboStep = 0;
        canCombo = false;
        _animator.SetInteger("comboStep", comboStep);
    }
    //衝刺
    IEnumerator Dash(Vector3 dashDirection)
    {
        isDashing = true;
        canMove = false;
        _animator.SetBool("isDashing", isDashing);
        DisableGuard();
        if (isLocked)
        {
            _animator.SetTrigger("LockDash");
        }
        else
        {
            _animator.SetTrigger("Dash");
        }
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
    //尋找鎖定目標
    void FindLockTarget()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, lockRange, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;
        foreach (Collider col in targets)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = col.transform;
            }
        }
        if (closestTarget != null)
        {
            lockTarget = closestTarget;
            isLocked = true;
        }
    }
    //Guard
    void EnableGurad()
    {
        _animator.SetBool("Guard", true);
        isGuard = true;
    }
    void DisableGuard()
    {
        _animator.SetBool("Guard", false);
        isGuard = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        TPCamera.isLock = isLocked;
        TPCamera.lockTarget = lockTarget;
        //是否在地面   
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, Ground);
        _animator.SetBool("isGrounded", isGrounded);
        //鎖定判定
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
        //角色移動
        //if (Input.GetKeyDown(KeyCode.Mouse1) && enemy.canParry)
        //{
        //    _animator.SetTrigger("Parry");
        //}
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            EnableGurad();
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
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
            //是否鎖定 方向不同
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
                if (moveDirection.sqrMagnitude > 0.001f) // 確保有方向輸入才旋轉
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSensitivity * Time.deltaTime);
                }
            }
            // Dash 及 跑步
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
            // 如果有輸入方向 判斷速度
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
            // 跳
            if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                DisableGuard();
                verticalVelocity = jumpForce;
                if (isLocked)
                {
                    _animator.SetTrigger("LockJump");
                }
                else
                {
                    _animator.SetTrigger("Jump");
                }
            }
            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = moveSpeed * moveDirection;
            velocity.y = verticalVelocity;
            _characterController.Move(velocity * Time.deltaTime);
            _animator.SetFloat("Speed", moveSpeed);
        }
        //就算不能移動 也有重力判定
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = Vector3.zero;
            velocity.y = verticalVelocity;
            _characterController.Move(velocity * Time.deltaTime);
            _animator.SetFloat("Speed", moveSpeed);
        }
        //攻擊               
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
                _animator.SetInteger("comboStep", comboStep);
                canCombo = false;
            }
            else if (comboStep == 0 )
            {
                comboStep = 1;
                _animator.SetInteger("comboStep", comboStep);
                if (isLocked)
                {
                    _animator.SetTrigger("LockAttack");
                }
                else
                {
                    _animator.SetTrigger("Attack");
                }
            }           
        }
        if(lastIsGround != isGrounded)
        {
            EndAttack();
        }
        lastIsGround = isGrounded;
    }
}
