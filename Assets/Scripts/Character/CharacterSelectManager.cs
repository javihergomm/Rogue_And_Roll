using UnityEngine;
using TMPro;
using System.Collections.Generic;

/*
 * CharacterSelectManager
 * ----------------------
 * Handles the character (cup) selection UI.
 * Assigns CharacterSO data to CharacterSlot objects,
 * updates the info panel, and confirms the selected character.
 *
 * When a character is confirmed:
 * - Saves the selection in PlayerPrefs
 * - Spawns the cup prefab at the correct spawn point
 * - Activates all character effects through CharacterEffectManager
 */
public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance;

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

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        selectorPanel.SetActive(false);
        AssignCharactersToSlots();
    }

    /*
     * AssignCharactersToSlots
     * -----------------------
     * Assigns each CharacterSO to a CharacterSlot in the scene.
     */
    private void AssignCharactersToSlots()
    {
        for (int i = 0; i < slots.Count && i < characters.Length; i++)
        {
            slots[i].Setup(characters[i], infoNameText, infoDescText);
        }
    }

    /*
     * ShowSelector
     * ------------
     * Opens the character selection panel and pauses the game.
     */
    public void ShowSelector()
    {
        if (!selectorDisabledForever)
        {
            selectorPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    /*
     * HideSelectorPanel
     * -----------------
     * Temporarily hides the selector (used when a popup appears).
     */
    public void HideSelectorPanel()
    {
        if (!selectorDisabledForever)
            selectorPanel.SetActive(false);
    }

    /*
     * ShowSelectorPanel
     * -----------------
     * Restores the selector if the popup is cancelled.
     */
    public void ShowSelectorPanel()
    {
        if (!selectorDisabledForever)
            selectorPanel.SetActive(true);
    }

    /*
     * DisableSelectorForever
     * ----------------------
     * Permanently disables the selector after a character is chosen.
     */
    public void DisableSelectorForever()
    {
        selectorDisabledForever = true;
        selectorPanel.SetActive(false);
    }

    /*
     * DeselectAllSlots
     * ----------------
     * Removes highlight from all character slots.
     */
    public void DeselectAllSlots()
    {
        foreach (var slot in slots)
            slot.DeselectSlot();
    }

    /*
     * ConfirmCharacter
     * ----------------
     * Saves the selected character, spawns it,
     * and activates all its effects through CharacterEffectManager.
     */
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
        SpawnCharacter();

        // Activate all character effects
        CharacterEffectManager.Instance.ActivateCharacter(selectedCharacter);
    }

    /*
     * SpawnCharacter
     * --------------
     * Instantiates the selected cup prefab at the correct spawn point.
     */
    private void SpawnCharacter()
    {
        Transform spawnPoint = GameObject.Find(selectedCharacter.spawnPointName)?.transform;

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn point not found: " + selectedCharacter.spawnPointName);
            return;
        }

        if (spawnedCup != null)
            Destroy(spawnedCup);

        spawnedCup = Instantiate(cupPrefab, spawnPoint.position, spawnPoint.rotation);

        // Apply cup color to all materials
        Renderer rend = spawnedCup.GetComponent<Renderer>();
        foreach (var mat in rend.materials)
            mat.color = selectedCharacter.characterColor;
    }

    /*
     * IsSelectorOpen
     * --------------
     * Returns true if the selector is open and not permanently disabled.
     */
    public bool IsSelectorOpen()
    {
        return selectorPanel.activeSelf && !selectorDisabledForever;
    }

    /*
     * IsAnySelectorUIOpen
     * --------------------
     * Returns true if either the selector or a popup is open.
     * Used to block other UI systems (inventory, shop, etc.).
     */
    public bool IsAnySelectorUIOpen()
    {
        if (selectorPanel.activeSelf && !selectorDisabledForever)
            return true;

        if (OptionPopupManager.Instance != null &&
            OptionPopupManager.Instance.IsPopupOpen)
            return true;

        return false;
    }
}
