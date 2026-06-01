using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public List<ItemSO> Items => _items;
    public event Action<ItemSO> OnItemAdded;
    public event Action<ItemSO> OnItemRemoved;
    public event Action OnInventoryChanged;

    private List<ItemSO> _items = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool AddItem(ItemSO item)
    {
        if (item == null) { Debug.LogError("[InventoryManager] AddItem called with null item"); return false; }

        _items.Add(item);
        Debug.Log($"[InventoryManager] Added '{item.itemName}', total items: {_items.Count}");
        OnItemAdded?.Invoke(item);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemSO item)
    {
        if (item == null) { Debug.LogError("[InventoryManager] RemoveItem called with null item"); return false; }
        if (!_items.Contains(item)) { Debug.LogWarning($"[InventoryManager] RemoveItem: '{item.itemName}' not in inventory"); return false; }

        _items.Remove(item);
        Debug.Log($"[InventoryManager] Removed '{item.itemName}', total items: {_items.Count}");
        OnItemRemoved?.Invoke(item);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(ItemSO item)
    {
        bool result = item != null && _items.Contains(item);
        if (item != null) Debug.Log($"[InventoryManager] HasItem('{item.itemName}') = {result}");
        return result;
    }

    public bool ConsumeItem(ItemSO item)
    {
        if (!HasItem(item))
        {
            Debug.LogWarning($"[InventoryManager] ConsumeItem: '{item?.itemName}' not in inventory");
            return false;
        }

        Debug.Log($"[InventoryManager] ConsumeItem: '{item.itemName}', isConsumable={item.isConsumable}");
        if (item.isConsumable)
        {
            bool removed = RemoveItem(item);
            Debug.Log($"[InventoryManager] Consumable '{item.itemName}' removed={removed}");
            return removed;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public ItemSO FindItemMatchingTags(string[] targetTags)
    {
        if (targetTags == null || targetTags.Length == 0)
        {
            Debug.LogWarning("[InventoryManager] FindItemMatchingTags called with null/empty targetTags");
            return null;
        }

        Debug.Log($"[InventoryManager] FindItemMatchingTags: searching {_items.Count} items for tags [{string.Join(", ", targetTags)}]");
        foreach (var item in _items)
        {
            bool matches = item.CanBeUsedOn(targetTags);
            Debug.Log($"[InventoryManager]   Checking '{item.itemName}' against tags: {matches}");
            if (matches)
                return item;
        }
        Debug.Log($"[InventoryManager] No matching item found for tags [{string.Join(", ", targetTags)}]");
        return null;
    }

    public void ClearInventory()
    {
        _items.Clear();
        OnInventoryChanged?.Invoke();
    }
}
