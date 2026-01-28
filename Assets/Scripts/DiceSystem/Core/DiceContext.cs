/*
 * DiceContext
 * -----------
 * Provides contextual information for dice roll effects.
 * Used by BaseDiceEffect and its subclasses to evaluate conditions
 * or modify the roll result based on game state.
 *
 * This class contains ONLY data. It has no logic.
 */
public class DiceContext
{
    // Turn information
    public int turnNumber;          // Current turn number
    public int previousRoll;        // Result of the previous dice roll

    // Slot that rolled the dice
    public ItemSlot slot;

    // HideRollEffect uses this to hide the roll in the UI
    public bool hideRollResult = false;

    // Optional: final roll after all effects (useful for chained effects)
    public int finalRoll;

    // Optional constructor for convenience
    public DiceContext() { }

    public DiceContext(int turn, int prevRoll, ItemSlot slot)
    {
        this.turnNumber = turn;
        this.previousRoll = prevRoll;
        this.slot = slot;
        this.finalRoll = prevRoll;
    }
}
