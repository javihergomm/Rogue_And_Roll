using UnityEngine;

/*
 * StrategyTotemEffect
 * -------------------
 * Rolls a d4 at the start of each turn.
 * If the result meets the threshold, the player ignores
 * one negative effect this turn.
 */
[CreateAssetMenu(fileName = "StrategyTotemEffect", menuName = "Effects/Passive/StrategyTotem")]
public class StrategyTotemEffect : BasePassiveEffect
{
    [SerializeField] private int requiredValue = 2;

    public override void OnTurnStart(PassiveContext ctx)
    {
        int roll = Random.Range(1, 5); // d4

        if (roll >= requiredValue)
            ctx.IgnoreNegativeEffect = true;
    }
}
