using UnityEngine;

[CreateAssetMenu(fileName = "BlockMovementEffect", menuName = "Effects/Passive/BlockMovement")]
public class BlockMovementEffect : BasePassiveEffect
{
    [SerializeField] private int turnsBlocked = 2;
    private int remaining;

    public override void Activate()
    {
        remaining = turnsBlocked;

        // Bloquea inmediatamente el turno actual del player
        StatManager.Instance.PreventMovementThisTurn = true;

        // Registrar el efecto para que reciba OnTurnStart
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
            // Cuando termina, se elimina del sistema
            StatManager.Instance.ActivePassiveEffects.Remove(this);
        }
    }
}
