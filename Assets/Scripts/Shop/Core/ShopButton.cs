using UnityEngine;
using System.Collections.Generic;

/*
 * ShopButton
 * ----------
 * UI button script that allows the player to manually enter the shop.
 * If the player is not standing on a checkpoint, a popup will show
 * listing all checkpoint positions.
 */
public class ShopButton : MonoBehaviour
{
    public void TryEnterShop()
    {
        // Find player movement
        Movement player = FindAnyObjectByType<Movement>(FindObjectsInactive.Include);

        if (player == null)
        {
            return;
        }

        // If not on a checkpoint, show popup with checkpoint list
        if (!player.IsOnCheckpoint())
        {
            List<int> checkpoints = new List<int>();

            // Extract checkpoint indices from Movement.spots
            foreach (var spot in player.GetSpots())
            {
                if (spot.checkpoint)
                    checkpoints.Add(spot.index);
            }

            PopupHelpers.ShowNotOnCheckpointPopup(checkpoints);
            return;
        }
        player.pausedByShop = true;
        // Enter shop
        ShopExitManager shop = FindAnyObjectByType<ShopExitManager>();

        if (shop == null)
        {
            return;
        }

        shop.EnterShop();
    }
}
