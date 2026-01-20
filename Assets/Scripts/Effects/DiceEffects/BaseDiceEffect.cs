using UnityEngine;

public abstract class BaseDiceEffect : BaseEffect
{
    // Modify the roll result before returning it
    public abstract int ModifyRoll(int roll, DiceContext ctx);
}
