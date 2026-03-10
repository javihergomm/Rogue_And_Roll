using UnityEngine;

/*
 * BasePassiveEffect
 * -----------------
 * Base class for passive effects.
 *
 * Passive effects:
 * - Are typically activated once (Activate) when granted by an item or event.
 * - May stay active for multiple turns.
 * - Receive turn-based callbacks (OnTurnStart, OnTurnEnd, etc.).
 *
 * Subclasses override only the methods they need.
 */
public abstract class BasePassiveEffect : BaseEffect
{
    /*
     * Called when the passive effect is first applied/activated.
     * Use this to initialize state, register in managers, etc.
     * Default implementation does nothing.
     */
    public virtual void Activate() { }

    public virtual void OnTurnStart(PassiveContext ctx) { }
    public virtual void OnTurnEnd(PassiveContext ctx) { }
    public virtual void OnMove(PassiveContext ctx) { }
    public virtual void OnEnterTile(PassiveContext ctx) { }
    public virtual void OnDangerTile(PassiveContext ctx) { }
    public virtual void OnRevealTile(PassiveContext ctx) { }
}
