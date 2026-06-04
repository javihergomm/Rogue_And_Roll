using TMPro;

public class CharacterSelectionUI
{
    private TMP_Text nameText;
    private TMP_Text descText;

    public CharacterSelectionUI(TMP_Text name, TMP_Text desc)
    {
        nameText = name;
        descText = desc;
    }

    public void UpdateInfo(CharacterSO data)
    {
        bool unlocked = Unlocks.IsUnlocked(data.characterID);

        if (nameText != null)
            nameText.text = unlocked ? data.characterName : "????";

        if (descText != null)
        {
            if (unlocked)
            {
                descText.text = data.description;
            }
            else
            {
                // Si no has puesto un unlockHint, muestra un texto por defecto
                descText.text = string.IsNullOrEmpty(data.unlockHint)
                    ? "Este personaje está bloqueado."
                    : data.unlockHint;
            }
        }
    }
}
