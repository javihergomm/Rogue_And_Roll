using UnityEngine;

/*
 * BaseItemSO
 * ----------
 * Abstract parent for all item types (DiceSO, ConsumableSO, PermanentSO).
 * Holds shared properties like name, icon, description, and shop prices.
 * Each subclass must implement its own UseItem() behavior.
 */
public abstract class BaseItemSO : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string itemDescription;
    public GameObject prefab3D;

    [Header("Shop Settings")]
    public int buyPrice;
    public int sellPrice;

    // Force subclasses to define their own use logic
    public abstract void UseItem();
}
