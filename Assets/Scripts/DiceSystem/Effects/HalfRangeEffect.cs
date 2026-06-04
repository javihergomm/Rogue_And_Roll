using UnityEngine;

[CreateAssetMenu(
    fileName = "HalfRangeEffect",
    menuName = "Effects/Dice/HalfRange"
)]
public class HalfRangeEffect : BaseDiceEffect
{
    [SerializeField] private bool roundUp = true;

    public override void ApplyToRange(ref int minAllowed, ref int maxAllowed, DiceContext ctx)
    {
        int half = roundUp ? Mathf.CeilToInt(maxAllowed / 2f) : maxAllowed / 2;
        maxAllowed = Mathf.Min(maxAllowed, half);
    }
    public void SetRoundUp(bool r) => roundUp = r;

}
