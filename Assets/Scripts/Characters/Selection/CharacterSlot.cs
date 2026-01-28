using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CharacterSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public Sprite characterIcon;
    [SerializeField] private Image iconImage;

    [Header("Highlight")]
    public CharacterSlotHighlight highlight;

    [Header("Character Data")]
    public CharacterSO characterData;

    [Header("Info Panel")]
    public CharacterSelectionUI selectionUI;

    // Flow controller
    private CharacterSelectionFlow flow;

    private void Awake()
    {
        if (iconImage != null && characterIcon != null)
            iconImage.sprite = characterIcon;

        flow = GetComponent<CharacterSelectionFlow>();
    }

    public void Setup(CharacterSO data, TMP_Text nameText, TMP_Text descText)
    {
        characterData = data;

        if (data.icon != null)
        {
            characterIcon = data.icon;
            if (iconImage != null)
                iconImage.sprite = characterIcon;
        }

        selectionUI = new CharacterSelectionUI(nameText, descText);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        flow.HandleClick(this);
    }
}
