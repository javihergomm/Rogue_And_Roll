using UnityEngine;
using System.Collections.Generic;

public class CharacterEffectManager : MonoBehaviour
{
    public static CharacterEffectManager Instance { get; private set; }

    [Header("Active Character")]
    public CharacterSO activeCharacter;

    public List<BaseDiceEffect> ActiveDiceEffects { get; private set; } = new();
    public List<BasePassiveEffect> ActivePassiveEffects { get; private set; } = new();

    private readonly List<BaseConsumableEffect> activeTemporaryEffects = new();

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
    // ACTIVACION DE PERSONAJE
    // ============================================================
    public void ActivateCharacter(CharacterSO character)
    {
        if (character == null)
        {
            Debug.LogError("CharacterEffectManager: Tried to activate a null character.");
            return;
        }

        if (activeCharacter != null)
            CharacterEffectApplier.RemoveEffects(activeCharacter);

        activeCharacter = character;

        CharacterEffectApplier.ApplyEffects(character);

        RebuildUnifiedEffectList();

        Debug.Log("[CharacterEffectManager] Activated character: " + character.characterName);
    }

    // ============================================================
    // REGISTRO DE EFECTOS
    // ============================================================
    public void AddDiceEffect(BaseDiceEffect eff)
    {
        if (eff != null && !ActiveDiceEffects.Contains(eff))
            ActiveDiceEffects.Add(eff);

        RebuildUnifiedEffectList();
    }

    public void AddPassiveEffect(BasePassiveEffect eff)
    {
        if (eff != null && !ActivePassiveEffects.Contains(eff))
            ActivePassiveEffects.Add(eff);

        RebuildUnifiedEffectList();
    }

    public void AddTemporaryEffect(BaseConsumableEffect eff)
    {
        if (eff != null && !activeTemporaryEffects.Contains(eff))
            activeTemporaryEffects.Add(eff);

        RebuildUnifiedEffectList();
    }

    public void RemoveDiceEffect(BaseDiceEffect eff)
    {
        if (eff != null)
            ActiveDiceEffects.Remove(eff);

        RebuildUnifiedEffectList();
    }

    public void RemovePassiveEffect(BasePassiveEffect eff)
    {
        if (eff != null)
            ActivePassiveEffects.Remove(eff);

        RebuildUnifiedEffectList();
    }

    public void RemoveTemporaryEffect(BaseConsumableEffect eff)
    {
        if (eff != null)
            activeTemporaryEffects.Remove(eff);

        RebuildUnifiedEffectList();
    }

    // ============================================================
    // LISTA UNIFICADA
    // ============================================================
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
    // CALLBACKS DE TURNO (ARREGLADOS)
    // ============================================================
    public void NotifyTurnStart()
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnTurnStart();
    }

    public void NotifyTurnEnd()
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnTurnEnd();
    }

    public void NotifyEnemyTurnStart(EnemyBase enemy)
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnEnemyTurnStart(enemy);
    }

    public void NotifyEnemyTurnEnd(EnemyBase enemy)
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnEnemyTurnEnd(enemy);
    }

    // ============================================================
    // CALLBACKS DE MOVIMIENTO (ARREGLADOS)
    // ============================================================
    public void NotifyMovementStart(Movement m)
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnMovementStart(m);
    }

    public void NotifyMovementEnd(Movement m)
    {
        for (int i = allEffects.Count - 1; i >= 0; i--)
            allEffects[i].OnMovementEnd(m);
    }
}
