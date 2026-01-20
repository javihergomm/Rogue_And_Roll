using UnityEngine;

/*
 * ConsumableSO
 * ------------
 * ScriptableObject representing a consumable item.
 * Consumables are removed from the inventory when used.
 * They may modify player stats or apply a temporary dice effect.
 */
[CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable")]
public class ConsumableSO : BaseItemSO
{
    [Header("Stat Change")]
    public StatType statToChange;       // Optional stat to modify
    public int amountToChangeStat;      // Amount applied to the stat

    [Header("Effects (Any Type)")]
    public BaseEffect[] effects;

    // Consumables are used through StatManager and InventoryManager
    public override void UseItem() { }
}
