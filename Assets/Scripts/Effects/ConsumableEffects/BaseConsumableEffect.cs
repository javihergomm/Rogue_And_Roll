using UnityEngine;

public abstract class BaseConsumableEffect : BaseEffect
{
    public abstract void Activate(ConsumableContext ctx);
}
