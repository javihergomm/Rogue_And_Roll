using UnityEngine;

/*
 * BasePassiveEffect
 * -----------------
 * Passive effects stay active as long as the item or character
 * that provides them remains active.
 *
 * Subclasses override only the events they need.
 */
public abstract class BasePassiveEffect : BaseEffect
{
    public virtual void OnTurnStart(PassiveContext ctx) { }
    public virtual void OnTurnEnd(PassiveContext ctx) { }
    public virtual void OnMove(PassiveContext ctx) { }
    public virtual void OnEnterTile(PassiveContext ctx) { }
    public virtual void OnDangerTile(PassiveContext ctx) { }
    public virtual void OnRevealTile(PassiveContext ctx) { }
}
