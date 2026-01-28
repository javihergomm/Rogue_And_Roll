using UnityEngine;

public class CharacterSelectionFlow : MonoBehaviour
{
    private bool clickedOnce = false;

    public void HandleClick(CharacterSlot slot)
    {
        if (!clickedOnce)
        {
            CharacterSelectManager.Instance.DeselectAllSlots();

            slot.highlight.Select();
            slot.selectionUI.UpdateInfo(slot.characterData);

            clickedOnce = true;
            return;
        }

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

    public void ResetClick()
    {
        clickedOnce = false;
    }
}
