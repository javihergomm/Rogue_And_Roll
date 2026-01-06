using UnityEngine;

/*
 * MinValueDiceEffect
 * ------------------
 * Ensures the roll result is never below a minimum value.
 */
[CreateAssetMenu(fileName = "MinValueDiceEffect", menuName = "Effects/Dice/MinValue")]
public class MinValueDiceEffect : BaseDiceEffect
{
    public int minValue = 1;

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return Mathf.Max(roll, minValue);
    }
}
