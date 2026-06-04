using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MirrorTeleportEffect",
    menuName = "Effects/Consumables/MirrorTeleport"
)]
public class MirrorTeleportEffect : BaseConsumableEffect
{
    public override void Activate(ConsumableContext ctx)
    {
        // Find the real player Movement component
        Movement player = null;

        // Search active Movement components
        foreach (var m in Object.FindObjectsByType<Movement>(FindObjectsInactive.Exclude))
        {
            if (m.isPlayer)
            {
                player = m;
                break;
            }
        }

        if (player == null)
            return;

        // Find the SpotController instance
        SpotController controller = Object.FindAnyObjectByType<SpotController>();
        if (controller == null)
            return;

        Spot[] spots = controller.GetSpotsOrdered();
        int playerPos = player.ActualPos;

        // 1. Search for the next Good spot ahead
        Spot positive = FindNextPositiveSpot(spots, playerPos);

        if (positive != null)
        {
            TeleportAndTrigger(player, positive);
            ctx.WasUsed = true;
            return;
        }

        // 2. If no Good spot exists, search for the nearest shop checkpoint
        Spot shop = FindNearestShopSpot(spots, playerPos);

        if (shop != null)
        {
            TeleportAndTrigger(player, shop);
            ctx.WasUsed = true;
            return;
        }
    }

    private Spot FindNextPositiveSpot(Spot[] spots, int startIndex)
    {
        int count = spots.Length;

        // Searches forward in circular order
        for (int i = 1; i < count; i++)
        {
            int idx = (startIndex - 1 + i) % count;

            if (spots[idx].type == Spot.SpotType.Good)
                return spots[idx];
        }

        return null;
    }

    private Spot FindNearestShopSpot(Spot[] spots, int startIndex)
    {
        int count = spots.Length;

        // Searches forward in circular order for a checkpoint
        for (int i = 1; i < count; i++)
        {
            int idx = (startIndex - 1 + i) % count;

            if (spots[idx].checkpoint)
                return spots[idx];
        }

        return null;
    }

    private void TeleportAndTrigger(Movement player, Spot target)
    {
        // Teleport the player to the target tile
        player.TeleportToPosition(target.index);

        // Trigger the tile effect after teleporting
        player.StartCoroutine(TriggerAfterTeleport(player, target));
    }

    private IEnumerator TriggerAfterTeleport(Movement player, Spot target)
    {
        // Wait one frame to ensure teleportation is applied
        yield return null;

        // Execute the tile's effect
        yield return player.StartCoroutine(target.TriggerSpotEffect(player));

        // Notify movement completion
        player.OnMovementFinished?.Invoke();
    }
}
