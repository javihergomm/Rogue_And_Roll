using UnityEngine;

/*
 * MathDiceEffect
 * --------------
 * Unified dice effect for all mathematical roll/range modifications.
 * Covers:
 *  - Additive (+/-)
 *  - Multiplier
 *  - MinClamp
 *  - MaxClamp
 *  - HalfRange
 *  - ForceEven
 *  - RandomMultiplier (Potion of Chance)
 *
 * This replaces multiple separate effect classes.
 */
public enum MathOperation
{
    Add,
    Multiply,
    MinClamp,
    MaxClamp,
    HalfRange,
    ForceEven,
    RandomMultiplier
}

[CreateAssetMenu(fileName = "MathDiceEffect", menuName = "Effects/Dice/Math")]
public class MathDiceEffect : BaseDiceEffect
{
    public MathOperation operation;

    // General numeric parameter
    public float value = 1f;

    // For HalfRange
    public bool roundUp = true;

    // For RandomMultiplier (Potion of Chance)
    public Vector2Int randomMultiplierRange = new Vector2Int(2, 5);

    public override void ApplyToRange(ref int minAllowed, ref int maxAllowed, DiceContext ctx)
    {
        switch (operation)
        {
            case MathOperation.MinClamp:
                minAllowed = Mathf.Max(minAllowed, Mathf.RoundToInt(value));
                break;

            case MathOperation.MaxClamp:
                maxAllowed = Mathf.Min(maxAllowed, Mathf.RoundToInt(value));
                break;

            case MathOperation.HalfRange:
                int half = roundUp
                    ? Mathf.CeilToInt(maxAllowed / 2f)
                    : maxAllowed / 2;
                maxAllowed = Mathf.Min(maxAllowed, half);
                break;

            case MathOperation.ForceEven:
                if (minAllowed % 2 != 0) minAllowed++;
                if (maxAllowed % 2 != 0) maxAllowed--;
                break;
        }
    }

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        switch (operation)
        {
            case MathOperation.Add:
                return roll + Mathf.RoundToInt(value);

            case MathOperation.Multiply:
                return Mathf.RoundToInt(roll * value);

            case MathOperation.MinClamp:
                return Mathf.Max(roll, Mathf.RoundToInt(value));

            case MathOperation.MaxClamp:
                return Mathf.Min(roll, Mathf.RoundToInt(value));

            case MathOperation.HalfRange:
                return roll;

            case MathOperation.ForceEven:
                if (roll % 2 == 0)
                    return roll;
                int lower = roll - 1;
                int upper = roll + 1;
                return (roll - lower <= upper - roll) ? lower : upper;

            case MathOperation.RandomMultiplier:
                int mult = Random.Range(randomMultiplierRange.x, randomMultiplierRange.y + 1);
                return roll * mult;
        }

        return roll;
    }
}
