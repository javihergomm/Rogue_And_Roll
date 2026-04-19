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
        if (nameText != null)
            nameText.text = data.characterName;

        if (descText != null)
            descText.text = data.description;
    }
}
