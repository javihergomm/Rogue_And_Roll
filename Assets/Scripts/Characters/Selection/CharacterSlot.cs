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
        flow = GetComponent<CharacterSelectionFlow>();
    }

    public void Setup(CharacterSO data, TMP_Text nameText, TMP_Text descText)
    {
        characterData = data;
        selectionUI = new CharacterSelectionUI(nameText, descText);

        UpdateIcon();
    }

    private void UpdateIcon()
    {
        bool unlocked = Unlocks.IsUnlocked(characterData.characterID);

        characterIcon = unlocked ? characterData.icon : characterData.lockedIcon;

        if (iconImage != null)
            iconImage.sprite = characterIcon;
    }
    public void SetFlow(CharacterSelectionFlow f)
    {
        flow = f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
     
        if (!Unlocks.IsUnlocked(characterData.characterID))
        {
            Debug.Log("Character locked: " + characterData.characterID);
            return;
        }

        flow.HandleClick(this);
    }
}
