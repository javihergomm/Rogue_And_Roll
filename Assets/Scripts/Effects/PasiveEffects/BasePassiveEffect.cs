using UnityEngine;

/*
 * BasePassiveEffect
 * -----------------
 * Passive effects stay active as long as the item or character
 * that provides them remains active.
 *
 * These effects react to gameplay events through callback methods.
 * Subclasses override only the events they need.
 *
 * PassiveContext provides information about the current turn,
 * tile, movement, or any other relevant state.
 */
public abstract class BasePassiveEffect : BaseEffect
{
    // Called at the start of each turn
    public virtual void OnTurnStart(PassiveContext ctx) { }

    // Called at the end of each turn
    public virtual void OnTurnEnd(PassiveContext ctx) { }

    // Called when the player moves
    public virtual void OnMove(PassiveContext ctx) { }

    // Called when the player enters any tile
    public virtual void OnEnterTile(PassiveContext ctx) { }

    // Called when the player enters a dangerous tile
    public virtual void OnDangerTile(PassiveContext ctx) { }

    // Called when a tile is revealed (if your game supports fog-of-war or hidden tiles)
    public virtual void OnRevealTile(PassiveContext ctx) { }
}
