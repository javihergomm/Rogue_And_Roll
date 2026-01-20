using UnityEngine;

/*
 * MaxValueDiceEffect
 * ------------------
 * Restricts the maximum allowed roll value.
 * Does NOT define the initial max; DiceRollManager provides it.
 */
[CreateAssetMenu(fileName = "MaxValueDiceEffect", menuName = "Effects/Dice/MaxValue")]
public class MaxValueDiceEffect : BaseDiceEffect
{
    [SerializeField]
    [Tooltip("Maximum allowed roll value imposed by this effect.")]
    private int maxValue = 999;

    public int MaxValue
    {
        get { return maxValue; }
    }

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return Mathf.Min(roll, maxValue);
    }
}
