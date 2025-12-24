using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable")]
public class ConsumableSO : BaseItemSO
{
    [Header("Consumable Settings")]
    public StatType statToChange;
    public int amountToChangeStat;

    // Consumables are used through InventoryManager, not directly here
    public override void UseItem() { }
}
