using UnityEngine;

[CreateAssetMenu(fileName = "BlockPlayerMovementEffect", menuName = "Effects/Passive/BlockPlayerMovement")]
public class BlockPlayerMovementEffect : BasePassiveEffect
{
    [SerializeField] private int turnsBlocked = 1;
    private int remaining;

    public override void Activate()
    {
        // Create a clone so each activation has its own state
        var clone = Instantiate(this);

        int duration = turnsBlocked;

        // Duplicate the effect duration if passive is active
        if (StatManager.Instance.PassiveCtx.DoubleBadSpotEffects)
            duration *= 2;

        clone.remaining = duration;

        StatManager.Instance.RegisterPassiveEffect(clone);
    }

    public override void OnTurnStart(PassiveContext ctx)
    {
        if (remaining > 0)
        {
            ctx.PreventMovement = true;
            StatManager.Instance.PreventMovementThisTurn = true;
            remaining--;
        }
        else
        {
            // Remove effect when finished
            StatManager.Instance.ActivePassiveEffects.Remove(this);
        }
    }
}
