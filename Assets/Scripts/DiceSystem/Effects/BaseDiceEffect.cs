using UnityEngine;
using System;

/*
 * BaseDiceEffect
 * --------------
 * Parent class for all dice-related effects.
 * Supports:
 *  - Local effects (affect only the die that owns them)
 *  - Global effects (affect all dice)
 *  - Synchronous and asynchronous roll modification
 *  - Range modification (min/max allowed faces)
 */
public abstract class BaseDiceEffect : BaseEffect
{
    [Tooltip("If true, this effect applies to ALL dice instead of only the owning die.")]
    public bool isGlobalEffect = false;

    public virtual bool RequiresAsyncResolution => false;

    // Synchronous roll modification
    public virtual int ModifyRoll(int roll, DiceContext ctx)
    {
        return roll;
    }

    // Asynchronous roll modification (e.g. Destiny Choice)
    public virtual void ModifyRollAsync(int currentRoll, DiceContext ctx, Action<int> callback)
    {
        callback?.Invoke(currentRoll);
    }

    // Range modification (min/max allowed faces)
    public virtual void ApplyToRange(ref int minAllowed, ref int maxAllowed, DiceContext ctx)
    {
        // Default: no change
    }
}
