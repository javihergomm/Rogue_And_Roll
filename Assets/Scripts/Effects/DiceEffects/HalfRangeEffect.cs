using UnityEngine;

[CreateAssetMenu(fileName = "HalfRangeEffect", menuName = "Effects/Dice/HalfRange")]
public class HalfRangeEffect : BaseDiceEffect
{
    [SerializeField]
    private bool roundUp = true;

    public override void ApplyToRange(ref int minAllowed, ref int maxAllowed, DiceContext ctx)
    {
        int originalMax = maxAllowed;

        int half = roundUp
            ? Mathf.CeilToInt(originalMax / 2f)
            : originalMax / 2;

        maxAllowed = Mathf.Min(maxAllowed, half);
    }

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return roll;
    }
}
