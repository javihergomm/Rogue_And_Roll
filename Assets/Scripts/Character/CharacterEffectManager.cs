using UnityEngine;
using System.Collections.Generic;

/*
 * CharacterEffectManager
 * ----------------------
 * Central manager that stores and exposes all effects coming from
 * the selected cup-character (CharacterSO).
 *
 * This manager does NOT execute effects directly.
 * It only stores them so other systems can query them.
 */
public class CharacterEffectManager : MonoBehaviour
{
    public static CharacterEffectManager Instance;

    [Header("Active Character")]
    public CharacterSO activeCharacter;

    [Header("Active Effects")]
    public List<BaseDiceEffect> ActiveDiceEffects = new List<BaseDiceEffect>();
    public List<BasePassiveEffect> ActivePassiveEffects = new List<BasePassiveEffect>();

    [Header("Special Character Flags")]
    public bool isBasicCup;
    public bool hasRandomBonus;
    public bool avoidsBadTileEvery3;
    public bool isMetalCup;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    /*
     * ActivateCharacter
     * -----------------
     * Called by CharacterSelectManager after the player confirms a character.
     * Loads all effects and flags from the selected CharacterSO.
     */
    public void ActivateCharacter(CharacterSO character)
    {
        if (character == null)
        {
            Debug.LogError("CharacterEffectManager: Tried to activate a null character.");
            return;
        }

        activeCharacter = character;

        // Clear previous effects
        ActiveDiceEffects.Clear();
        ActivePassiveEffects.Clear();

        // Load all effects from the character
        if (character.effects != null)
        {
            foreach (var eff in character.effects)
            {
                if (eff == null)
                    continue;

                if (eff is BaseDiceEffect diceEff)
                    ActiveDiceEffects.Add(diceEff);

                else if (eff is BasePassiveEffect passiveEff)
                    ActivePassiveEffects.Add(passiveEff);
            }
        }

        // Load special flags
        isBasicCup = character.isBasicCup;
        hasRandomBonus = character.hasRandomBonus;
        avoidsBadTileEvery3 = character.avoidsBadTileEvery3;
        isMetalCup = character.isMetalCup;

        Debug.Log("[CharacterEffectManager] Activated character: " + character.characterName);
        Debug.Log("[CharacterEffectManager] Dice effects: " + ActiveDiceEffects.Count);
        Debug.Log("[CharacterEffectManager] Passive effects: " + ActivePassiveEffects.Count);
    }

    /*
     * HasDiceEffect<T>
     * ----------------
     * Utility method to check if the active character has a specific dice effect type.
     */
    public bool HasDiceEffect<T>() where T : BaseDiceEffect
    {
        foreach (var eff in ActiveDiceEffects)
        {
            if (eff is T)
                return true;
        }
        return false;
    }

    /*
     * HasPassiveEffect<T>
     * -------------------
     * Utility method to check if the active character has a specific passive effect type.
     */
    public bool HasPassiveEffect<T>() where T : BasePassiveEffect
    {
        foreach (var eff in ActivePassiveEffects)
        {
            if (eff is T)
                return true;
        }
        return false;
    }
}
