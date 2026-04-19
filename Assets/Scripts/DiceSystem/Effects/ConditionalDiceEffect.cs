using UnityEngine;

/*
 * ConditionalDiceEffect
 * ---------------------
 * Base class for effects that only apply when a condition is met.
 * Subclasses implement:
 *  - Condition(roll, ctx)
 *  - ApplyEffect(roll, ctx)
 */
public abstract class ConditionalDiceEffect : BaseDiceEffect
{
    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return Condition(roll, ctx) ? ApplyEffect(roll, ctx) : roll;
    }

    protected abstract bool Condition(int roll, DiceContext ctx);
    protected abstract int ApplyEffect(int roll, DiceContext ctx);
}
