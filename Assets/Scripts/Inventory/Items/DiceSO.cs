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

    public int GetMaxFaceValue() =>
     diceType switch
     {
         DiceType.D4 => 4,
         DiceType.D6 => 6,
         DiceType.D8 => 8,
         DiceType.D20 => 20,
         _ => 0
     };

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
