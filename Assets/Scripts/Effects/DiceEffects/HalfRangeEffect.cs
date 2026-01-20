using UnityEngine;

/*
 * HalfRangeEffect
 * ----------------
 * Reduces the allowed face range of the dice to half of its maximum value.
 *
 * IMPORTANT:
 * - This effect ONLY restricts the allowed faces.
 * - The final roll CAN be modified by later effects (bonus, multipliers, etc.).
 * - But those effects only apply to results that originate from allowed faces.
 *
 * Example:
 *  - D20 -> allowed faces become 1-10
 *  - D12 -> allowed faces become 1-6
 *  - D6  -> allowed faces become 1-3
 *
 * This effect does NOT modify the final roll result.
 * DiceRollManager must call GetMaxAllowed() when applying range restrictions.
 */
[CreateAssetMenu(fileName = "HalfRangeEffect", menuName = "Effects/Dice/HalfRange")]
public class HalfRangeEffect : BaseDiceEffect
{
    [SerializeField]
    [Tooltip("If true, rounds up the half-range (e.g., D5 -> 1-3).")]
    private bool roundUp = true;

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        // This effect does NOT modify the final roll.
        // It only affects allowed faces.
        return roll;
    }

    // Called by DiceRollManager when applying range restrictions
    public int GetMaxAllowed(int originalMax)
    {
        if (roundUp)
            return Mathf.CeilToInt(originalMax / 2f);

        return originalMax / 2;
    }
}
