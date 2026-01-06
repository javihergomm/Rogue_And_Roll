using UnityEngine;

public abstract class BaseConsumableEffect : ScriptableObject
{
    public abstract void Activate(ConsumableContext ctx);
}
