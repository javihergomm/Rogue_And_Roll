using UnityEngine;

/*
 * SmallDiceBonusEffect
 * --------------------
 * Adds a bonus to the roll ONLY if the dice is D4 or D6.
 */
[CreateAssetMenu(fileName = "SmallDiceBonusEffect", menuName = "Effects/Dice/SmallDiceBonus")]
public class SmallDiceBonusEffect : ConditionalDiceEffect
{
    [SerializeField] private int bonusAmount = 1;

    protected override bool Condition(int roll, DiceContext ctx)
    {
        if (ctx?.slot == null)
            return false;

        BaseItemSO item = InventoryManager.Instance.GetItemSO(ctx.slot.ItemName);
        return item is DiceSO dice &&
               (dice.DiceType == DiceType.D4 || dice.DiceType == DiceType.D6);
    }

    protected override int ApplyEffect(int roll, DiceContext ctx)
    {
        return roll + bonusAmount;
    }
}

