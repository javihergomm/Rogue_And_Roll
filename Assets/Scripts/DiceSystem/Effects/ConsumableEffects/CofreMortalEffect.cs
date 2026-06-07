using UnityEngine;

[CreateAssetMenu(menuName = "Effects/CofreMortalEffect")]
public class CofreMortalEffect : BaseConsumableEffect
{
    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null || ctx.Player == null)
            return;

        int startIndex = ctx.Player.ActualPos;

        // Search forward until no more spots exist
        for (int i = startIndex + 1; ; i++)
        {
            Spot spot = SpotController.Instance.GetSpotByIndex(i);

            if (spot == null)
                break; // No more spots ahead

            // Convert the first normal spot into a bad spot
            if (spot.GetSpotType() == Spot.SpotType.Normal)
            {
                spot.AssignType(Spot.SpotType.Bad);
                ctx.WasUsed = true;
                return;
            }
        }

        // No normal spot found ahead
        ctx.WasUsed = false;
    }
}
