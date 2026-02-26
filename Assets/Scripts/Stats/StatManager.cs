using UnityEngine;
using System.Collections.Generic;

/*
 * StatManager
 * -----------
 * Central system for managing player stats and turn-based state.
 *
 * Responsibilities:
 * - Stores and updates core stats (gold, rolls, rerolls).
 * - Tracks turn progression and dice results.
 * - Manages temporary dice effects from consumable items.
 * - Handles effects that apply immediately or on the next roll.
 * - Executes passive effects at the start of each turn.
 * - Provides turn-based flags used by other systems (UI, movement, effects).
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
    [SerializeField] private int maxShopRerolls = 2;

    private readonly Dictionary<StatType, int> currentValues = new();
    private readonly Dictionary<StatType, int> maxValues = new();

    public int PreviousRoll { get; private set; }
    public int CurrentTurn { get; private set; } = 1;

    // Active dice effects applied to the current roll
    public List<BaseDiceEffect> ActiveConsumableEffects { get; private set; }
        = new List<BaseDiceEffect>();

    // Dice effects waiting for the next available roll
    public List<BaseDiceEffect> PendingDiceEffects { get; private set; }
        = new List<BaseDiceEffect>();

    // Active passive effects executed at the start of each turn
    public List<BasePassiveEffect> ActivePassiveEffects { get; private set; }
        = new List<BasePassiveEffect>();

    private ShopExitManager cachedExitManager;

    public event System.Action OnStatsChanged;

    /*
     * Turn-based flags used by consumable and passive effects.
     * These are reset every turn.
     */
    public bool HideRollThisTurn = false;
    public bool HidePieceThisTurn = false;
    public bool HasPlayerRolledThisTurn = false;
    public bool PreventMovementThisTurn = false;

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

    /*
     * Registers dice effects from a consumable item.
     * Effects may apply immediately or be queued for the next roll.
     */
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

    /*
     * Registers a passive effect that executes every turn.
     */
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

    /*
     * Called when the final dice result is known.
     * Stores the roll and removes single-use effects.
     */
    public void OnDiceFinalResult(int finalRoll)
    {
        PreviousRoll = finalRoll;
        HasPlayerRolledThisTurn = true;

        ActiveConsumableEffects.RemoveAll(e => e.RemoveAfterRoll);
    }

    /*
     * Advances the turn counter, executes passive effects,
     * activates pending dice effects, and resets temporary flags.
     */
    public void NextTurn()
    {
        CurrentTurn++;

        ResetTurnFlags();

        // Execute passive effects
        PassiveContext ctx = new();
        foreach (var eff in ActivePassiveEffects)
            eff.OnTurnStart(ctx);

        // Apply passive movement block
        PreventMovementThisTurn = ctx.PreventMovement;

        // Activate pending dice effects
        if (PendingDiceEffects.Count > 0)
        {
            ActiveConsumableEffects.AddRange(PendingDiceEffects);
            PendingDiceEffects.Clear();
        }

        NotifyUI();
    }

    /*
     * Resets temporary flags that only last one turn.
     */
    public void ResetTurnFlags()
    {
        HideRollThisTurn = false;
        HidePieceThisTurn = false;
        HasPlayerRolledThisTurn = false;
        PreventMovementThisTurn = false;
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
}
