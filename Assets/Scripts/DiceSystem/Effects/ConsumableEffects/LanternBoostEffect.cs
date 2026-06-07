using UnityEngine;

/*
 * LanternBoostEffect
 * ------------------
 * Enables a temporary boost that doubles the next positive spot effect.
 * SpotController handles the actual boost logic and resets it after use.
 */
[CreateAssetMenu(
    fileName = "LanternBoostEffect",
    menuName = "Effects/Consumables/LanternBoost"
)]
public class LanternBoostEffect : BaseConsumableEffect
{
    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null || ctx.Player == null)
        {
            ctx.WasUsed = false;
            return;
        }

        // Enable boost for the next positive spot
        SpotController.Instance.lanternBoostActive = true;

        ctx.WasUsed = true;
    }
}
