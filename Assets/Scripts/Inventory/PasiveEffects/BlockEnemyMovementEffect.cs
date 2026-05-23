using UnityEngine;

[CreateAssetMenu(fileName = "BlockEnemyMovementEffect", menuName = "Effects/Passive/BlockEnemyMovement")]
public class BlockEnemyMovementEffect : BasePassiveEffect
{
    public int turnsBlocked = 1;   
    private int remaining;

    public override void Activate()
    {
        var clone = Instantiate(this);
        clone.remaining = turnsBlocked;

        StatManager.Instance.PreventEnemyMovementThisTurn = true;

        CharacterEffectManager.Instance.AddPassiveEffect(clone);
    }

    public override void OnTurnStart()
    {
        if (remaining > 0)
        {
            StatManager.Instance.PassiveCtx.PreventEnemyMovement = true;
            StatManager.Instance.PreventEnemyMovementThisTurn = true;

            remaining--;
        }
        else
        {
            CharacterEffectManager.Instance.RemovePassiveEffect(this);
        }
    }
}
