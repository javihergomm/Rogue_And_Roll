using UnityEngine;

[CreateAssetMenu(fileName = "HideRollEffect", menuName = "Effects/Dice/HideRoll")]
public class HideRollEffect : BaseDiceEffect
{
    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        ctx.hideRollResult = true;
        return roll;
    }
}

