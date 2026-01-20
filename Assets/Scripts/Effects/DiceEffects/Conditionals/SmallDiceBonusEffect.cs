using UnityEngine;

/*
 * SmallDiceBonusEffect
 * --------------------
 * Adds a configurable bonus to the roll ONLY if the dice is D4 or D6.
 */
[CreateAssetMenu(fileName = "SmallDiceBonusEffect", menuName = "Effects/Dice/SmallDiceBonus")]
public class SmallDiceBonusEffect : ConditionalDiceEffect
{
    [SerializeField][Tooltip("Amount added to the roll when using a small dice (D4 or D6).")] private int bonusAmount = 1;

    protected override bool Condition(int roll, DiceContext ctx)
    {
        if (ctx == null || ctx.slot == null)
            return false;

        // Retrieve the dice being rolled
        BaseItemSO item = InventoryManager.Instance.GetItemSO(ctx.slot.itemName);
        if (item is DiceSO dice)
        {
            return dice.diceType == DiceType.D4 || dice.diceType == DiceType.D6;
        }

        return false;
    }

    protected override int ApplyEffect(int roll, DiceContext ctx)
    {
        return roll + bonusAmount;
    }
}
