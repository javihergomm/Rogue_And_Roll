using UnityEngine;
using System;

/*
 * BaseDiceEffect
 * --------------
 * Parent class for all dice-related effects.
 *
 * Supports:
 *  - Local effects (affect only the die that owns them)
 *  - Global effects (affect all dice)
 *  - Synchronous and asynchronous roll modification
 *  - Range modification (min/max allowed faces)
 *
 */
public abstract class BaseDiceEffect : BaseEffect
{

    public ConsumableSO SourceItem { get; set; }
    [Tooltip("If true, this effect applies to ALL dice instead of only the owning die.")]
    public bool isGlobalEffect = false;

    // If true, this effect requires asynchronous resolution (e.g. player choice)
    public virtual bool RequiresAsyncResolution => false;

    // If true, the effect is queued if the player already rolled this turn
    public virtual bool ApplyOnNextAvailableRoll => false;

    // If true, the effect is removed immediately after the roll
    public virtual bool RemoveAfterRoll => false;

    // Synchronous roll modification
    public virtual int ModifyRoll(int roll, DiceContext ctx)
    {
        return roll;
    }

    // Asynchronous roll modification (e.g. effects that require player input)
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
