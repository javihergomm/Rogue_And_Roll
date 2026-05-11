using UnityEngine;

/*
 * ConsumableContext
 * -----------------
 * Context passed to all consumable effects when a consumable item is used.
 *
 * Purpose:
 * - Provides the effect with all relevant information about the current state.
 * - Allows effects to modify the player, the board, or the movement system.
 * - Allows effects to know whether they were successfully used (WasUsed).
 *
 * Notes:
 * - InventoryManager removes the item ONLY if WasUsed = true.
 * - TargetSpot and TargetColorSpot are only set when the item is used on a tile.
 * - Player and Movement references are optional but recommended.
 */
public class ConsumableContext
{
    // Board information
    public int currentTileIndex;       // Player's current tile index
    public int targetTileIndex;        // Tile index the item is used on (if any)

    public Spot TargetSpot;            // Generic spot reference
    public ColorSpot TargetColorSpot;  // Specific ColorSpot reference (if required)

    public ItemSlot TargetSlot;

    // Player and movement references
    public Movement Player;            // Player movement component
    public SpotController SpotCtrl;    // Cached SpotController reference

    // Whether the item was successfully used
    // InventoryManager removes the item only if this is true
    public bool WasUsed = false;

    public ConsumableContext()
    {
        // Auto-fill common references
        Player = Object.FindFirstObjectByType<Movement>();
        SpotCtrl = SpotController.Instance;
    }
}
