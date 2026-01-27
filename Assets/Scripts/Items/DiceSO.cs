using System.Collections.Generic;
using UnityEngine;

/*
 * DiceSO
 * ------
 * Represents a dice item.
 * Dice effects are applied automatically during a roll by DiceRollManager.
 * DiceSO does NOT execute effects itself; it only stores them.
 */
[CreateAssetMenu(fileName = "NewDice", menuName = "Inventory/Dice")]
public class DiceSO : BaseItemSO
{
    [Header("Dice Settings")]
    public DiceType diceType;

    [Header("Effects (Any Type)")]
    public BaseEffect[] effects;

    public int GetMaxFaceValue()
    {
        switch (diceType)
        {
            case DiceType.D4: return 4;
            case DiceType.D6: return 6;
            case DiceType.D8: return 8;
            case DiceType.D20: return 20;
        }
        return 0;
    }

    // Dice are not "used" manually
    public override void UseItem() { }

    /*
     * Returns all dice effects attached to this dice.
     * DiceRollManager calls this when applying roll modifications.
     */
    public IEnumerable<BaseDiceEffect> GetDiceEffects()
    {
        if (effects == null)
            yield break;

        foreach (var eff in effects)
            if (eff is BaseDiceEffect diceEff)
                yield return diceEff;
    }
}
