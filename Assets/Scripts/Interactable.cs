using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private string interactActionName = "Interact";
    [SerializeField] private string animatorTriggerName = "Interact";
    [SerializeField] private LayerMask playerLayer = 1 << 3;
    [SerializeField] private bool InteractiveEnabled = true;

    [Header("References")]
    [SerializeField] private Animator animator;

    private InputAction _interactAction;
    private bool _isPlayerInside;
    private bool _interactPressed;

    private void Awake()
    {
        _interactAction = InputSystem.actions[interactActionName];

        if (animator == null)
            animator = GetComponent<Animator>();

        var collider = GetComponent<Collider>();
        if (!collider.isTrigger)
        {
            Debug.LogWarning($"[Interactable] Collider on '{gameObject.name}' is not marked as Trigger!", this);
        }
    }

    private void OnEnable() => _interactAction?.Enable();
    private void OnDisable() => _interactAction?.Disable();

    private void Update()
    {
        if (!InteractiveEnabled)
        {
            return;
        }

        if (_interactAction != null)
            _interactPressed = _interactAction.WasPressedThisFrame();

        if (_interactPressed && _isPlayerInside)
        {
            TriggerInteraction();
            _interactPressed = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
            _isPlayerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
            _isPlayerInside = false;
    }

    private void TriggerInteraction()
    {
        if (animator == null)
        {
            Debug.LogWarning($"[Interactable] Animator is null on {gameObject.name}", this);
            return;
        }

        animator.SetTrigger(animatorTriggerName);

        #if UNITY_EDITOR
        Debug.Log($"[Interactable] Triggered '{animatorTriggerName}' on {gameObject.name}", this);
        #endif
    }

    private void OnDrawGizmosSelected()
    {
        var collider = GetComponent<Collider>();
        if (collider == null) return;

        Gizmos.color = Color.yellow;

        if (collider is SphereCollider sphere)
        {
            Vector3 worldCenter = transform.TransformPoint(sphere.center);
            Gizmos.DrawWireSphere(worldCenter, sphere.radius);
        }
        else if (collider is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
        else if (collider is CapsuleCollider capsule)
        {
            Vector3 worldCenter = transform.TransformPoint(capsule.center);
            Gizmos.DrawWireSphere(worldCenter, capsule.radius);
        }
    }
}
