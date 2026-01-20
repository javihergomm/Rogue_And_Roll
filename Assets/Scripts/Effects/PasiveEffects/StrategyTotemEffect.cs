using UnityEngine;

/*
 * StrategyTotemEffect
 * -------------------
 * Passive effect.
 * At the start of each turn, roll a d4.
 * If the result meets the threshold, the player ignores
 * one negative effect this turn.
 *
 * This version does NOT depend on tiles or board systems.
 */
[CreateAssetMenu(fileName = "StrategyTotemEffect", menuName = "Effects/Passive/StrategyTotem")]
public class StrategyTotemEffect : BasePassiveEffect
{
    [SerializeField]
    private int requiredValue = 2; // Example: d4 >= 2

    public override void OnTurnStart(PassiveContext ctx)
    {
        int roll = Random.Range(1, 5); // d4

        if (roll >= requiredValue)
            ctx.ignoreNegativeEffect = true;
    }
}
