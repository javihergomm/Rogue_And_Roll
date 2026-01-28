using UnityEngine;
using System.Collections.Generic;

/*
 * CharacterEffectManager
 * ----------------------
 * Stores all active dice and passive effects coming from:
 *  - The selected character
 *  - Permanent items
 *  - Temporary consumable effects
 *
 * This manager does NOT execute effects.
 * Other systems simply query these lists.
 */
public class CharacterEffectManager : MonoBehaviour
{
    public static CharacterEffectManager Instance { get; private set; }

    [Header("Active Character")]
    public CharacterSO activeCharacter;

    [Header("Active Effects")]
    public List<BaseDiceEffect> ActiveDiceEffects { get; private set; } = new List<BaseDiceEffect>();
    public List<BasePassiveEffect> ActivePassiveEffects { get; private set; } = new List<BasePassiveEffect>();


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
        DontDestroyOnLoad(gameObject);
    }


    // -------------------------------------------------------------------------
    // CHARACTER ACTIVATION
    // -------------------------------------------------------------------------

    public void ActivateCharacter(CharacterSO character)
    {
        if (character == null)
        {
            Debug.LogError("CharacterEffectManager: Tried to activate a null character.");
            return;
        }

        // Remove previous character effects
        if (activeCharacter != null)
            CharacterEffectApplier.RemoveEffects(activeCharacter);

        activeCharacter = character;

        // Apply new character effects
        CharacterEffectApplier.ApplyEffects(character);

        Debug.Log("[CharacterEffectManager] Activated character: " + character.characterName);
    }


    // -------------------------------------------------------------------------
    // ADD / REMOVE EFFECTS
    // -------------------------------------------------------------------------

    public void AddDiceEffect(BaseDiceEffect eff)
    {
        if (eff != null && !ActiveDiceEffects.Contains(eff))
            ActiveDiceEffects.Add(eff);
    }

    public void AddPassiveEffect(BasePassiveEffect eff)
    {
        if (eff != null && !ActivePassiveEffects.Contains(eff))
            ActivePassiveEffects.Add(eff);
    }

    public void RemoveDiceEffect(BaseDiceEffect eff)
    {
        if (eff != null)
            ActiveDiceEffects.Remove(eff);
    }

    public void RemovePassiveEffect(BasePassiveEffect eff)
    {
        if (eff != null)
            ActivePassiveEffects.Remove(eff);
    }


    // -------------------------------------------------------------------------
    // UTILITIES
    // -------------------------------------------------------------------------

    public bool HasDiceEffect<T>() where T : BaseDiceEffect
    {
        foreach (var eff in ActiveDiceEffects)
            if (eff is T)
                return true;

        return false;
    }

    public bool HasPassiveEffect<T>() where T : BasePassiveEffect
    {
        foreach (var eff in ActivePassiveEffects)
            if (eff is T)
                return true;

        return false;
    }
}
