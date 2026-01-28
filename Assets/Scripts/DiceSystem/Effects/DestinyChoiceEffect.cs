using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DestinyChoiceEffect", menuName = "Effects/Dice/DestinyChoice")]
public class DestinyChoiceEffect : BaseDiceEffect
{
    [SerializeField] private bool showInPreview = true;

    public override bool RequiresAsyncResolution => true;

    public override void ModifyRollAsync(int currentRoll, DiceContext ctx, Action<int> callback)
    {
        List<int> allowed = DiceRollManager.Instance.GetAllowedFacesForSlot(ctx.slot);

        if (allowed == null || allowed.Count == 0)
        {
            callback(currentRoll);
            return;
        }

        int alt = currentRoll;
        int safety = 20;

        while (alt == currentRoll && safety-- > 0)
            alt = allowed[UnityEngine.Random.Range(0, allowed.Count)];

        // Nuevo sistema de opciones
        var options = new List<PopupOption>
        {
            new PopupOption($"Tirada actual: {currentRoll}", () => callback(currentRoll), isConfirm: true),
            new PopupOption($"Tirada alternativa: {alt}", () => callback(alt))
        };

        OptionPopupManager.Instance.ShowPopup(
            "Elige tu destino:",
            options
        );
    }

    public override int ModifyRoll(int roll, DiceContext ctx) => roll;

    public bool ShouldShowInPreview() => showInPreview;
}
