using UnityEngine;
using System;

/*
 * BaseDiceEffect
 * --------------
 * Dice effects can be:
 *  - Local (affect only the die they belong to)
 *  - Global (affect all active dice)
 */
public abstract class BaseDiceEffect : BaseEffect
{
    // If true, this effect applies to ALL dice.
    // If false, it applies only to the die that owns it.
    public bool isGlobalEffect = false;

    public virtual bool RequiresAsyncResolution => false;

    public virtual int ModifyRoll(int roll, DiceContext ctx)
    {
        return roll;
    }

    public virtual void ModifyRollAsync(int currentRoll, DiceContext ctx, Action<int> callback)
    {
        callback(currentRoll);
    }

    public virtual void ApplyToRange(ref int minAllowed, ref int maxAllowed, DiceContext ctx)
    {
    }
}
