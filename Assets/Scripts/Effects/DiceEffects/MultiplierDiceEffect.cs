using UnityEngine;

/*
 * MultiplierDiceEffect
 * --------------------
 * Multiplies the roll result by a given factor.
 */
[CreateAssetMenu(fileName = "MultiplierDiceEffect", menuName = "Effects/Dice/Multiplier")]
public class MultiplierDiceEffect : BaseDiceEffect
{
    public float factor = 2f;

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return Mathf.RoundToInt(roll * factor);
    }
}
