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

        // Siempre usar el icono real
        characterIcon = characterData.icon;

        if (iconImage != null)
        {
            iconImage.sprite = characterIcon;

            if (!unlocked)
            {
                // Personaje bloqueado -> icono completamente negro
                iconImage.color = Color.black;
            }
            else
            {
                // Personaje desbloqueado -> icono normal
                iconImage.color = Color.white;
            }
        }
    }

    public void SetFlow(CharacterSelectionFlow f)
    {
        flow = f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        bool unlocked = Unlocks.IsUnlocked(characterData.characterID);

       
        selectionUI.UpdateInfo(characterData);

        if (!unlocked)
        {
            Debug.Log("Character locked: " + characterData.characterID);
            return;
        }

        flow.HandleClick(this);
    }

}
