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

    // Store display adjustments
    [Header("Store Display Overrides")]
    [Tooltip("Extra rotation applied ONLY in the shop display.")]
    public Vector3 StoreRotationOffset = Vector3.zero;

    [Tooltip("Extra height offset applied ONLY in the shop display.")]
    public float StoreHeightOffset = 0f;

    [Tooltip("Scale multiplier applied ONLY in the shop display.")]
    public float StoreScaleMultiplier = 1f;

    [Tooltip("Extra Z offset applied ONLY in the shop display.")]
    public float StoreZPositionOffset = 0f;

    [Tooltip("Extra X offset applied ONLY in the shop display.")]
    public float StoreXPositionOffset = 0f;

    // Public accessors
    public string ItemName => itemName;
    public Sprite Icon => icon;
    public string Description => itemDescription;
    public GameObject Prefab3D => prefab3D;

    public int BuyPrice => buyPrice;
    public int SellPrice => sellPrice;

    public bool CanBeDiscarded = true;

    // Controls whether the item is consumed immediately when used
    public bool ConsumeOnUse = true;

    public ItemPolarity Polarity => polarity;

    public abstract void UseItem();
}
