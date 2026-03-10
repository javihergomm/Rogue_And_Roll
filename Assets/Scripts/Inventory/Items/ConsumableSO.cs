using UnityEngine;

/*
 * ConsumableSO
 * ------------
 * ScriptableObject representing a consumable item.
 * A consumable executes one or more effects when used.
 *
 * Behavior:
 * - Dice effects may apply immediately or be deferred to the next roll.
 * - Consumable effects execute custom logic through BaseConsumableEffect.
 * - Passive effects must be activated (Activate()) so they register
 *   themselves and manage their own turn-based duration.
 *
 * InventoryManager handles removing the item after use.
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
        // Generic use (e.g., from a button)
        UseItem(new ConsumableContext());
        // Do NOT remove the item here. InventoryManager handles removal.
    }

    /*
     * Executes all effects assigned to this consumable.
     * Dice effects:
     *  - Apply immediately if the player has not rolled yet.
     *  - Otherwise, they are queued for the next available roll.
     *
     * Consumable effects:
     *  - Execute custom logic through BaseConsumableEffect.Activate().
     *
     * Passive effects:
     *  - Must call Activate() so they register themselves and manage
     *    their own multi-turn behavior.
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

                ctx.WasUsed = true; // Required so InventoryManager removes the item
                continue;
            }

            // Consumable-specific effects
            if (eff is BaseConsumableEffect consEff)
            {
                consEff.Activate(ctx);
                continue;
            }

            // Passive effects (must be activated properly)
            if (eff is BasePassiveEffect passiveEff)
            {
                passiveEff.Activate();   
                ctx.WasUsed = true;
                continue;
            }
        }
    }
}
