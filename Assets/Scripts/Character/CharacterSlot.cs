using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/*
 * CharacterSlot
 * -------------
 * Represents a single character icon in the selection panel.
 * Handles selection highlight, info panel updates, and confirmation popup.
 * CharacterSelectManager controls global selection logic.
 */
public class CharacterSlot : MonoBehaviour, IPointerClickHandler
{
    // UI elements
    public Sprite characterIcon;              
    [SerializeField] private Image iconImage; 
    public GameObject selectedShader;

    // Character data assigned to this slot
    public CharacterSO characterData;

    // Info panel references (assigned by the manager)
    public TMP_Text infoNameText;
    public TMP_Text infoDescText;

    // Selection state
    public bool thisCharacterSelected;

    private bool clickedOnce = false;

    private void Awake()
    {
        if (selectedShader != null)
            selectedShader.SetActive(false);

        // Show the inspector sprite immediately
        if (iconImage != null && characterIcon != null)
            iconImage.sprite = characterIcon;
    }

    /*
     * Setup
     * -----
     * Initializes the slot with character data and info panel references.
     */
    public void Setup(CharacterSO data, TMP_Text nameText, TMP_Text descText)
    {
        characterData = data;

        // Override inspector sprite if CharacterSO has one
        if (data.icon != null)
        {
            characterIcon = data.icon;
            if (iconImage != null)
                iconImage.sprite = characterIcon;
        }

        infoNameText = nameText;
        infoDescText = descText;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!clickedOnce)
        {
            CharacterSelectManager.Instance.DeselectAllSlots();
            SelectSlot();
            clickedOnce = true;
            return;
        }

        clickedOnce = false;

        CharacterSelectManager.Instance.HideSelectorPanel();

        OptionPopupManager.Instance.ShowConfirmCharacterPopup(
            characterData.characterName,
            () =>
            {
                CharacterSelectManager.Instance.ConfirmCharacter(characterData);
                CharacterSelectManager.Instance.DisableSelectorForever();
            },
            () =>
            {
                CharacterSelectManager.Instance.ShowSelectorPanel();
            }
        );
    }

    public void SelectSlot()
    {
        if (selectedShader != null)
            selectedShader.SetActive(true);

        thisCharacterSelected = true;

        if (infoNameText != null)
            infoNameText.text = characterData.characterName;

        if (infoDescText != null)
            infoDescText.text = characterData.description;
    }

    public void DeselectSlot()
    {
        if (selectedShader != null)
            selectedShader.SetActive(false);

        thisCharacterSelected = false;
        clickedOnce = false;
    }
}
