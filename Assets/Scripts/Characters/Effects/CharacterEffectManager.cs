using UnityEngine;
using System.Collections.Generic;

public class CharacterEffectManager : MonoBehaviour
{
    public static CharacterEffectManager Instance { get; private set; }

    [Header("Active Character")]
    public CharacterSO activeCharacter;

    // Listas REALES que usa tu sistema
    public List<BaseDiceEffect> ActiveDiceEffects { get; private set; } = new();
    public List<BasePassiveEffect> ActivePassiveEffects { get; private set; } = new();

    // Efectos temporales (Bridge of Catan, etc.)
    private readonly List<BaseConsumableEffect> activeTemporaryEffects = new();

    // Lista unificada para callbacks
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
    // ACTIVACIÓN DE PERSONAJE
    // ============================================================
    public void ActivateCharacter(CharacterSO character)
    {
        if (character == null)
        {
            Debug.LogError("CharacterEffectManager: Tried to activate a null character.");
            return;
        }

        // Quitar efectos anteriores
        if (activeCharacter != null)
            CharacterEffectApplier.RemoveEffects(activeCharacter);

        activeCharacter = character;

        // Aplicar efectos nuevos
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
    // LISTA UNIFICADA PARA CALLBACKS
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
    // CALLBACKS DE TURNO
    // ============================================================
    public void NotifyTurnStart()
    {
        foreach (var eff in allEffects)
            eff.OnTurnStart();
    }

    public void NotifyTurnEnd()
    {
        foreach (var eff in allEffects)
            eff.OnTurnEnd();
    }

    public void NotifyEnemyTurnStart(EnemyBase enemy)
    {
        foreach (var eff in allEffects)
            eff.OnEnemyTurnStart(enemy);
    }

    public void NotifyEnemyTurnEnd(EnemyBase enemy)
    {
        foreach (var eff in allEffects)
            eff.OnEnemyTurnEnd(enemy);
    }

    // ============================================================
    // CALLBACKS DE MOVIMIENTO
    // ============================================================
    public void NotifyMovementStart(Movement m)
    {
        foreach (var eff in allEffects)
            eff.OnMovementStart(m);
    }

    public void NotifyMovementEnd(Movement m)
    {
        foreach (var eff in allEffects)
            eff.OnMovementEnd(m);
    }
}
