/*
 * DiceContext
 * -----------
 * Provides contextual information for dice roll effects.
 * Used by BaseDiceEffect and its subclasses to evaluate conditions
 * or modify the roll result based on game state.
 */
public class DiceContext
{
    public int turnNumber;          // Current turn number
    public int previousRoll;        // Result of the previous dice roll
    public ItemSlot slot;           // Slot that rolled the dice

    // HideRollEffect uses this to hide the roll in the UI
    public bool hideRollResult = false;

    // Optional: final roll after all effects (useful for chained effects)
    public int finalRoll;
}
