using UnityEngine;

/*
 * ItemSO
 * ------
 * ScriptableObject representing an item definition.
 * Supports:
 * - Consumable
 * - Permanent
 * - Dice
 */
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public enum ItemType
    {
        Consumable,
        Permanent,
        Dice
    }

    public enum StatType
    {
        None,
        gold,
        rolls
    }

    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string itemDescription;
    public GameObject prefab3D;

    [Header("Item Type")]
    public ItemType itemType;

    [Header("Stat Effect (Consumables only)")]
    public StatType statToChange;
    public int amountToChangeStat;

    [Header("Dice Settings (Dice only)")]
    public int diceFaces = 6;

    [Header("Shop Settings")]
    public int buyPrice;
    public int sellPrice;

    public string GetStatDisplayName()
    {
        switch (statToChange)
        {
            case StatType.gold: return "Pesetas";
            case StatType.rolls: return "Tiradas";
            default: return "None";
        }
    }

    public int GetCurrentStatValue()
    {
        if (statToChange == StatType.None) return 0;
        return StatManager.Instance != null ? StatManager.Instance.GetCurrentValue(statToChange) : 0;
    }

    public int GetNewStatValue()
    {
        if (statToChange == StatType.None) return 0;
        return GetCurrentStatValue() + amountToChangeStat;
    }

    public void UseItem()
    {
        Debug.Log("[ItemSO] Using " + itemName);

        switch (itemType)
        {
            case ItemType.Consumable:
                HandleConsumable();
                break;

            case ItemType.Permanent:
                HandlePermanent();
                break;

            case ItemType.Dice:
                HandleDice();
                break;
        }
    }

    private void HandleConsumable()
    {
        if (statToChange == StatType.None)
        {
            Debug.Log(itemName + " does not affect stats.");
            return;
        }

        StatManager.Instance?.TryUseItem(this);
    }

    private void HandlePermanent()
    {
        Debug.Log(itemName + " is a permanent item. It is not consumed.");
    }

    private void HandleDice()
    {
        int result = Random.Range(1, diceFaces + 1);
        Debug.Log("Dice " + itemName + " rolled: " + result);
    }
}
