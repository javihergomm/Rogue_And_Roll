using UnityEngine;

[CreateAssetMenu(fileName = "BlockEnemyMovementEffect", menuName = "Effects/Passive/BlockEnemyMovement")]
public class BlockEnemyMovementEffect : BasePassiveEffect
{
    [SerializeField] private int turnsBlocked = 1;
    private int remaining;

    public override void Activate()
    {
        remaining = turnsBlocked;

        // Apply immediately for this turn
        StatManager.Instance.PreventEnemyMovementThisTurn = true;

        // Register for future turns
        StatManager.Instance.RegisterPassiveEffect(this);
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
