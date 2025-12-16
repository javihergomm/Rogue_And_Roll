using UnityEngine;

/*
 * DiceSO
 * ------
 * ScriptableObject especializado para dados.
 * Define el tipo de dado, número de caras y comportamiento de tirada.
 */
[CreateAssetMenu(fileName = "NewDice", menuName = "Inventory/Dice")]
public class DiceSO : BaseItemSO
{
    [Header("Dice Settings")]
    public DiceType diceType;   // Enum que ya tienes definido en DiceRoller
    public int diceFaces = 6;   // Número de caras (por claridad, aunque coincide con diceType)

    public override void UseItem()
    {
        int result = Random.Range(1, diceFaces + 1);
        Debug.Log("Dice " + itemName + " (" + diceType + ") rolled: " + result);

        // Aquí puedes integrar con DiceRollManager o UI para mostrar el resultado
    }

    // Método auxiliar para tiradas desde DiceRollManager
    public int Roll()
    {
        return Random.Range(1, diceFaces + 1);
    }
}
