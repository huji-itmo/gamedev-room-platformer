using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private static readonly int HoldAttackHash = Animator.StringToHash("holdAttack");
    private static readonly int FloatingHash = Animator.StringToHash("floating");
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform modelTransform;
    [SerializeField] private float airControlMultiplier = 0.5f;

    [Tooltip("Input magnitude required to START or RESUME running")]
    [SerializeField] private float runThreshold = 0.7f;

    [Tooltip("Speed multiplier when in Walking State")]
    [SerializeField] private float walkSpeedMultiplier = 0.2f;

    private bool _isInputtingWalk = false;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private OnGroundCollider groundCollider;
    [SerializeField] private Animator animator;

    private static readonly int InputSpeedHash = Animator.StringToHash("inputSpeed");

    [SerializeField] private ThirdPersonCamera playerCamera;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _attackAction;

    private Vector2 _moveInput;
    private bool _isJumping;
    private bool _isSprinting;
    private bool _isAttacking;

    private bool isInAttackState
    {
        get {
           if (animator == null)
            {
                return false;
            }

            var state = animator.GetCurrentAnimatorStateInfo(0);

            List<string> attackNames = new()
            {
                "Attack1", "Attack2", "Attack3",
            };

            return attackNames.Any(name => state.IsName(name));
        }
    }

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
        _sprintAction = InputSystem.actions["Sprint"];
        _attackAction = InputSystem.actions["Attack"];
    }

    void Update()
    {
        if (_moveAction != null)
            _moveInput = _moveAction.ReadValue<Vector2>();

        if (_jumpAction != null)
            _isJumping = _jumpAction.IsPressed();

        if (_sprintAction != null)
            _isSprinting = _sprintAction.IsPressed();

        if (_attackAction != null)
            _isAttacking = _attackAction.IsPressed();
    }

    void FixedUpdate()
    {
        if (rb == null || playerCamera == null) return;

        bool isGrounded = groundCollider.IsOnGround;

        float inputMagnitude = _moveInput.magnitude;

        _isInputtingWalk = inputMagnitude < runThreshold;

        Vector3 moveDirection = CalculateMoveDirection();

        bool isSharpTurn = false;

        if (moveDirection.magnitude > 0.1f)
        {
            RotateModelSmoothly(moveDirection, isGrounded);

            Vector3 forwardDir = modelTransform.forward.normalized;
            Vector3 inputDir   = moveDirection.normalized;

            float dot = Vector3.Dot(forwardDir, inputDir);

            if (dot < 0f)
            {
                isSharpTurn = true;
            }
        }

        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new(currentVelocity.x, 0, currentVelocity.z);

        float controlFactor;
        if (!isGrounded)
        {
            controlFactor = airControlMultiplier;
        } else
        {
            if (isInAttackState)
            {
                controlFactor = 0.05f;
            }
            else if (isSharpTurn)
            {
                controlFactor = walkSpeedMultiplier;
            }
            else
            {
                controlFactor = 1f;
            }
        }

        Vector3 targetHorizontalVelocity = moveDirection * moveSpeed;

        Vector3 newHorizontalVelocity = Vector3.Lerp(horizontalVelocity, targetHorizontalVelocity, controlFactor);

        float animSpeed = newHorizontalVelocity.magnitude * controlFactor / moveSpeed ;
        animator.SetFloat(InputSpeedHash, Mathf.Clamp01(animSpeed));
        animator.SetBool(FloatingHash, !isGrounded);
        animator.SetBool(HoldAttackHash, _isAttacking);

        rb.linearVelocity = new Vector3(newHorizontalVelocity.x, currentVelocity.y, newHorizontalVelocity.z);

        if (_isJumping && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            _isJumping = false;
            SnapRotationToMoveDirection(moveDirection);
        }
    }

    private Vector3 CalculateMoveDirection()
    {
        if (_moveInput == Vector2.zero) return Vector3.zero;

        Vector3 camForward = playerCamera.ForwardDirection;
        Vector3 camRight = playerCamera.RightDirection;

        Vector3 direction = (camForward * _moveInput.y) + (camRight * _moveInput.x);

        return direction.normalized;
    }

    private void RotateModelSmoothly(Vector3 moveDirection, bool isGrounded)
    {
        if (moveDirection == Vector3.zero || !isGrounded) return;

        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

        modelTransform.rotation = Quaternion.RotateTowards(modelTransform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    private void SnapRotationToMoveDirection(Vector3 moveDirection)
    {
        if (moveDirection == Vector3.zero) return;

        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

        modelTransform.rotation = targetRotation;
    }
}
