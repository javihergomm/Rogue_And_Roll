using UnityEngine;

/*
 * ForceEvenRollEffect
 * -------------------
 * Forces the final roll result to be an even number.
 *
 * Behavior:
 * - If the roll is already even, it stays the same.
 * - If the roll is odd, it becomes the nearest even number.
 *   Example: 7 -> 8, 5 -> 6
 *
 * This effect does NOT restrict allowed faces.
 * It only modifies the final roll result.
 */
[CreateAssetMenu(fileName = "ForceEvenRollEffect", menuName = "Effects/Dice/ForceEven")]
public class ForceEvenRollEffect : BaseDiceEffect
{
    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        // If already even, return as is
        if (roll % 2 == 0)
            return roll;

        // If odd, round to nearest even
        // Example: 7 -> 8, 5 -> 6
        int lower = roll - 1;
        int upper = roll + 1;

        // Choose the closest even number
        int diffLower = roll - lower;
        int diffUpper = upper - roll;

        if (diffLower <= diffUpper)
            return lower;

        return upper;
    }
}
