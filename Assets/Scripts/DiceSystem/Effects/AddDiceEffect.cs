using UnityEngine;

[CreateAssetMenu(
    fileName = "AddDiceEffect",
    menuName = "Effects/Dice/Add"
)]
public class AddDiceEffect : BaseDiceEffect
{
    [SerializeField] private int amount = 1;

    // Este efecto solo modifica el resultado final de la tirada
    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        if (!ctx.IsFinal)
            return roll;

        return roll + amount;
    }

    public void SetAmount(int a) => amount = a;
}
