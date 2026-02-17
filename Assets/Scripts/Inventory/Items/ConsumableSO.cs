using UnityEngine;

/*
 * ConsumableSO
 * ------------
 * ScriptableObject representing a consumable item.
 * A consumable executes one or more effects when used.
 * Effects can modify dice, apply consumable logic, or trigger passive behaviors.
 */
[CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable")]
public class ConsumableSO : BaseItemSO
{
    public bool CanBeUsedOnSpot => canBeUsedOnSpot;
    public bool AppearsIn3D;
    public bool AutoUseOnPickup = false;

    [SerializeField] private bool canBeUsedOnSpot = false;

    [SerializeField] private BaseEffect[] effects;
    public BaseEffect[] Effects => effects;

    public override void UseItem()
    {
        // Generic use (for example, from a button)
        UseItem(new ConsumableContext());
        // Do NOT remove the item here. InventoryManager handles removal using the correct ItemSlot.
    }

    /*
     * Executes all effects assigned to this consumable.
     * Handles dice effects correctly:
     *  - Immediate effects if the player has not rolled yet
     *  - Deferred effects (next turn) if ApplyOnNextAvailableRoll is true
     */
    public void UseItem(ConsumableContext ctx)
    {
        if (effects == null || effects.Length == 0)
            return;

        foreach (var eff in effects)
        {
            if (eff == null)
                continue;

            // Dice-related effects
            if (eff is BaseDiceEffect diceEff)
            {
                diceEff.SourceItem = this;

                // If the effect must wait for the next roll
                if (diceEff.ApplyOnNextAvailableRoll &&
                    StatManager.Instance.HasPlayerRolledThisTurn)
                {
                    StatManager.Instance.PendingDiceEffects.Add(diceEff);
                }
                else
                {
                    // Apply immediately
                    StatManager.Instance.ActiveConsumableEffects.Add(diceEff);
                }

                ctx.WasUsed = true; // REQUIRED so InventoryManager removes the item
                continue;
            }

            // Consumable-specific effects
            if (eff is BaseConsumableEffect consEff)
            {
                consEff.Activate(ctx);
                continue;
            }

            // Passive effects triggered immediately
            if (eff is BasePassiveEffect passiveEff)
            {
                passiveEff.OnTurnStart(new PassiveContext());
                ctx.WasUsed = true;
                continue;
            }
        }
    }
}
