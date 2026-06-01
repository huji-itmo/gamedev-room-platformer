using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform; // Assign your main camera
    [SerializeField] private PauseManager pauseManager; // Assign your main camera

    private CharacterController _controller;
    private Vector3 _playerVelocity;
    private bool _isGrounded;
    private float _xRotation = 0f;

    // Input fields (polled in Update)
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _isJumping;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        
        // Match your pattern: lookup actions by name from the global InputSystem
        _moveAction = InputSystem.actions["Move"];
        _lookAction = InputSystem.actions["Look"];
        _jumpAction = InputSystem.actions["Jump"];
    }

    private void OnEnable()
    {
        _moveAction?.Enable();
        _lookAction?.Enable();
        _jumpAction?.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        _moveAction?.Disable();
        _lookAction?.Disable();
        _jumpAction?.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Poll inputs (exactly like your example)
        if (_moveAction != null)
            _moveInput = _moveAction.ReadValue<Vector2>();
        
        if (_lookAction != null)
            _lookInput = _lookAction.ReadValue<Vector2>();
        
        if (_jumpAction != null)
            _isJumping = _jumpAction.IsPressed();

        // Mouse look is non-physics → keep in Update
        HandleMouseLook();
    }

    private void FixedUpdate()
    {
        // Ground detection
        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _playerVelocity.y < 0)
            _playerVelocity.y = -2f; // Smooth grounding

        // Movement
        Vector3 moveDirection = CalculateMoveDirection();
        _controller.Move(moveDirection * moveSpeed * Time.fixedDeltaTime);

        // Jump (triggered on press, applied in physics step)
        if (_isGrounded && _isJumping)
        {
            _playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _isJumping = false; // consume input
        }

        // Gravity
        _playerVelocity.y += gravity * Time.fixedDeltaTime;
        _controller.Move(_playerVelocity * Time.fixedDeltaTime);
    }

    private Vector3 CalculateMoveDirection()
    {
        if (_moveInput == Vector2.zero) return Vector3.zero;
        
        // First-person: move relative to player's forward/right (not camera)
        Vector3 direction = (transform.forward * _moveInput.y) + (transform.right * _moveInput.x);
        return direction.normalized;
    }

    private void HandleMouseLook()
    {

        if (_lookInput == Vector2.zero || pauseManager.IsPaused()) return;

        float mouseX = _lookInput.x * mouseSensitivity;
        float mouseY = _lookInput.y * mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f); // Prevent neck break

        // Rotate camera locally (pitch)
        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        // Rotate player body globally (yaw)
        transform.Rotate(Vector3.up * mouseX);
    }
}