using UnityEngine;

public abstract class BasePassiveEffect : BaseEffect
{
    public virtual void OnTurnStart(PassiveContext ctx) { }
    public virtual void OnTurnEnd(PassiveContext ctx) { }
    public virtual void OnMove(PassiveContext ctx) { }
    public virtual void OnEnterTile(PassiveContext ctx) { }
    public virtual void OnDangerTile(PassiveContext ctx) { }
    public virtual void OnRevealTile(PassiveContext ctx) { }
}
