using UnityEngine;

/*
 * ConsumableContext
 * -----------------
 * Context passed to all consumable effects when a consumable item is used.
 * - Provides the effect with all relevant information about the current state.
 * - Allows effects to modify the player, the board, or the movement system.
 * - Allows effects to know whether they were successfully used (WasUsed).
 */
public class ConsumableContext
{
    // Board information
    public int currentTileIndex;
    public int targetTileIndex;

    public Spot TargetSpot;
    public ColorSpot TargetColorSpot;

    public ItemSlot TargetSlot;

    // Player and movement references
    public Movement Player;
    public SpotController SpotCtrl;

    // Whether the item was successfully used
    public bool WasUsed = false;

    public ConsumableContext()
    {
        // Auto-fill common references
        Player = Object.FindAnyObjectByType<Movement>();
        SpotCtrl = SpotController.Instance;
    }
}
