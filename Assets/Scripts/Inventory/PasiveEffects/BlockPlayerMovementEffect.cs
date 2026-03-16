using UnityEngine;

[CreateAssetMenu(fileName = "BlockPlayerMovementEffect", menuName = "Effects/Passive/BlockPlayerMovement")]
public class BlockPlayerMovementEffect : BasePassiveEffect
{
    // This effect blocks player movement for a number of turns using a cloned instance.

    [SerializeField] private int turnsBlocked = 1;
    private int remaining;

    public override void Activate()
    {
        // Clone the effect so each activation has its own state
        var clone = Instantiate(this);
        clone.remaining = clone.turnsBlocked;

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
            StatManager.Instance.PreventMovementThisTurn = false;
            StatManager.Instance.ActivePassiveEffects.Remove(this);
        }
    }
}
