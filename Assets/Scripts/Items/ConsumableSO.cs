using UnityEngine;

/*
 * ConsumableSO
 * ------------
 * Consumables apply their own effects when used.
 * They do NOT remove themselves from inventory.
 * They do NOT modify stats directly.
 * InventoryManager handles removal after UseItem().
 */
[CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable")]
public class ConsumableSO : BaseItemSO
{
    [Header("Effects (Any Type)")]
    public BaseEffect[] effects;

    public override void UseItem()
    {
        if (effects == null || effects.Length == 0)
            return;

        ConsumableContext ctx = new ConsumableContext();

        foreach (var eff in effects)
        {
            if (eff == null)
                continue;

            // Temporary dice effects
            if (eff is BaseDiceEffect diceEff)
            {
                StatManager.Instance.ActiveConsumableEffects.Add(diceEff);
                continue;
            }

            // Immediate consumable effects (stat changes, healing, etc.)
            if (eff is BaseConsumableEffect consEff)
            {
                consEff.Activate(ctx);
                continue;
            }

            // Passive effects triggered immediately
            if (eff is BasePassiveEffect passiveEff)
            {
                passiveEff.OnTurnStart(new PassiveContext());
                continue;
            }
        }
    }
}
