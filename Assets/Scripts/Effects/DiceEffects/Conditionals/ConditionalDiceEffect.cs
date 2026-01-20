using UnityEngine;

/*
 * ConditionalDiceEffect
 * ---------------------
 * Base class for effects that only apply when a condition is met.
 * Subclasses define the condition and the effect logic.
 */
public abstract class ConditionalDiceEffect : BaseDiceEffect
{
    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        if (Condition(roll, ctx))
            return ApplyEffect(roll, ctx);

        return roll;
    }

    protected abstract bool Condition(int roll, DiceContext ctx);
    protected abstract int ApplyEffect(int roll, DiceContext ctx);
}
