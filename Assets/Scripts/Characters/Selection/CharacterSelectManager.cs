using UnityEngine;
using TMPro;
using System.Collections.Generic;

/*
 * CharacterSelectManager
 * ----------------------
 * Manages the character selection UI and logic.
 * - Assigns CharacterSO data to CharacterSlot objects
 * - Shows and hides the selector panel
 * - Confirms the selected character
 * - Spawns the selected character in the world
 */
public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject selectorPanel;

    [Header("Info Panel")]
    [SerializeField] private TMP_Text infoNameText;
    [SerializeField] private TMP_Text infoDescText;

    [Header("Characters")]
    [SerializeField] private CharacterSO[] characters;
    [SerializeField] private List<CharacterSlot> slots;

    [Header("Cup Prefab")]
    [SerializeField] private GameObject cupPrefab;

    private CharacterSO selectedCharacter;
    private GameObject spawnedCup;

    // Exposes the selected character so other systems can read its data
    public CharacterSO SelectedCharacter => selectedCharacter;

    private bool selectorDisabledForever = false;


    // -------------------------------------------------------------------------
    // INITIALIZATION
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        selectorPanel.SetActive(false);
        AssignCharactersToSlots();
    }

    private void Start()
    {
        // Opens the selector when the game starts
        ShowSelector();
    }


    // -------------------------------------------------------------------------
    // SLOT SETUP
    // -------------------------------------------------------------------------

    private void AssignCharactersToSlots()
    {
        for (int i = 0; i < slots.Count && i < characters.Length; i++)
        {
            slots[i].Setup(characters[i], infoNameText, infoDescText);
        }
    }


    // -------------------------------------------------------------------------
    // PANEL CONTROL
    // -------------------------------------------------------------------------

    public void ShowSelector()
    {
        if (selectorDisabledForever)
            return;

        selectorPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideSelectorPanel()
    {
        if (!selectorDisabledForever)
            selectorPanel.SetActive(false);
    }

    public void ShowSelectorPanel()
    {
        if (!selectorDisabledForever)
            selectorPanel.SetActive(true);
    }

    public void DisableSelectorForever()
    {
        selectorDisabledForever = true;
        selectorPanel.SetActive(false);
    }


    // -------------------------------------------------------------------------
    // SLOT CONTROL
    // -------------------------------------------------------------------------

    public void DeselectAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.highlight.Deselect();
            slot.GetComponent<CharacterSelectionFlow>().ResetClick();
        }
    }


    // -------------------------------------------------------------------------
    // CHARACTER CONFIRMATION
    // -------------------------------------------------------------------------

    public void ConfirmCharacter(CharacterSO character)
    {
        selectedCharacter = character;

        // DEBUGS IMPORTANTES
        Debug.Log("=== CHARACTER SELECTED ===");
        Debug.Log("Character ID: " + character.characterID);
        Debug.Log("Character Name: " + character.characterName);
        Debug.Log("Character SpawnPointName: " + character.spawnPointName);

        PlayerPrefs.SetString("SelectedCharacterID", character.characterID);
        PlayerPrefs.SetInt("HasSelectedCharacter", 1);

        selectorPanel.SetActive(false);
        Time.timeScale = 1f;

        // DEBUG: Antes de spawnear
        Debug.Log("Calling CharacterSpawner.Instance.Spawn() with spawnPointName = " + character.spawnPointName);

        CharacterSpawner.Instance.Spawn(selectedCharacter);

        // DEBUG: Después de spawnear
        Debug.Log("CharacterSpawner finished. Player should now be at: " + character.spawnPointName);

        CharacterEffectManager.Instance.ActivateCharacter(selectedCharacter);

        // DEBUG: Confirmación final
        Debug.Log("=== CHARACTER CONFIRMATION COMPLETE ===");
    }



    // -------------------------------------------------------------------------
    // UI STATE CHECKS
    // -------------------------------------------------------------------------

    public bool IsSelectorOpen()
    {
        return selectorPanel.activeSelf && !selectorDisabledForever;
    }

    public bool IsAnySelectorUIOpen()
    {
        if (IsSelectorOpen())
            return true;

        if (OptionPopupManager.Instance != null &&
            OptionPopupManager.Instance.IsPopupOpen)
            return true;

        return false;
    }
}
