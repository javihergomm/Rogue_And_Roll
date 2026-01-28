using UnityEngine;

/*
 * BaseItemSO
 * ----------
 * Abstract parent for all item types.
 * Stores shared data: name, icon, description, prices, polarity.
 */
public abstract class BaseItemSO : ScriptableObject
{
    public enum ItemPolarity
    {
        Positive,
        Negative
    }

    [Header("Basic Info")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField] private string itemDescription;
    [SerializeField] private GameObject prefab3D;

    [Header("Shop Settings")]
    [SerializeField] private int buyPrice;
    [SerializeField] private int sellPrice;

    [Header("Loot Settings")]
    [SerializeField] private ItemPolarity polarity;

    public string ItemName => itemName;
    public Sprite Icon => icon;
    public string Description => itemDescription;
    public GameObject Prefab3D => prefab3D;

    public int BuyPrice => buyPrice;
    public int SellPrice => sellPrice;

    public ItemPolarity Polarity => polarity;

    public abstract void UseItem();
}
