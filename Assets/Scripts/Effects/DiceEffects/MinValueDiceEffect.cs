using UnityEngine;

/*
 * MinValueDiceEffect
 * ------------------
 * Ensures the roll result is never below a configurable minimum value.
 */
[CreateAssetMenu(fileName = "MinValueDiceEffect", menuName = "Effects/Dice/MinValue")]
public class MinValueDiceEffect : BaseDiceEffect
{
    [SerializeField]
    [Tooltip("Minimum allowed roll value.")]
    private int minValue = 1;

    // Public getter so DiceRollManager can read it
    public int MinValue
    {
        get { return minValue; }
    }

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return Mathf.Max(roll, minValue);
    }
}
