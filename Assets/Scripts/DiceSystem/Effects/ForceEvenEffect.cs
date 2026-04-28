using UnityEngine;

[CreateAssetMenu(
    fileName = "ForceEvenEffect",
    menuName = "Effects/Dice/ForceEven"
)]
public class ForceEvenEffect : BaseDiceEffect
{
    public override void ApplyToRange(ref int minAllowed, ref int maxAllowed, DiceContext ctx)
    {
        if (minAllowed % 2 != 0) minAllowed++;
        if (maxAllowed % 2 != 0) maxAllowed--;
    }

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        if (!ctx.IsFinal) return roll;
        return (roll % 2 == 0) ? roll : (roll + 1);
    }
}
