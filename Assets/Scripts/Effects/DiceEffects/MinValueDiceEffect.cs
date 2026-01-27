using UnityEngine;

[CreateAssetMenu(fileName = "MinValueDiceEffect", menuName = "Effects/Dice/MinValue")]
public class MinValueDiceEffect : BaseDiceEffect
{
    [SerializeField]
    private int minValue = 1;

    public override void ApplyToRange(ref int minAllowed, ref int maxAllowed, DiceContext ctx)
    {
        minAllowed = Mathf.Max(minAllowed, minValue);
    }

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return Mathf.Max(roll, minValue);
    }
}
