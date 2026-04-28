using UnityEngine;

[CreateAssetMenu(
    fileName = "AddDiceEffect",
    menuName = "Effects/Dice/Add"
)]
public class AddDiceEffect : BaseDiceEffect
{
    [SerializeField] private int amount = 1;

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        return ctx.IsFinal ? roll + amount : roll;
    }
    public void SetAmount(int a) => amount = a;

}
