using UnityEngine;

/*
 * BaseItemSO
 * ----------
 * Base ScriptableObject for all item types.
 * Stores shared item data such as name, icon, description,
 * 3D prefab, prices, and polarity.
 * Child classes define how the item behaves when used.
 */
public abstract class BaseItemSO : ScriptableObject
{
    public enum ItemPolarity
    {
        Positive,
        Negative
    }

    // Basic item information
    [Header("Basic Info")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField][TextArea] private string itemDescription;
    [SerializeField] private GameObject prefab3D;

    // Shop-related values
    [Header("Shop Settings")]
    [SerializeField] private int buyPrice;
    [SerializeField] private int sellPrice;

    // Loot classification
    [Header("Loot Settings")]
    [SerializeField] private ItemPolarity polarity;

    // Public accessors
    public string ItemName => itemName;
    public Sprite Icon => icon;
    public string Description => itemDescription;
    public GameObject Prefab3D => prefab3D;

    public int BuyPrice => buyPrice;
    public int SellPrice => sellPrice;

    public ItemPolarity Polarity => polarity;

    /*
     * Called when the item is used.
     * Child classes implement their own behavior.
     */
    public abstract void UseItem();
}
