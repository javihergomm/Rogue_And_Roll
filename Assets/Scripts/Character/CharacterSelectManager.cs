using UnityEngine;
using TMPro;
using System.Collections.Generic;

/*
 * CharacterSelectManager
 * ----------------------
 * Manages the character selection UI.
 * Uses existing CharacterSlot objects placed in the scene.
 * Updates the info panel when a slot is selected.
 * Uses OptionPopupManager to confirm the chosen character.
 * Saves the selected character and spawns it in the correct board area.
 */
public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance;

    [SerializeField] private GameObject selectorPanel;

    [SerializeField] private TMP_Text infoNameText;
    [SerializeField] private TMP_Text infoDescText;

    [SerializeField] private CharacterSO[] characters;

    [SerializeField] private List<CharacterSlot> slots;

    [SerializeField] private GameObject cupPrefab;

    private CharacterSO selectedCharacter;
    private GameObject spawnedCup;

    // Tracks whether the selector should remain disabled for the rest of the session
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
     * Assigns each CharacterSO to an existing CharacterSlot in the scene.
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
     * Displays the character selection panel and pauses the game.
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
     * Temporarily hides the selector while a popup is open.
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
     * Permanently disables the selector for the rest of the session.
     */
    public void DisableSelectorForever()
    {
        selectorDisabledForever = true;
        selectorPanel.SetActive(false);
    }

    /*
     * DeselectAllSlots
     * ----------------
     * Clears selection highlight from all slots.
     */
    public void DeselectAllSlots()
    {
        foreach (var slot in slots)
            slot.DeselectSlot();
    }

    /*
     * ConfirmCharacter
     * ----------------
     * Saves the selected character and spawns it on the board.
     */
    public void ConfirmCharacter(CharacterSO character)
    {
        selectedCharacter = character;

        PlayerPrefs.SetString("SelectedCharacterID", character.characterID);
        PlayerPrefs.SetInt("HasSelectedCharacter", 1);

        selectorPanel.SetActive(false);
        Time.timeScale = 1f;

        SpawnCharacter();
    }

    /*
     * SpawnCharacter
     * --------------
     * Instantiates the selected character's cup at the correct spawn point.
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

        Renderer rend = spawnedCup.GetComponent<Renderer>();

        foreach (var mat in rend.materials)
            mat.color = selectedCharacter.characterColor;
    }
}
