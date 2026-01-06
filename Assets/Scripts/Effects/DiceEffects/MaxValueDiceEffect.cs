using UnityEngine;

/*
 * MaxValueDiceEffect
 * ------------------
 * Ensures the roll result is never above a maximum value.
 * The maxValue is fully configurable in the inspector.
 */
[CreateAssetMenu(fileName = "MaxValueDiceEffect", menuName = "Effects/Dice/MaxValue")]
public class MaxValueDiceEffect : BaseDiceEffect
{
    public int maxValue = 999;

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return Mathf.Min(roll, maxValue);
    }
}
