using UnityEngine;

/*
 * DiceSO
 * ------
 * Specialized ScriptableObject for dice items.
 * Defines dice faces and roll behavior.
 */
[CreateAssetMenu(fileName = "NewDice", menuName = "Inventory/Dice")]
public class DiceSO : BaseItemSO
{
    [Header("Dice Settings")]
    public int diceFaces = 6;

    public override void UseItem()
    {
        int result = Random.Range(1, diceFaces + 1);
        Debug.Log("Dice " + itemName + " rolled: " + result);

        // Later: integrate with DiceSelector to ensure only the active dice is rolled
    }
}
