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
    public LayerMask Danger;
    bool isDanger;
    bool isGrounded;

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

    EnemyAI enemyAI;
    public bool isGuard;
    public bool parrySuccess;
    private bool parryEffectTriggered = false; // 新增：追蹤特效是否已觸發
    private bool hitEffectTriggered = false; // 新增：追蹤受傷特效是否已觸發

    // 新增：Parry 特效事件
    public System.Action<Vector3> OnParrySuccess; // 當 Parry 成功時觸發，參數為碰撞點
    // 新增：Guard 特效事件
    public System.Action<Vector3> OnGuardSuccess; // 當 Guard 成功時觸發，參數為碰撞點
    // 新增：Hit 特效事件
    public System.Action<Vector3> OnHitOccurred; // 當受傷時觸發，參數為碰撞點
    public float attackRadius = 15f;
    public float attackAngle = 120f;
    public LayerMask targetLayer;
    private const float maxDeltaTime = 0.02f; // 對應 50 FPS
    float deltaTime;
    public TPCamera _TPCamera;
    Transform lockTarget;
    bool isLock;
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
        if (_animator != null)
        {
            _animator.SetInteger("ComboStep", comboStep);
        }
    }
    IEnumerator Dash(Vector3 dashDirection)
    {
        isDashing = true;
        canMove = false;
        if (_animator != null)
        {
            _animator.SetBool("isDashing", isDashing);
            _animator.SetTrigger("Dash");
        }
        DisableGuard();
        transform.rotation = Quaternion.LookRotation(dashDirection);
        float dashTime = 0.2f;
        float elapsed = 0f;
        while (elapsed < dashTime)
        {
            if (_characterController != null)
            {
                _characterController.Move(dashDirection * dashDis * deltaTime / dashTime);
            }
            elapsed += deltaTime;
            yield return null;
        }
        canMove = true;
        isDashing = false;
        if (_animator != null)
        {
            _animator.SetBool("isDashing", isDashing);
        }
    }
    //Guard
    void EnableGurad()
    {
        isGuard = true;
        if (_animator != null)
        {
            _animator.SetBool("Guard", isGuard);
        }
    }
    void DisableGuard()
    {
        isGuard = false;
        if (_animator != null)
        {
            _animator.SetBool("Guard", isGuard);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _playerGrapple = GetComponent<PlayerGrapple>();
        _playerStatus = GetComponent<PlayerStatus>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // 初始化 parrySuccess 為 false
        parrySuccess = false;

        // 確保有ExecutionSystem組件
        if (GetComponent<ExecutionSystem>() == null)
        {
            gameObject.AddComponent<ExecutionSystem>();
        }

        // 訂閱 PlayerStatus 的受傷事件
        if (_playerStatus != null)
        {
            _playerStatus.OnHitOccurred += HandlePlayerHit;
        }
    }
    // Update is called once per frame
    void Update()
    {
        deltaTime = Mathf.Min(Time.deltaTime, maxDeltaTime);

        if (_TPCamera != null)
        {
            lockTarget = _TPCamera.lockTarget;
            isLock = _TPCamera.isLock;
        }
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (_animator == null || (_playerGrapple != null && _playerGrapple.IsGrappling()) || (_playerStatus != null && _playerStatus.isDeath == true) || stateInfo.IsTag("Sit")) return;

        if (stateInfo.IsTag("Attack") || stateInfo.IsTag("Hit") || stateInfo.IsTag("Stagger") || stateInfo.IsTag("Heal") || stateInfo.IsTag("Execution"))
        {
            canMove = false;
        }
        else
        {
            canMove = true;
        }
        //Debug.Log(comboTimer);
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, Ground);
            isDanger = Physics.CheckSphere(groundCheck.position, groundDistance, Danger);
        }
        else
        {
            isGrounded = true; // 如果沒有 groundCheck，假設在地面上
        }
        if (_animator != null)
        {
            _animator.SetBool("isGrounded", isGrounded);
        }
        if (comboTimer > 0)
        {
            comboTimer -= deltaTime;
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
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
                    if (enemyAI != null)
                    {
                        parrySuccess = enemyAI.canBeParried;
                        if (parrySuccess && !parryEffectTriggered)
                        {
                            Debug.Log("[TPController] Parry 成功，觸發動畫和特效");
                            int parry = UnityEngine.Random.Range(1, 3);
                            if (_animator != null)
                            {
                                _animator.SetTrigger("Parry" + parry);
                            }

                            // 標記特效已觸發
                            parryEffectTriggered = true;

                            // 觸發 Parry 特效事件（只觸發玩家的特效）
                            if (OnParrySuccess != null)
                            {
                                Vector3 parryPosition = hit.transform.position;
                                OnParrySuccess.Invoke(parryPosition);
                            }

                            // 觸發敵人 Parry 成功事件（只用於模糊效果，不產生特效）
                            if (enemyAI.OnEnemyParrySuccess != null)
                            {
                                Vector3 enemyParryPosition = hit.transform.position;
                                enemyAI.OnEnemyParrySuccess.Invoke(enemyParryPosition);
                            }

                            // 被Parry的敵人增加架勢值20
                            HealthPostureController enemyHealthController = hit.GetComponent<HealthPostureController>();
                            if (enemyHealthController != null)
                            {
                                Debug.Log($"[TPController] 玩家Parry敵人成功！敵人: {hit.name}, 位置: {hit.transform.position}");
                                Debug.Log($"[TPController] 敵人架勢值增加來源: TPController.cs (玩家Parry敵人)");
                                Debug.Log($"[TPController] 增加數值: 20, 是否為Parry: false");
                                enemyHealthController.AddPosture(20, false);
                                Debug.Log("[TPController] 被Parry的敵人架勢值增加20");
                            }
                            else
                            {
                                Debug.LogWarning($"[TPController] 警告：敵人 {hit.name} 沒有 HealthPostureController 組件");
                            }

                            // 確保特效有足夠時間觸發
                            StartCoroutine(ResetParrySuccessAfterDelay(0.2f));
                        }
                    }
                }
            }
            if (!parrySuccess)
            {
                EnableGurad();
            }
        }
        if (!stateInfo.IsTag("Parry"))
            parrySuccess = false;
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
            if (_animator != null)
            {
                _animator.SetFloat("Horizontal", fH);
                _animator.SetFloat("Vertical", fV);
            }
            Vector2 inputVector = new Vector2(fH, fV);
            float inputMagnitude = inputVector.magnitude;
            Vector3 moveDirection;
            if (isLock && lockTarget != null)
            {
                Vector3 directionToTarget = lockTarget.position - transform.position;
                directionToTarget.y = 0;
                Vector3 lockForward = directionToTarget.normalized;
                Vector3 lockRight = Vector3.Cross(Vector3.up, lockForward);
                moveDirection = lockRight * fH + lockForward * fV;
                moveDirection.Normalize();
                if (!isDashing && !isRunning)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lockForward), rotateSensitivity * deltaTime);
                }
                else
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSensitivity * deltaTime);
                }
            }
            else
            {
                if (tpCamera != null)
                {
                    Transform camTransform = tpCamera.transform;
                    // get move direction
                    moveDirection = camTransform.right * fH + camTransform.forward * fV;
                    moveDirection.y = 0;
                    moveDirection.Normalize();
                    if (moveDirection.sqrMagnitude > 0.001f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSensitivity * deltaTime);
                    }
                }
                else
                {
                    // 如果沒有相機，使用世界座標系
                    moveDirection = Vector3.right * fH + Vector3.forward * fV;
                    moveDirection.y = 0;
                    moveDirection.Normalize();
                    if (moveDirection.sqrMagnitude > 0.001f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSensitivity * deltaTime);
                    }
                }
            }
            // Dash �� �]�B
            if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && !isLock && canMove)
            {
                StartCoroutine(Dash(transform.forward));
                isRunning = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftShift) && lockTarget != null && !isDashing && isLock && canMove)
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isGrounded)
                {
                    DisableGuard();
                    verticalVelocity = jumpForce;
                    if (_animator != null)
                    {
                        _animator.SetTrigger("Jump");
                    }
                }
                else if (isDanger)
                {
                    DisableGuard();
                    verticalVelocity = jumpForce;
                    _animator.SetTrigger("DangerJump");
                }
            }
            verticalVelocity += gravity * deltaTime;
            Vector3 velocity = moveSpeed * moveDirection;
            velocity.y = Mathf.Max(verticalVelocity, -100);
            if (_characterController != null)
            {
                _characterController.Move(velocity * deltaTime);
            }
            if (_animator != null)
            {
                _animator.SetFloat("Speed", moveSpeed);
            }
        }
        else
        {
            verticalVelocity += gravity * deltaTime;
            Vector3 velocity = Vector3.zero;
            velocity.y = Mathf.Max(verticalVelocity, -100);
            if (_characterController != null)
            {
                _characterController.Move(velocity * deltaTime);
            }
            if (_animator != null)
            {
                _animator.SetFloat("Speed", moveSpeed);
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isDashing)
        {
            DisableGuard();
            if (isLock && lockTarget != null)
            {
                Vector3 directionToTarget = lockTarget.position - transform.position;
                directionToTarget.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = targetRotation;
            }
            if (canCombo && comboStep < 4)
            {
                comboStep++;
                if (_animator != null)
                {
                    _animator.SetInteger("ComboStep", comboStep);
                }
                canCombo = false;
            }
            else if (comboStep == 0)
            {
                comboStep = 1;
                if (_animator != null)
                {
                    _animator.SetInteger("ComboStep", comboStep);
                    _animator.SetTrigger("Attack");
                }
            }
        }
    }
    // 延遲重置 parrySuccess 狀態，確保特效有足夠時間觸發
    private IEnumerator ResetParrySuccessAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        parrySuccess = false;
        parryEffectTriggered = false; // 重置特效觸發標記
        Debug.Log("[TPController] Parry 狀態已重置");
    }

    // 處理玩家受傷事件
    private void HandlePlayerHit(Vector3 hitPosition)
    {
        if (!hitEffectTriggered && OnHitOccurred != null)
        {
            hitEffectTriggered = true;
            OnHitOccurred.Invoke(hitPosition);
            StartCoroutine(ResetHitEffectAfterDelay(0.2f));
        }
    }

    // 延遲重置受傷特效觸發標記
    private IEnumerator ResetHitEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hitEffectTriggered = false;
        Debug.Log("[TPController] Hit 特效已重置");
    }
}
