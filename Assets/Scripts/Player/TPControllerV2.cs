using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
[RequireComponent(typeof(CharacterController))]
public class TPContrallerV2 : MonoBehaviour
{
    [Header("��v��")]
    public Transform tpCamera;
    public float rotateSensitivity;

    [Header("Lock")]
    public Transform lockTarget;
    private bool isLocked = false;
    public float lockRange = 10.0f;
    public LayerMask enemyLayer;
    public TPCamera TPCamera;

    [Header("���ʰѼ�")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float dodgeDistance = 5f;
    public float dodgeCooldown = 1f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("���� Combo")]
    public int maxCombo = 3;
    public float comboResetTime = 1.0f;

    private CharacterController controller;
    private Animator animator;

    private Vector3 velocity;
    private bool isGrounded;
    private float dodgeTimer;

    // Combo �t���ܼ�
    private int comboStep = 0;
    private bool isAttacking = false;
    private bool canCombo = false;
    private float comboTimer = 0f;

    private enum PlayerState { Idle, Walk, Run, Jump, Dodge, Attack, Block, Parry }
    private PlayerState currentState = PlayerState.Idle;
    private void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }
    private void ReturnToIdleIfGrounded()
    {
        if (!isGrounded) return;
        ChangeState(PlayerState.Idle);
        animator.SetBool("IsBlocking", false);
        animator.SetBool("IsMoving", false);
        animator.SetBool("IsRunning", false);
    }
    private void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }
    private void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
        ApplyGravity();
        if (!isAttacking)
            HandleMovement();
        HandleActions();
        if (isAttacking && Time.time > comboTimer)
            ResetCombo();
    }
    private void HandleMovement()
    {
        float fH = Input.GetAxis("Horizontal");
        float fV = Input.GetAxis("Vertical");
        Vector2 inputVector = new Vector2(fH, fV);
        float inputMagnitude = inputVector.magnitude;
        Vector3 moveDirection;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && inputVector.magnitude > 0.1f;
        float speed = isRunning ? runSpeed : walkSpeed;
        if (isLocked && lockTarget != null)
        {
            Vector3 directionToTarget = lockTarget.position - transform.position;
            directionToTarget.y = 0;
            Vector3 lockForward = directionToTarget.normalized;
            Vector3 lockRight = Vector3.Cross(Vector3.up, lockForward);
            moveDirection = lockRight * fH + lockForward * fV;
            moveDirection.Normalize();
            if (currentState == PlayerState.Run || currentState == PlayerState.Dodge)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSensitivity * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lockForward), rotateSensitivity * Time.deltaTime);
            }
        }
        else
        {
            Transform camTransform = tpCamera.transform;
            // get move direction
            moveDirection = camTransform.right * fH + camTransform.forward * fV;
            moveDirection.y = 0;
            moveDirection.Normalize();
            if (moveDirection.sqrMagnitude > 0.001f) // �T�O����V��J�~����
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSensitivity * Time.deltaTime);
            }
        }
        if (inputVector.magnitude > 0.1f)
        {
            ChangeState(isRunning ? PlayerState.Run : PlayerState.Walk);
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsRunning", false);
            ChangeState(PlayerState.Idle);
        }
        controller.Move(moveDirection * speed * Time.deltaTime);
    }
    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    private void HandleActions()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && dodgeTimer <= 0)
        {
            Dodge();
        }
        if (Input.GetMouseButtonDown(0))
            HandleAttack();
        if (Input.GetMouseButtonDown(1))
            TryParry();
        if (Input.GetMouseButton(1))
            Block();
        else
            animator.SetBool("IsBlocking", false);
    }
    private void Jump()
    {
        if (isAttacking) return;
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        animator.SetTrigger("Jump");
        currentState = PlayerState.Jump;
    }
    private void Dodge()
    {
        if (isAttacking) return;
        ChangeState(PlayerState.Dodge);
        Vector3 dodgeDir = transform.forward;
        controller.Move(dodgeDir * dodgeDistance);
        animator.SetTrigger("Dodge");
        dodgeTimer = dodgeCooldown;
        Invoke(nameof(ResetDodge), dodgeCooldown);
    }
    private void ResetDodge() => dodgeTimer = 0;
    private void Block()
    {
        if (isAttacking) return;
        animator.SetBool("IsBlocking", true);
        ChangeState(PlayerState.Block);
    }
    private void TryParry()
    {
        if (isAttacking) return;
        animator.SetTrigger("Parry");
        ChangeState(PlayerState.Parry);
    }
    private void HandleAttack()
    {
        if (currentState == PlayerState.Dodge || currentState == PlayerState.Jump) return;

        if (!isAttacking)
        {
            // ��������
            isAttacking = true;
            comboStep = 1;
            PlayAttackAnimation(comboStep);
        }
        else if (canCombo)
        {
            comboStep++;
            if (comboStep > maxCombo) comboStep = 1;
            PlayAttackAnimation(comboStep);
            canCombo = false;
        }
        comboTimer = Time.time + comboResetTime;
        ChangeState(PlayerState.Attack);
    }
    private void PlayAttackAnimation(int step)
    {
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");
        animator.SetTrigger("Attack" + step);
    }
    public void OnComboWindowStart() => canCombo = true;
    public void OnComboWindowEnd() => canCombo = false;
    public void OnAttackAnimationEnd()
    {
        ResetCombo();
        ReturnToIdleIfGrounded();
    }
    private void ResetCombo()
    {
        comboStep = 0;
        isAttacking = false;
        canCombo = false;
    }
}
