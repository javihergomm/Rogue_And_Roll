using UnityEngine;
using System.Collections.Generic;

/*
 * StatManager
 * -----------
 * Core stat logic:
 * - Stores and updates stat values
 * - Tracks turns and dice results
 * - Registers temporary consumable dice effects
 * - Handles effects that apply immediately or on the next available roll
 * - Provides turn-based flags for temporary effects
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

    // Active effects that apply to the current roll
    public List<BaseDiceEffect> ActiveConsumableEffects { get; private set; }
        = new List<BaseDiceEffect>();

    // Effects that should apply on the next available roll
    public List<BaseDiceEffect> PendingDiceEffects { get; private set; }
        = new List<BaseDiceEffect>();

    private ShopExitManager cachedExitManager;

    public event System.Action OnStatsChanged;

    /*
     * Temporary turn-based flags used by consumable effects.
     */
    public bool HideRollThisTurn = false;
    public bool HidePieceThisTurn = false;
    public bool HasPlayerRolledThisTurn = false;

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
     * Registers dice effects granted by a consumable item.
     * If the player already rolled this turn and the effect
     * is marked as ApplyOnNextAvailableRoll, it is stored
     * for the next turn instead of applying immediately.
     */
    public void RegisterConsumableEffects(ConsumableSO item)
    {
        if (item?.Effects == null)
            return;

        foreach (var eff in item.Effects)
        {
            if (eff is BaseDiceEffect diceEff)
            {
                // Store reference to the consumable that created this effect
                diceEff.SourceItem = item;

                // If the effect must wait for the next roll
                if (diceEff.ApplyOnNextAvailableRoll && HasPlayerRolledThisTurn)
                {
                    PendingDiceEffects.Add(diceEff);
                }
                else
                {
                    ActiveConsumableEffects.Add(diceEff);
                }
            }
        }
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
     * Stores the roll and removes effects that last only one roll.
     */
    public void OnDiceFinalResult(int finalRoll)
    {
        PreviousRoll = finalRoll;
        HasPlayerRolledThisTurn = true;

        // Remove only effects that last a single roll
        ActiveConsumableEffects.RemoveAll(e => e.RemoveAfterRoll);
    }

    /*
     * Advances the turn counter and activates pending effects.
     */
    public void NextTurn()
    {
        CurrentTurn++;

        ResetTurnFlags();

        // Move pending effects into active effects
        if (PendingDiceEffects.Count > 0)
        {
            ActiveConsumableEffects.AddRange(PendingDiceEffects);
            PendingDiceEffects.Clear();
        }

        NotifyUI();
    }

    /*
     * Resets temporary flags used by consumable effects.
     */
    public void ResetTurnFlags()
    {
        HideRollThisTurn = false;
        HidePieceThisTurn = false;
        HasPlayerRolledThisTurn = false;
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
