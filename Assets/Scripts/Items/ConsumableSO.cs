using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable")]
public class ConsumableSO : BaseItemSO
{
    [Header("Consumable Settings")]
    public StatType statToChange;
    public int amountToChangeStat;

    public override void UseItem()
    {
        Debug.Log("[Consumable] Using " + itemName);

        if (StatManager.Instance != null)
        {
            StatManager.Instance.TryUseItem(this);
        }
    }
}
