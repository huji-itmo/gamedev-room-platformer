using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [Header("Hotbar Settings")]
    [SerializeField] private int slotCount = 4;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private float previewScale = 0.15f;

    [Header("Selection Visuals")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new(0.5f, 0.5f, 0.5f, 1f);

    private GameObject[] _slotObjects;
    private GameObject[] _previewObjects;
    private Image[] _slotImages;
    private int _selectedIndex;

    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.renderMode = RenderMode.WorldSpace;

        _slotObjects = new GameObject[slotCount];
        _previewObjects = new GameObject[slotCount];
        _slotImages = new Image[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            _slotObjects[i] = Instantiate(slotPrefab, slotsContainer);
            _slotImages[i] = _slotObjects[i].GetComponent<Image>();
            _slotObjects[i].name = $"Slot_{i + 1}";
        }

        SelectSlot(0);
    }

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += Refresh;
            Refresh();
        }
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += Refresh;
            Refresh();
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
    }

    private void Update()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                SelectSlot(i);
                break;
            }
        }
    }

    public void Refresh()
    {
        Debug.Log("[HotbarUI] Refresh called");
        if (InventoryManager.Instance == null) { Debug.LogWarning("[HotbarUI] InventoryManager.Instance is null"); return; }

        var items = InventoryManager.Instance.Items;
        Debug.Log($"[HotbarUI] Inventory has {items.Count} items");
        for (int i = 0; i < slotCount; i++)
        {
            if (i < items.Count && items[i] != null)
                SetSlotPreview(i, items[i]);
            else
                ClearSlotPreview(i);
        }
    }

    private void SetSlotPreview(int index, ItemSO item)
    {
        ClearSlotPreview(index);

        if (item.itemPrefab == null)
        {
            Debug.LogWarning($"[HotbarUI] Slot {index}: '{item.itemName}' has no itemPrefab assigned!");
            return;
        }

        Debug.Log($"[HotbarUI] Slot {index}: instantiating preview for '{item.itemName}'");
        GameObject preview = Instantiate(item.itemPrefab, _slotObjects[index].transform);
        preview.name = $"Preview_{item.itemName}";
        preview.transform.localPosition = Vector3.zero;
        preview.transform.localRotation = Quaternion.identity;
        preview.transform.localScale = Vector3.one * previewScale * item.hotbarPreviewScale;

        foreach (Collider col in preview.GetComponentsInChildren<Collider>(true)) col.enabled = false;
        foreach (Rigidbody rb in preview.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;
        foreach (MonoBehaviour mb in preview.GetComponentsInChildren<MonoBehaviour>(true)) mb.enabled = false;

        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            preview.transform.localPosition = new Vector3(0, 0, -(bounds.extents.z + 0.005f));
        }

        _previewObjects[index] = preview;
    }

    private void ClearSlotPreview(int index)
    {
        if (_previewObjects[index] != null)
        {
            Destroy(_previewObjects[index]);
            _previewObjects[index] = null;
        }
    }

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;

        if (_selectedIndex >= 0 && _selectedIndex < slotCount && _slotImages[_selectedIndex] != null)
            _slotImages[_selectedIndex].color = unselectedColor;

        _selectedIndex = index;

        if (_slotImages[_selectedIndex] != null)
            _slotImages[_selectedIndex].color = selectedColor;

    }
}
