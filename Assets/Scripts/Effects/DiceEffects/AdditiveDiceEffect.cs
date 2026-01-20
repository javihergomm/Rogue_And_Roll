using UnityEngine;

/*
 * AdditiveDiceEffect
 * ------------------
 * Adds or subtracts a fixed value from the roll result.
 * This effect applies to all dice types and all situations.
 *
 * Example:
 *  - Roll 5 -> becomes 6
 *  - Roll 10 -> becomes 12 (if amount = 2)
 *
 * This effect does NOT restrict allowed faces.
 * It only modifies the final roll result.
 */
[CreateAssetMenu(fileName = "AdditiveDiceEffect", menuName = "Effects/Dice/Additive")]
public class AdditiveDiceEffect : BaseDiceEffect
{
    [SerializeField]
    [Tooltip("Flat value added to the roll. Can be negative.")]
    private int amount = 1;

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return roll + amount;
    }
}
