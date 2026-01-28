using UnityEngine;

/*
 * BlockMovementEffect
 * -------------------
 * Prevents the player from moving for a number of turns.
 */
[CreateAssetMenu(fileName = "BlockMovementEffect", menuName = "Effects/Passive/BlockMovement")]
public class BlockMovementEffect : BasePassiveEffect
{
    [SerializeField] private int turnsBlocked = 2;
    private int remaining;

    public void Activate()
    {
        remaining = turnsBlocked;
    }

    public override void OnTurnStart(PassiveContext ctx)
    {
        if (remaining > 0)
        {
            ctx.PreventMovement = true;
            remaining--;
        }
    }
}
