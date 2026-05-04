using UnityEngine;

[CreateAssetMenu(fileName = "BlockEnemyMovementEffect", menuName = "Effects/Passive/BlockEnemyMovement")]
public class BlockEnemyMovementEffect : BasePassiveEffect
{
    // This effect blocks enemy movement for a number of turns using a cloned instance.

    public int turnsBlocked = 1;
    private int remaining;

    public override void Activate()
    {
        // Clone the effect so each activation has its own state
        var clone = Instantiate(this);
        clone.remaining = clone.turnsBlocked;

        // Apply immediately for this turn
        StatManager.Instance.PreventEnemyMovementThisTurn = true;

        // Register clone for future turns
        StatManager.Instance.RegisterPassiveEffect(clone);
    }

    public override void OnTurnStart(PassiveContext ctx)
    {
        if (remaining > 0)
        {
            ctx.PreventEnemyMovement = true;
            remaining--;
        }
        else
        {
            StatManager.Instance.ActivePassiveEffects.Remove(this);
        }
    }
}
