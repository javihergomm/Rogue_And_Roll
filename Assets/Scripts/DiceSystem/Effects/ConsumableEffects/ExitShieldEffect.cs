using UnityEngine;

/*
 * ExitShieldEffect
 * ----------------
 * Grants a shield that blocks the next bad spot the player steps on.
 * After blocking one bad spot, the shield is consumed automatically.
 *
 * Behavior:
 * - SpotController checks if the shield is active.
 * - If active and the player enters a bad spot:
 *      The negative effect is ignored and the shield is removed.
 */
[CreateAssetMenu(
    fileName = "ExitShieldEffect",
    menuName = "Effects/Consumables/ExitShield"
)]
public class ExitShieldEffect : BaseConsumableEffect
{
    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null || ctx.Player == null)
            return;

        SpotController ctrl = SpotController.Instance;

        // Enable shield mode: the next bad spot will be ignored
        ctrl.exitShieldActive = true;

        ctx.WasUsed = true;
    }
}
