using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    public Transform target; 
    public LayerMask collisionLayers;

    [Header("Settings")]
    public float defaultDistance = 5f;
    public float minDistance = 0.5f; 
    public float mouseSensitivity = 0.1f; 
    
    public float pitchMin = -40f;
    public float pitchMax = 80f;

    [Header("Target Offset")]
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0);

    [Header("Collision")]
    [Tooltip("Distance to keep from walls/floors. Increase if clipping.")]
    public float collisionOffset = 0.2f;

    [Header("Debug")]
    public bool showDebugLines = true;

    [Header("Smoothing")]
    public float rotationSmoothTime = 0.1f;

    private float currentYaw = 0f;
    private float currentPitch = 0f;

 public Vector3 ForwardDirection
    {
        get
        {
            Vector3 forward = transform.forward;
            forward.y = 0f; 
            return forward.normalized;
        }
    }
    
    public Vector3 RightDirection
    {
        get
        {
            Vector3 right = transform.right;
            right.y = 0f;
            return right.normalized;
        }
    }

    void Awake()
    {
        if (target != null)
        {
            Vector3 forward = target.forward;
            currentYaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        }
    }

    void Start()
    {
        UpdateCameraTransform();
    }

    void OnDestroy()
    {
    }


    void LateUpdate()
    {
        if (target == null) return;
        if (Time.timeScale == 0f) return;

        HandleRotation();
        UpdateCameraTransform();
    }

    void HandleRotation()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            currentYaw += delta.x * mouseSensitivity;
            currentPitch -= delta.y * mouseSensitivity;
            currentPitch = Mathf.Clamp(currentPitch, pitchMin, pitchMax);
        }
    }

    void UpdateCameraTransform()
    {
        
        Vector3 pivotPoint = target.position + targetOffset;

        
        Quaternion targetRotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);

        
        Vector3 idealOffset = transform.rotation * Vector3.back * defaultDistance;
        Vector3 idealPosition = pivotPoint + idealOffset;
        
        Vector3 direction = idealPosition - pivotPoint;
        float desiredMagnitude = direction.magnitude; 
        
        RaycastHit hit;
        
        
        if (Physics.Raycast(pivotPoint, direction.normalized, out hit, desiredMagnitude, collisionLayers))
        {
            Vector3 newPos = hit.point - (direction.normalized * collisionOffset);   
            
            if (Vector3.Distance(newPos, pivotPoint) < minDistance)
            {
                newPos = pivotPoint + (direction.normalized * minDistance);
            }

            transform.position = newPos;
        }
        else
        {
            
            transform.position = idealPosition;
        }

        
        if (showDebugLines)
        {
            Debug.DrawLine(pivotPoint, idealPosition, Color.green); 
            if (Physics.Raycast(pivotPoint, direction.normalized, out hit, desiredMagnitude, collisionLayers))
            {
                Debug.DrawLine(pivotPoint, hit.point, Color.red); 
                Debug.DrawRay(hit.point, Vector3.up * 0.5f, Color.yellow); 
            }
        }
    }
}