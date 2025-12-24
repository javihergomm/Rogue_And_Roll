using UnityEngine;

/*
 * DiceSO
 * ------
 * ScriptableObject representing a dice item.
 * Supports optional special effects through BaseDiceEffect.
 */
[CreateAssetMenu(fileName = "NewDice", menuName = "Inventory/Dice")]
public class DiceSO : BaseItemSO
{
    [Header("Dice Settings")]
    public DiceType diceType;       // Type of dice (enum defined elsewhere)
    public int diceFaces = 6;       // Number of faces on the dice

    [Header("Special Effect (Optional)")]
    public BaseDiceEffect effect;   // Null for normal dice (e.g., base D6)

    // Dice items are not used directly from inventory
    public override void UseItem() { }

    // Rolls the dice and returns a value between 1 and diceFaces
    public int Roll()
    {
        return Random.Range(1, diceFaces + 1);
    }

    // Executes the special effect if assigned and returns the modified roll
    public int ApplyEffect(int roll, DiceContext context)
    {
        if (effect != null)
            return effect.ModifyRoll(roll, context);

        return roll;
    }
}
