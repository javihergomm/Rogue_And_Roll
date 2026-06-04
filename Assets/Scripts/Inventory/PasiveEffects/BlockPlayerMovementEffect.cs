using UnityEngine;

[CreateAssetMenu(fileName = "BlockPlayerMovementEffect", menuName = "Effects/Passive/BlockPlayerMovement")]
public class BlockPlayerMovementEffect : BasePassiveEffect
{
    public int turnsBlocked = 1;   
    private int remaining;

    public override void Activate()
    {
        var clone = Instantiate(this);

        int duration = turnsBlocked;

        if (StatManager.Instance.PassiveCtx.DoubleBadSpotEffects)
            duration *= 2;

        clone.remaining = duration + 1;

        CharacterEffectManager.Instance.AddPassiveEffect(clone);
    }

    public override void OnTurnStart()
    {
        if (remaining > 0)
        {
            StatManager.Instance.PassiveCtx.PreventMovement = true;
            StatManager.Instance.PreventMovementThisTurn = true;

            remaining--;
        }
        else
        {
            CharacterEffectManager.Instance.RemovePassiveEffect(this);
        }
    }
}
