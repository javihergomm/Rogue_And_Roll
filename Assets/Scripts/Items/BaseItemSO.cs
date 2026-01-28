using UnityEngine;

/*
 * BaseItemSO
 * ----------
 * Abstract parent for all item types (DiceSO, ConsumableSO, PermanentSO).
 * Holds shared properties like name, icon, description, and shop prices.
 * Each item also defines its polarity (Positive or Negative), which is
 * used by loot boxes to validate whether the item can be included.
 */
public abstract class BaseItemSO : ScriptableObject
{
    public enum ItemPolarity
    {
        Positive,
        Negative
    }

    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string itemDescription;
    public GameObject prefab3D;

    [Header("Shop Settings")]
    public int buyPrice;
    public int sellPrice;

    [Header("Loot Settings")]
    [Tooltip("Determines whether this item is considered positive or negative.")]
    public ItemPolarity polarity;

    // Force subclasses to define their own use logic
    public abstract void UseItem();
}
