using UnityEngine;
using System.Collections.Generic;

/*
 * CharacterEffectManager
 * ----------------------
 * Central manager that stores and exposes all effects coming from
 * the selected cup-character (CharacterSO).
 *
 * Responsibilities:
 * - Store the active CharacterSO
 * - Register all dice effects and passive effects from the selected character
 * - Expose special character flags (e.g., random bonus, avoid bad tile every 3 turns)
 * - Provide global access for DiceRollManager, StatManager, etc.
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

        // Load dice effects
        if (character.diceEffects != null)
        {
            foreach (var eff in character.diceEffects)
            {
                if (eff != null)
                    ActiveDiceEffects.Add(eff);
            }
        }

        // Load passive effects
        if (character.passiveEffects != null)
        {
            foreach (var eff in character.passiveEffects)
            {
                if (eff != null)
                    ActivePassiveEffects.Add(eff);
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
