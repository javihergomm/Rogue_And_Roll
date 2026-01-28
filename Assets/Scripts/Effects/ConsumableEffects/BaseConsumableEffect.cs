using UnityEngine;

/*
 * BaseConsumableEffect
 * --------------------
 * Effects that trigger immediately when a consumable item is used.
 * These effects do not modify dice rolls directly.
 * They operate on the ConsumableContext (movement, tiles, map changes, etc.).
 */
public abstract class BaseConsumableEffect : BaseEffect
{
    public abstract void Activate(ConsumableContext ctx);
}
