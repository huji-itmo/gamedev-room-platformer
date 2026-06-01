using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public bool isConsumable;
    
    [Header("Visuals")]
    [Tooltip("Complete prefab for inventory preview & world placement")]
    public GameObject itemPrefab;
    [Tooltip("Scale multiplier for the hotbar preview (relative to HotbarUI's previewScale)")]
    public float hotbarPreviewScale = 1f;
    
    [Header("Usage Rules")]
    [Tooltip("Case-insensitive tags this item can be used on (e.g., 'Main Door', 'Chest')")]
    public string[] usableOnTags;
    
    public bool CanBeUsedOn(string[] targetTags)
    {
        if (usableOnTags == null || usableOnTags.Length == 0 || targetTags == null || targetTags.Length == 0) 
            return false;
        
        foreach (string itemTag in usableOnTags)
        {
            foreach (string targetTag in targetTags)
            {
                if (itemTag.Equals(targetTag, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }
}