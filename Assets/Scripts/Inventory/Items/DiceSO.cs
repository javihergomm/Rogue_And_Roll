using System.Collections.Generic;
using UnityEngine;

/*
 * DiceSO
 * ------
 * Represents a dice item.
 * Stores dice effects.
 */
[CreateAssetMenu(fileName = "NewDice", menuName = "Inventory/Dice")]
public class DiceSO : BaseItemSO
{
    [Header("Dice Settings")]
    [SerializeField] private DiceType diceType;

    [Header("Effects")]
    [SerializeField] private BaseEffect[] effects;

    public DiceType DiceType => diceType;
    public BaseEffect[] Effects => effects;

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

    public override void UseItem() { }

    public IEnumerable<BaseDiceEffect> GetDiceEffects()
    {
        if (effects == null)
            yield break;

        foreach (var eff in effects)
            if (eff is BaseDiceEffect diceEff)
                yield return diceEff;
    }
}
