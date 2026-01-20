using UnityEngine;

/*
 * DestinyChoiceEffect
 * -------------------
 * Allows the player to choose between two roll results
 * for EACH active dice independently.
 *
 * DiceRollManager must pass the allowed range (minAllowed, maxAllowed)
 * after applying all range-restricting effects.
 */
[CreateAssetMenu(fileName = "DestinyChoiceEffect", menuName = "Effects/Dice/DestinyChoice")]
public class DestinyChoiceEffect : BaseDiceEffect
{
    [SerializeField]
    private bool showInPreview = true;

    // DiceRollManager calls this with the allowed range
    public int GetAlternativeRoll(int minAllowed, int maxAllowed)
    {
        return Random.Range(minAllowed, maxAllowed + 1);
    }

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        // The final choice is made outside this effect.
        return roll;
    }

    public bool ShouldShowInPreview()
    {
        return showInPreview;
    }
}
