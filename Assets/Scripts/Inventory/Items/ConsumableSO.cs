using UnityEngine;

/*
 * ConsumableSO
 * ------------
 * Applies effects when used.
 * InventoryManager removes the item after UseItem().
 */
[CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable")]
public class ConsumableSO : BaseItemSO
{
    [Header("Effects")]
    [SerializeField] private BaseEffect[] effects;

    public BaseEffect[] Effects => effects;

    public override void UseItem()
    {
        if (effects == null || effects.Length == 0)
            return;

        ConsumableContext ctx = new ConsumableContext();

        foreach (var eff in effects)
        {
            if (eff == null)
                continue;

            if (eff is BaseDiceEffect diceEff)
            {
                StatManager.Instance.ActiveConsumableEffects.Add(diceEff);
                continue;
            }

            if (eff is BaseConsumableEffect consEff)
            {
                consEff.Activate(ctx);
                continue;
            }

            if (eff is BasePassiveEffect passiveEff)
            {
                passiveEff.OnTurnStart(new PassiveContext());
                continue;
            }
        }
    }
}
