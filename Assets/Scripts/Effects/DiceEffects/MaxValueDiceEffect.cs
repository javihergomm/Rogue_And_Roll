using UnityEngine;

[CreateAssetMenu(fileName = "MaxValueDiceEffect", menuName = "Effects/Dice/MaxValue")]
public class MaxValueDiceEffect : BaseDiceEffect
{
    [SerializeField]
    private int maxValue = 999;

    public override void ApplyToRange(ref int minAllowed, ref int maxAllowed, DiceContext ctx)
    {
        maxAllowed = Mathf.Min(maxAllowed, maxValue);
    }

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return Mathf.Min(roll, maxValue);
    }
}
