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
        {
            ctx.WasUsed = false;
            return;
        }

        // Activar boost para la siguiente casilla buena
        SpotController.Instance.lanternBoostActive = true;

        ctx.WasUsed = true;
    }
}
