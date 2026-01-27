using UnityEngine;
using System;

public abstract class BaseDiceEffect : BaseEffect
{
    // Whether this effect needs asynchronous resolution (UI, player choice, animations, etc.)
    public virtual bool RequiresAsyncResolution => false;

    // Synchronous modification (default behavior for most effects)
    public virtual int ModifyRoll(int roll, DiceContext ctx)
    {
        return roll;
    }

    // Asynchronous modification (used by effects like DestinyChoice)
    // The effect MUST call the callback with the final resolved value.
    public virtual void ModifyRollAsync(int currentRoll, DiceContext ctx, Action<int> callback)
    {
        // Default async behavior: no async logic, return immediately
        callback(currentRoll);
    }

    // NEW: Allows effects to modify the allowed face range BEFORE rolling.
    // DiceRollManager calls this when building the allowed face list.
    public virtual void ApplyToRange(ref int minAllowed, ref int maxAllowed, DiceContext ctx)
    {
        // Default: no range modification
    }
}
