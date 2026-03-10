using UnityEngine;

/*
 * BrokenMapEffect
 * ----------------
 * This effect hides both the dice roll result and the player piece.
 *
 * Behavior:
 * - If the player has NOT rolled yet this turn:
 *      The effect applies immediately on this turn's roll.
 *
 * - If the player HAS ALREADY rolled this turn:
 *      The effect is stored and will automatically apply
 *      on the next available roll (next turn).
 *
 * Additional notes:
 * - The effect lasts for exactly one roll.
 * - After the roll is resolved, the effect removes itself.
 */
[CreateAssetMenu(
    fileName = "BrokenMapEffect",
    menuName = "Effects/Dice/BrokenMap"
)]
public class BrokenMapEffect : BaseDiceEffect
{
    // This effect should be queued if the player already rolled this turn.
    public override bool ApplyOnNextAvailableRoll => true;

    // This effect lasts only for a single roll.
    public override bool RemoveAfterRoll => true;

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        // Hide the roll result in the UI
        StatManager.Instance.HideRollThisTurn = true;

        // Hide the player piece during movement
        StatManager.Instance.HidePieceThisTurn = true;

        return roll;
    }
}
