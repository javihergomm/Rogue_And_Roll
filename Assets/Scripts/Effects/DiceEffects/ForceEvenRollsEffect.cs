using UnityEngine;

[CreateAssetMenu(fileName = "ForceEvenRollEffect", menuName = "Effects/Dice/ForceEven")]
public class ForceEvenRollEffect : BaseDiceEffect
{
    public override void ApplyToRange(ref int minAllowed, ref int maxAllowed, DiceContext ctx)
    {
        if (minAllowed % 2 != 0) minAllowed++;
        if (maxAllowed % 2 != 0) maxAllowed--;
    }

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        if (roll % 2 == 0)
            return roll;

        int lower = roll - 1;
        int upper = roll + 1;

        int diffLower = roll - lower;
        int diffUpper = upper - roll;

        return (diffLower <= diffUpper) ? lower : upper;
    }
}
