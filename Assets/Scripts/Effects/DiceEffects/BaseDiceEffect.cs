using UnityEngine;
using System;

public abstract class BaseDiceEffect : BaseEffect
{
    // Indicates whether this effect requires asynchronous resolution (UI, choices, etc.)
    public virtual bool RequiresAsyncResolution => false;

    // Synchronous modification (default behavior)
    public virtual int ModifyRoll(int roll, DiceContext ctx)
    {
        return roll;
    }

    // Asynchronous modification (used by effects like DestinyChoice)
    // The effect must call the callback with the final result.
    public virtual void ModifyRollAsync(int currentRoll, DiceContext ctx, Action<int> callback)
    {
        callback(currentRoll);
    }
}
