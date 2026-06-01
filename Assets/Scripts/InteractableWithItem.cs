using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class InteractableWithItem : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private string interactActionName = "Interact";
    [SerializeField] private LayerMask playerLayer = 1 << 3;
    [SerializeField] private bool interactiveEnabled = true;
    public void SetInteractiveEnabled(bool enabled) => interactiveEnabled = enabled;

    [Header("Item Pickup (Optional)")]
    [Tooltip("If assigned, interacting gives this item to player")]
    [SerializeField] private ItemSO itemToGive;
    [SerializeField] private bool destroyOnPickup = true;

    [Header("Auto-Usage Target")]
    [Tooltip("Tags this object accepts. System auto-finds matching item in inventory.")]
    [SerializeField] private string[] acceptableTags;

    [Header("Events")]
    [Tooltip("Fired when player picks up itemToGive")]
    public UnityEvent<ItemSO> OnItemPickedUp;
    [Tooltip("Fired when auto-found item is successfully used")]
    public UnityEvent<ItemSO> OnItemUsedSuccessfully;
    [Tooltip("Fired when no matching item is found in inventory")]
    public UnityEvent OnItemUseFailed;

    private InputAction _interactAction;
    private bool _isPlayerInside;

    private void Awake()
    {
        _interactAction = InputSystem.actions[interactActionName];
        if (_interactAction == null)
        {
            Debug.LogError($"[InteractableWithItem] Input action '{interactActionName}' not found on '{gameObject.name}'!", this);
            enabled = false;
            return;
        }

        var collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
            Debug.LogWarning($"[InteractableWithItem] Collider on '{gameObject.name}' should be Trigger!", this);
    }

    private void OnEnable()
    {
        if (_interactAction != null && !_interactAction.enabled)
            _interactAction.Enable();
    }

    private void OnDisable() { }

    private void Update()
    {
        if (_interactAction == null) return;

        if (!_interactAction.enabled)
            _interactAction.Enable();

        if (!interactiveEnabled || !_isPlayerInside) return;

        if (_interactAction.WasPressedThisFrame())
            HandleInteraction();
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = ((1 << other.gameObject.layer) & playerLayer) != 0;
        Debug.Log($"[InteractableWithItem] OnTriggerEnter '{other.name}' (layer={other.gameObject.layer}), isPlayer={isPlayer} on '{gameObject.name}'");
        if (isPlayer) _isPlayerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        bool isPlayer = ((1 << other.gameObject.layer) & playerLayer) != 0;
        Debug.Log($"[InteractableWithItem] OnTriggerExit '{other.name}' (layer={other.gameObject.layer}), isPlayer={isPlayer} on '{gameObject.name}'");
        if (isPlayer) _isPlayerInside = false;
    }

    private void HandleInteraction()
    {
        if (itemToGive != null)
        {
            PickupItem();
            return;
        }

        AutoUseItem();
    }

    private void PickupItem()
    {
        if (InventoryManager.Instance == null) { Debug.LogError($"[InteractableWithItem] InventoryManager.Instance is NULL on '{gameObject.name}'!"); return; }
        if (itemToGive == null) { Debug.LogError($"[InteractableWithItem] itemToGive is null on '{gameObject.name}'!"); return; }

        Debug.Log($"[InteractableWithItem] Attempting to add '{itemToGive.itemName}' to inventory on '{gameObject.name}'");
        if (InventoryManager.Instance.AddItem(itemToGive))
        {
            Debug.Log($"[InteractableWithItem] Successfully added '{itemToGive.itemName}' to inventory");
            OnItemPickedUp?.Invoke(itemToGive);
            if (destroyOnPickup) { Debug.Log($"[InteractableWithItem] Destroying '{gameObject.name}' after pickup"); Destroy(gameObject); }
        }
        else
        {
            Debug.LogWarning($"[InteractableWithItem] Failed to add '{itemToGive.itemName}' (already in inventory)");
        }
    }

    private void AutoUseItem()
    {
        if (InventoryManager.Instance == null) { Debug.LogError($"[InteractableWithItem] InventoryManager.Instance is NULL on '{gameObject.name}'!"); return; }

        Debug.Log($"[InteractableWithItem] AutoUseItem on '{gameObject.name}', acceptableTags=[{string.Join(", ", acceptableTags ?? new string[0])}]");
        ItemSO foundItem = InventoryManager.Instance.FindItemMatchingTags(acceptableTags);

        if (foundItem != null)
        {
            Debug.Log($"[InteractableWithItem] Found matching item '{foundItem.itemName}', attempting ConsumeItem");
            InventoryManager.Instance.ConsumeItem(foundItem);
            OnItemUsedSuccessfully?.Invoke(foundItem);
            Debug.Log($"[InteractableWithItem] Auto-used '{foundItem.itemName}' on {gameObject.name}");
        }
        else
        {
            Debug.Log($"[InteractableWithItem] No matching item found in inventory for tags on '{gameObject.name}'");
            OnItemUseFailed?.Invoke();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            if (collider is SphereCollider s)
                Gizmos.DrawWireSphere(transform.TransformPoint(s.center), s.radius);
            else if (collider is BoxCollider b)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(b.center, b.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }

        if (acceptableTags != null && acceptableTags.Length > 0)
        {
            Gizmos.color = Color.green;
            foreach (string tag in acceptableTags)
                UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"{tag}");
        }
    }
#endif
}
