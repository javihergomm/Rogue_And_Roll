using UnityEngine;

/*
 * CharacterSelectionFlow
 * ----------------------
 * Handles the click logic for character selection.
 * Controls the two-step selection process:
 *   1) Highlight a slot and show its information.
 *   2) Confirm the selected character.
 * Prevents interaction with empty slots.
 */
public class CharacterSelectionFlow : MonoBehaviour
{
    private bool clickedOnce = false;

    public void HandleClick(CharacterSlot slot)
    {
        // Prevents interaction when the slot has no character assigned
        if (slot == null || slot.characterData == null)
        {
            Debug.LogWarning("CharacterSelectionFlow: Empty slot clicked.");
            return;
        }

        if (!Unlocks.IsUnlocked(slot.characterData.characterID))
        {
            Debug.Log("Character locked: " + slot.characterData.characterID);
            return;
        }

        // First click: highlight and show character information
        if (!clickedOnce)
        {
            CharacterSelectManager.Instance.DeselectAllSlots();

            slot.highlight.Select();
            slot.selectionUI.UpdateInfo(slot.characterData);

            clickedOnce = true;
            return;
        }

        // Second click: confirm the selected character
        clickedOnce = false;

        CharacterSelectManager.Instance.HideSelectorPanel();

        PopupHelpers.ShowConfirmCharacterPopup(
            slot.characterData.characterName,
            () =>
            {
                CharacterSelectManager.Instance.ConfirmCharacter(slot.characterData);
                CharacterSelectManager.Instance.DisableSelectorForever();
            },
            () =>
            {
                CharacterSelectManager.Instance.ShowSelectorPanel();
            }
        );
    }

    /*
     * Resets the internal click state so the next click starts the selection flow again.
     */
    public void ResetClick()
    {
        clickedOnce = false;
    }
}
