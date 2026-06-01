using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class InventoryUI : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionReference toggleInventoryAction;

    [Header("Animation Settings")]
    [SerializeField] private string animatorParamName = "UIState";
    [SerializeField] private int stateHidden = 0;
    [SerializeField] private int stateActive = 1;

    [Header("UI References")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("Preview Settings")]
    [Tooltip("Scale multiplier applied to instantiated prefab previews")]
    [SerializeField] private float previewScaleMultiplier = 0.15f;
    [Tooltip("Optional: Layer to assign previews to (prevents raycast interference)")]
    [SerializeField] private LayerMask previewLayer;

    private Animator _animator;
    private bool _isOpen;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (toggleInventoryAction == null || toggleInventoryAction.action == null)
        {
            Debug.LogError($"[InventoryUI] Toggle Inventory InputActionReference not assigned!", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        toggleInventoryAction.action.Enable();
        toggleInventoryAction.action.performed += OnToggleInput;
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
    }

    private void OnDisable()
    {
        toggleInventoryAction.action.performed -= OnToggleInput;
        toggleInventoryAction.action.Disable();
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
    }

    private void OnToggleInput(InputAction.CallbackContext context) => ToggleInventory();

    public void ToggleInventory()
    {
        _isOpen = !_isOpen;
        _animator.SetInteger(animatorParamName, _isOpen ? stateActive : stateHidden);
        if (_isOpen) RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in itemsContainer)
            Destroy(child.gameObject);

        if (InventoryManager.Instance == null) return;

        foreach (ItemSO item in InventoryManager.Instance.Items)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemsContainer);

            if (item.itemPrefab != null)
            {
                GameObject preview = Instantiate(item.itemPrefab, slot.transform);
                preview.name = $"Preview_{item.itemName}";

                preview.transform.localPosition = Vector3.zero;
                preview.transform.localRotation = Quaternion.identity;
                preview.transform.localScale = Vector3.one * previewScaleMultiplier;

                if (previewLayer != 0) preview.layer = previewLayer;

                foreach (Collider col in preview.GetComponentsInChildren<Collider>(true)) col.enabled = false;
                foreach (Rigidbody rb in preview.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;

                foreach (MonoBehaviour mb in preview.GetComponentsInChildren<MonoBehaviour>(true)) mb.enabled = false;
            }

            var textLabel = slot.GetComponent<UnityEngine.UI.Text>();
            if (textLabel != null) textLabel.text = item.itemName;
        }
    }
}
