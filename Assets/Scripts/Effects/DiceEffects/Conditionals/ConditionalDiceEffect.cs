using UnityEngine;

/*
 * ConditionalDiceEffect
 * ---------------------
 * Base class for dice effects that only apply when a condition is met.
 * Subclasses define:
 *  - Condition(roll, ctx)
 *  - ApplyEffect(roll, ctx)
 *
 * If the condition is false, the roll is returned unchanged.
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
