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
        Neutral,   
        Positive,  
        Negative,  
        Especial   
    }

    // Basic item information
    [Header("Basic Info")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField][TextArea] private string itemDescription;
    [SerializeField] private GameObject prefab3D;

    [Header("Unlockable ID")]
    public string itemID; // ASCII-only, único

    // Shop-related values
    [Header("Shop Settings")]
    [SerializeField] private int buyPrice;
    [SerializeField] private int sellPrice;

    // Loot classification
    [Header("Loot Settings")]
    [SerializeField] private ItemPolarity polarity;

    // Store display adjustments
    [Header("Store Display Overrides")]
    public Vector3 StoreRotationOffset = Vector3.zero;
    public float StoreHeightOffset = 0f;
    public float StoreScaleMultiplier = 1f;
    public float StoreZPositionOffset = 0f;
    public float StoreXPositionOffset = 0f;

    // Public accessors
    public string ItemName => itemName;
    public Sprite Icon => icon;
    public string Description => itemDescription;
    public GameObject Prefab3D => prefab3D;

    public int BuyPrice => buyPrice;
    public int SellPrice => sellPrice;

    public ItemPolarity Polarity => polarity;

    public abstract void UseItem();
}
