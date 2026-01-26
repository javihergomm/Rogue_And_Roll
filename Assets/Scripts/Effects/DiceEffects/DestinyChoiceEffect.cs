using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DestinyChoiceEffect", menuName = "Effects/Dice/DestinyChoice")]
public class DestinyChoiceEffect : BaseDiceEffect
{
    [SerializeField] private bool showInPreview = true;

    public override bool RequiresAsyncResolution => true;

    /*
     * This effect lets the player choose between:
     * - The current roll
     * - A second valid roll generated from allowed faces
     *
     * The effect is fully responsible for:
     * - Generating the alternative roll
     * - Showing the popup
     * - Calling the callback with the chosen result
     */
    public override void ModifyRollAsync(int currentRoll, DiceContext ctx, Action<int> callback)
    {
        // Get allowed faces for this dice slot
        List<int> allowed = DiceRollManager.Instance.GetAllowedFacesForSlot(ctx.slot);

        // Safety check
        if (allowed == null || allowed.Count == 0)
        {
            callback(currentRoll);
            return;
        }

        // Generate alternative roll from allowed faces
        int alt = allowed[UnityEngine.Random.Range(0, allowed.Count)];

        // Spanish popup options
        var options = new Dictionary<string, Action>
        {
            { "Elegir " + currentRoll, () => callback(currentRoll) },
            { "Elegir " + alt, () => callback(alt) }
        };

        // Show popup in Spanish
        OptionPopupManager.Instance.ShowPopup(
            "Elige tu destino:",
            options
        );
    }

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        // Synchronous version does nothing
        return roll;
    }

    public bool ShouldShowInPreview()
    {
        return showInPreview;
    }
}
