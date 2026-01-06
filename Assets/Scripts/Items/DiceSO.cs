using UnityEngine;

/*
 * DiceSO
 * ------
 * ScriptableObject representing a dice item.
 * Defines the dice type and an optional roll-modifying effect.
 * Dice items are rolled through physical dice in the scene, not through this class.
 */
[CreateAssetMenu(fileName = "NewDice", menuName = "Inventory/Dice")]
public class DiceSO : BaseItemSO
{
    [Header("Dice Settings")]
    public DiceType diceType;            // Type of dice (D4, D6, etc.)

    [Header("Roll Effect (Optional)")]
    public BaseDiceEffect diceEffect;    // Optional effect applied during roll processing

    /*
     * Returns the maximum face value for this dice based on its type.
     */
    public int GetMaxFaceValue()
    {
        switch (diceType)
        {
            case DiceType.D4: return 4;
            case DiceType.D6: return 6;
            case DiceType.D8: return 8;
            case DiceType.D10: return 10;
            case DiceType.D12: return 12;
            case DiceType.D20: return 20;
        }

        return 0;
    }

    public override void UseItem() { }
}
