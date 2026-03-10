using UnityEngine;

[CreateAssetMenu(fileName = "BlockPlayerMovementEffect", menuName = "Effects/Passive/BlockPlayerMovement")]
public class BlockPlayerMovementEffect : BasePassiveEffect
{
    [SerializeField] private int turnsBlocked = 1;
    private int remaining;

    public override void Activate()
    {
        remaining = turnsBlocked;
        StatManager.Instance.PreventMovementThisTurn = true;
        StatManager.Instance.RegisterPassiveEffect(this);
    }

    public override void OnTurnStart(PassiveContext ctx)
    {
        if (remaining > 0)
        {
            ctx.PreventMovement = true;
            remaining--;
        }
        else
        {
            StatManager.Instance.ActivePassiveEffects.Remove(this);
        }
    }
}
