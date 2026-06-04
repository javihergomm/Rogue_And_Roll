using UnityEngine;
using System.Collections.Generic;

/*
 * CharacterEffectManager
 * ----------------------
 * Centralized manager for all character-related effects:
 * - Dice effects (modify dice rolls)
 * - Passive effects (trigger each turn)
 * - Temporary consumable effects
 * 
 * Maintains a unified list so all callbacks (turn, movement, enemy turn)
 * are dispatched in a consistent and predictable order.
 */
public class CharacterEffectManager : MonoBehaviour
{
    public static CharacterEffectManager Instance { get; private set; }

    [Header("Active Character")]
    public CharacterSO activeCharacter;

    // Lists of active effects grouped by type
    public List<BaseDiceEffect> ActiveDiceEffects { get; private set; } = new();
    public List<BasePassiveEffect> ActivePassiveEffects { get; private set; } = new();

    // Temporary consumable effects (expire manually)
    private readonly List<BaseConsumableEffect> activeTemporaryEffects = new();

    // Unified list used for callbacks
    private readonly List<BaseEffect> allEffects = new();

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

    // ============================================================
    // CHARACTER ACTIVATION
    // ============================================================

    /*
     * Activates a character and applies all its starting effects.
     * Removes effects from the previous character if needed.
     */
    public void ActivateCharacter(CharacterSO character)
    {
        if (character == null)
        {
            Debug.LogError("CharacterEffectManager: Se intentó activar un personaje nulo.");
            return;
        }

        // Remove previous character effects
        if (activeCharacter != null)
            CharacterEffectApplier.RemoveEffects(activeCharacter);

        activeCharacter = character;

        // Apply new character effects
        CharacterEffectApplier.ApplyEffects(character);

        // Rebuild unified list
        RebuildUnifiedEffectList();

        Debug.Log("[CharacterEffectManager] Personaje activado: " + character.characterName);
    }

    // ============================================================
    // EFFECT REGISTRATION
    // ============================================================

    /*
     * Registers a dice effect and rebuilds the unified list.
     */
    public void AddDiceEffect(BaseDiceEffect eff)
    {
        if (eff != null && !ActiveDiceEffects.Contains(eff))
            ActiveDiceEffects.Add(eff);

        RebuildUnifiedEffectList();
    }

    /*
     * Registers a passive effect and rebuilds the unified list.
     */
    public void AddPassiveEffect(BasePassiveEffect eff)
    {
        if (eff != null && !ActivePassiveEffects.Contains(eff))
            ActivePassiveEffects.Add(eff);

        RebuildUnifiedEffectList();
    }

    /*
     * Registers a temporary consumable effect.
     */
    public void AddTemporaryEffect(BaseConsumableEffect eff)
    {
        if (eff != null && !activeTemporaryEffects.Contains(eff))
            activeTemporaryEffects.Add(eff);

        RebuildUnifiedEffectList();
    }

    /*
     * Removes a dice effect.
     */
    public void RemoveDiceEffect(BaseDiceEffect eff)
    {
        if (eff != null)
            ActiveDiceEffects.Remove(eff);

        RebuildUnifiedEffectList();
    }

    /*
     * Removes a passive effect.
     */
    public void RemovePassiveEffect(BasePassiveEffect eff)
    {
        if (eff != null)
            ActivePassiveEffects.Remove(eff);

        RebuildUnifiedEffectList();
    }

    /*
     * Removes a temporary consumable effect.
     */
    public void RemoveTemporaryEffect(BaseConsumableEffect eff)
    {
        if (eff != null)
            activeTemporaryEffects.Remove(eff);

        RebuildUnifiedEffectList();
    }

    // ============================================================
    // UNIFIED LIST
    // ============================================================

    /*
     * Rebuilds the unified list of all effects.
     * This ensures callbacks run in a consistent order.
     */
    private void RebuildUnifiedEffectList()
    {
        allEffects.Clear();

        foreach (var e in ActiveDiceEffects)
            allEffects.Add(e);

        foreach (var e in ActivePassiveEffects)
            allEffects.Add(e);

        foreach (var e in activeTemporaryEffects)
            allEffects.Add(e);
    }

    // ============================================================
    // TURN CALLBACKS
    // ============================================================

    /*
     * Calls OnTurnStart on all effects.
     * Reverse iteration protects against effects removing themselves.
     */
    public void NotifyTurnStart()
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnTurnStart();
    }

    /*
     * Calls OnTurnEnd on all effects.
     */
    public void NotifyTurnEnd()
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnTurnEnd();
    }

    /*
     * Calls OnEnemyTurnStart on all effects.
     */
    public void NotifyEnemyTurnStart(EnemyBase enemy)
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnEnemyTurnStart(enemy);
    }

    /*
     * Calls OnEnemyTurnEnd on all effects.
     */
    public void NotifyEnemyTurnEnd(EnemyBase enemy)
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnEnemyTurnEnd(enemy);
    }

    // ============================================================
    // MOVEMENT CALLBACKS
    // ============================================================

    /*
     * Calls OnMovementStart on all effects.
     */
    public void NotifyMovementStart(Movement m)
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnMovementStart(m);
    }

    /*
     * Calls OnMovementEnd on all effects.
     */
    public void NotifyMovementEnd(Movement m)
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnMovementEnd(m);
    }

    // ============================================================
    // PASSIVE CHECK HELPERS
    // ============================================================

    /*
     * Returns true if the Cleric Blessing passive is active.
     * Used by DiceRollManager to allow multiple rolls per turn.
     */
    public bool HasClericBlessing()
    {
        foreach (var eff in ActivePassiveEffects)
            if (eff is BendicionClerigoEffect)
                return true;

        return false;
    }
}
