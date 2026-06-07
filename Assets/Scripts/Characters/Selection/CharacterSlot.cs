using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/*
 * CharacterSlot
 * -------------
 * Represents a selectable character slot in the character selection UI.
 * Handles icon display, locked state, highlight, and click behavior.
 */
public class CharacterSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public Sprite characterIcon;                 // Icon currently displayed
    [SerializeField] private Image iconImage;    // UI Image component for the icon

    [Header("Highlight")]
    public CharacterSlotHighlight highlight;     // Visual highlight for selection

    [Header("Character Data")]
    public CharacterSO characterData;            // Data for the character represented by this slot

    [Header("Info Panel")]
    public CharacterSelectionUI selectionUI;     // UI panel that shows name and description

    // Flow controller for selection logic
    private CharacterSelectionFlow flow;

    private void Awake()
    {
        // Attempts to get the flow controller on the same GameObject
        flow = GetComponent<CharacterSelectionFlow>();
    }

    /*
     * Initializes the slot with character data and UI references.
     */
    public void Setup(CharacterSO data, TMP_Text nameText, TMP_Text descText)
    {
        characterData = data;
        selectionUI = new CharacterSelectionUI(nameText, descText);

        UpdateIcon();
    }

    /*
     * Updates the displayed icon based on unlock status.
     * Locked characters show a fully black icon.
     */
    private void UpdateIcon()
    {
        bool unlocked = Unlocks.IsUnlocked(characterData.characterID);

        // Always use the real icon
        characterIcon = characterData.icon;

        if (iconImage != null)
        {
            iconImage.sprite = characterIcon;

            if (!unlocked)
            {
                // Locked character -> fully black icon
                iconImage.color = Color.black;
            }
            else
            {
                // Unlocked character -> normal icon
                iconImage.color = Color.white;
            }
        }
    }

    /*
     * Allows external assignment of the flow controller.
     */
    public void SetFlow(CharacterSelectionFlow f)
    {
        flow = f;
    }

    /*
     * Handles click events on the character slot.
     * Updates the info panel and triggers selection if unlocked.
     */
    public void OnPointerClick(PointerEventData eventData)
    {
        bool unlocked = Unlocks.IsUnlocked(characterData.characterID);

        // Always update the info panel, even if locked
        selectionUI.UpdateInfo(characterData);

        if (!unlocked)
        {
            return;
        }

        // Passes the click to the selection flow
        flow.HandleClick(this);
    }
}
