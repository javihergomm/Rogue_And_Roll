using UnityEngine;
using System.Collections.Generic;

/*
 * Central system for managing player stats, passive effects, consumables,
 * turn progression and movement-blocking flags across turns.
 */
public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    [Header("Gold Settings")]
    [SerializeField] private int startingGold = 0;
    [SerializeField] private int maxGold = 1000;

    [Header("Roll Settings")]
    [SerializeField] private int startingRolls = 1;

    [Header("Shop Reroll Settings")]
    [SerializeField] private int maxShopRerolls = 100;

    private readonly Dictionary<StatType, int> currentValues = new();
    private readonly Dictionary<StatType, int> maxValues = new();

    public int PreviousRoll { get; private set; }
    public int CurrentTurn { get; private set; } = 1;

    public List<BaseDiceEffect> ActiveConsumableEffects { get; private set; }
        = new List<BaseDiceEffect>();

    public List<BaseDiceEffect> PendingDiceEffects { get; private set; }
        = new List<BaseDiceEffect>();

    public List<BasePassiveEffect> ActivePassiveEffects { get; private set; }
        = new List<BasePassiveEffect>();

    private ShopExitManager cachedExitManager;

    public event System.Action OnStatsChanged;

    public bool HideRollThisTurn = false;
    public bool HidePieceThisTurn = false;
    public bool HasPlayerRolledThisTurn = false;

    public bool PreventMovementThisTurn = false;
    public bool PreventEnemyMovementThisTurn = false;

    public PassiveContext PassiveCtx { get; private set; } = new PassiveContext();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        cachedExitManager = FindFirstObjectByType<ShopExitManager>();

        InitializeStats();
        NotifyUI();
    }

    private void Start()
    {
        var exit = FindFirstObjectByType<ShopExitManager>();
        if (exit != null)
            exit.OnShopStateChanged += HandleShopStateChanged;

        NotifyUI();
    }

    private void InitializeStats()
    {
        currentValues[StatType.Gold] = startingGold;
        maxValues[StatType.Gold] = maxGold;

        currentValues[StatType.Rolls] = Mathf.Max(1, startingRolls);
        maxValues[StatType.Rolls] = int.MaxValue;

        currentValues[StatType.ShopRerolls] = maxShopRerolls;
        maxValues[StatType.ShopRerolls] = maxShopRerolls;
    }

    public void RegisterConsumableEffects(ConsumableSO item)
    {
        if (item == null || item.Effects == null)
            return;

        foreach (var eff in item.Effects)
        {
            if (eff is BaseDiceEffect diceEff)
            {
                diceEff.SourceItem = item;
                ActiveConsumableEffects.Add(diceEff);
            }
            else if (eff is BasePassiveEffect passive)
            {
                RegisterPassiveEffect(passive);
            }
        }
    }

    public void RegisterPassiveEffect(BasePassiveEffect effect)
    {
        if (!ActivePassiveEffects.Contains(effect))
            ActivePassiveEffects.Add(effect);
    }

    public void ChangeStat(StatType stat, int amount)
    {
        if (!currentValues.ContainsKey(stat))
            currentValues[stat] = 0;

        currentValues[stat] += amount;

        if (stat == StatType.Rolls && currentValues[stat] < 1)
            currentValues[stat] = 1;

        if (currentValues[stat] > GetMaxValue(stat))
            currentValues[stat] = GetMaxValue(stat);

        if (currentValues[stat] < 0)
            currentValues[stat] = 0;

        NotifyUI();
    }

    public int GetCurrentValue(StatType stat)
    {
        return currentValues.TryGetValue(stat, out int value) ? value : 0;
    }

    public int GetMaxValue(StatType stat)
    {
        return maxValues.TryGetValue(stat, out int value) ? value : int.MaxValue;
    }

    public void UseShopReroll()
    {
        ChangeStat(StatType.ShopRerolls, -1);
    }

    public void OnDiceFinalResult(int finalRoll)
    {
        PreviousRoll = finalRoll;
        HasPlayerRolledThisTurn = true;

        ActiveConsumableEffects.RemoveAll(e => e.RemoveAfterRoll);
    }

    public void NextTurn()
    {
        CurrentTurn++;

        ResetTurnFlags();

        // Reset passive context
        PassiveCtx.PreventMovement = false;
        PassiveCtx.PreventEnemyMovement = false;

        // Recorrer copia para evitar InvalidOperationException
        foreach (var eff in new List<BasePassiveEffect>(ActivePassiveEffects))
        {
            eff.OnTurnStart(PassiveCtx);
        }

        PreventMovementThisTurn = PassiveCtx.PreventMovement;
        PreventEnemyMovementThisTurn = PassiveCtx.PreventEnemyMovement;

        if (PendingDiceEffects.Count > 0)
        {
            ActiveConsumableEffects.AddRange(PendingDiceEffects);
            PendingDiceEffects.Clear();
        }

        NotifyUI();
    }


    public void ResetTurnFlags()
    {
        HideRollThisTurn = false;
        HidePieceThisTurn = false;
        HasPlayerRolledThisTurn = false;

        PreventMovementThisTurn = false;
        PreventEnemyMovementThisTurn = false;
    }

    private void HandleShopStateChanged(bool inShop)
    {
        NotifyUI();
    }

    public bool IsPlayerInShop()
    {
        return cachedExitManager != null && cachedExitManager.IsInShop();
    }

    private void NotifyUI()
    {
        OnStatsChanged?.Invoke();
    }
    public void TriggerStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

}
