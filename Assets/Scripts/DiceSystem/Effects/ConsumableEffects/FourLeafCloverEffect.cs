using UnityEngine;

/*
 * FourLeafCloverEffect
 * --------------------
 * Forces the next BAD spot the player steps on to become a positive LuckBox.
 *
 * Behavior:
 * - Saves the original BAD spot probabilities.
 * - Forces all BAD outcomes to 0 except the positive LuckBox.
 * - Activates a "clover mode" in SpotController.
 * - SpotController is responsible for restoring probabilities
 *   after the next BAD spot is triggered.
 *
 * This effect does NOT restore probabilities itself.
 * Restoration must happen inside SpotController when the BAD spot is consumed.
 */
[CreateAssetMenu(
    fileName = "FourLeafCloverEffect",
    menuName = "Effects/Consumables/FourLeafClover"
)]
public class FourLeafCloverEffect : BaseConsumableEffect
{
    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null)
            return;

        SpotController ctrl = SpotController.Instance;
        if (ctrl == null)
        {
            Debug.LogError("SpotController.Instance is NULL.");
            ctx.WasUsed = false;
            return;
        }

        // Enable clover mode
        ctrl.cloverActive = true;

        // Save original BAD spot probabilities
        ctrl.savedBadSteps = ctrl.probBadNegativeSteps;
        ctrl.savedBadBlock = ctrl.probBadBlockPlayer;
        ctrl.savedBadLoot = ctrl.probBadLootBox;

        // Force the next BAD spot to become a positive LuckBox
        ctrl.probBadNegativeSteps = 0;
        ctrl.probBadBlockPlayer = 0;
        ctrl.probBadLootBox = 100;

        // Optional feedback to the player
        if (ctx.Player != null)
        {
            ctx.Player.lastSpotEffectText =
                "Trébol activado: la próxima casilla mala se convertirá en una LuckBox positiva."
;
        }

        // Mark consumable as used so InventoryManager removes it
        ctx.WasUsed = true;
    }
}
