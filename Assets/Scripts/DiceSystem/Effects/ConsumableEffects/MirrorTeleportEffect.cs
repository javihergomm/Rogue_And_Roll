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
        // Encontrar al jugador real
        Movement player = null;
        foreach (var m in Object.FindObjectsByType<Movement>(FindObjectsSortMode.None))
        {
            if (m.isPlayer)
            {
                player = m;
                break;
            }
        }
        if (player == null)
            return;

        SpotController controller = Object.FindFirstObjectByType<SpotController>();
        if (controller == null)
            return;

        Spot[] spots = controller.GetSpotsOrdered();
        int playerPos = player.ActualPos;

        // 1. Buscar casilla Good hacia delante
        Spot positive = FindNextPositiveSpot(spots, playerPos);

        if (positive != null)
        {
            TeleportAndTrigger(player, positive);
            ctx.WasUsed = true;
            return;
        }

        // 2. Si no hay Good, buscar tienda mas cercana
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
        // Teletransporte
        player.TeleportToPosition(target.index);

        // IMPORTANTE: activar efecto real de la casilla
        player.StartCoroutine(TriggerAfterTeleport(player, target));
    }

    private IEnumerator TriggerAfterTeleport(Movement player, Spot target)
    {
        // Esperar un frame para que el teletransporte se aplique
        yield return null;

        // Activar efecto real de la casilla
        yield return player.StartCoroutine(target.TriggerSpotEffect(player));

        // Notificar fin de movimiento
        player.OnMovementFinished?.Invoke();
    }
}
