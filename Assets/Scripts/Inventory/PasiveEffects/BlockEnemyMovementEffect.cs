using UnityEngine;

[CreateAssetMenu(fileName = "BlockEnemyMovementEffect", menuName = "Effects/Passive/BlockEnemyMovement")]
public class BlockEnemyMovementEffect : BasePassiveEffect
{
    public int turnsBlocked = 1;   // Total turns to block
    private int remaining;         // Turns left

    public override void Activate()
    {
        // Clone so each activation has its own timer
        var clone = Instantiate(this);
        clone.remaining = turnsBlocked;

        // Block movement this turn
        StatManager.Instance.PreventEnemyMovementThisTurn = true;

        // Register effect
        CharacterEffectManager.Instance.AddPassiveEffect(clone);
    }

    public override void OnTurnStart()
    {
        if (remaining > 0)
        {
            // Keep blocking
            StatManager.Instance.PassiveCtx.PreventEnemyMovement = true;
            StatManager.Instance.PreventEnemyMovementThisTurn = true;

            remaining--;
        }
        else
        {
            // Remove when finished
            CharacterEffectManager.Instance.RemovePassiveEffect(this);
        }
    }
}
