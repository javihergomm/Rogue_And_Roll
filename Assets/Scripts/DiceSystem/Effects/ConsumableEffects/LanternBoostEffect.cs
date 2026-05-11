using UnityEngine;

[CreateAssetMenu(
    fileName = "LanternBoostEffect",
    menuName = "Effects/Consumables/LanternBoost"
)]
public class LanternBoostEffect : BaseConsumableEffect
{
    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null || ctx.Player == null)
            return;

        SpotController.Instance.lanternBoostActive = true;
        ctx.WasUsed = true;
    }
}
