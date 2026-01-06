using UnityEngine;

/*
 * AdditiveDiceEffect
 * ------------------
 * Adds or subtracts a fixed value from the roll result.
 */
[CreateAssetMenu(fileName = "AdditiveDiceEffect", menuName = "Effects/Dice/Additive")]
public class AdditiveDiceEffect : BaseDiceEffect
{
    public int amount = 1;

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return roll + amount;
    }
}
