using UnityEngine;
using TMPro;
using System.Collections.Generic;

/*
 * CharacterSelectManager
 * ----------------------
 * Coordinates the character selection system.
 * - Assigns CharacterSO data to CharacterSlot objects
 * - Controls the selector panel visibility
 * - Confirms the selected character
 * - Spawns the character prefab
 * - Activates character effects
 *
 * UI details, slot highlight logic, and click flow
 * are handled by other components.
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

    // Prevents reopening the selector after a character is chosen
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

        // Save selection
        PlayerPrefs.SetString("SelectedCharacterID", character.characterID);
        PlayerPrefs.SetInt("HasSelectedCharacter", 1);

        // Close UI and resume game
        selectorPanel.SetActive(false);
        Time.timeScale = 1f;

        // Spawn the cup in the world
        CharacterSpawner.Instance.Spawn(selectedCharacter, cupPrefab);

        // Activate all character effects
        CharacterEffectManager.Instance.ActivateCharacter(selectedCharacter);
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
