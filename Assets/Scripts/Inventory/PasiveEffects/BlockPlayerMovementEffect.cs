using UnityEngine;

[CreateAssetMenu(fileName = "BlockPlayerMovementEffect", menuName = "Effects/Passive/BlockPlayerMovement")]
public class BlockPlayerMovementEffect : BasePassiveEffect
{
    public int turnsBlocked = 1;   // Base duration
    private int remaining;         // Turns left

    public override void Activate()
    {
        // Clone so each activation has its own timer
        var clone = Instantiate(this);

        int duration = turnsBlocked;

        // Double duration if passive context requires it
        if (StatManager.Instance.PassiveCtx.DoubleBadSpotEffects)
            duration *= 2;

        // +1 so the current turn also counts
        clone.remaining = duration + 1;

        // Register effect
        CharacterEffectManager.Instance.AddPassiveEffect(clone);
    }

    public override void OnTurnStart()
    {
        if (remaining > 0)
        {
            // Block movement this turn
            StatManager.Instance.PassiveCtx.PreventMovement = true;
            StatManager.Instance.PreventMovementThisTurn = true;

            remaining--;
        }
        else
        {
            // Remove when finished
            CharacterEffectManager.Instance.RemovePassiveEffect(this);
        }
    }
}
