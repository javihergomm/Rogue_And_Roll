using UnityEngine;
using System.Collections.Generic;

/*
 * StatManager
 * -----------
 * Central system for managing:
 *   - Player stats (gold, rolls, rerolls)
 *   - Passive effects
 *   - Consumable effects
 *   - Turn progression
 *   - Movement-blocking flags
 *
 * This class exposes events so UI elements can update when stats change.
 */
public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    [Header("Gold Settings")]
    [SerializeField] private int startingGold = 0;
    [SerializeField] private int maxGold = 100;

    [Header("Roll Settings")]
    [SerializeField] private int startingRolls = 1;

    [Header("Shop Reroll Settings")]
    [SerializeField] private int maxShopRerolls = 100;

    // Current and maximum values for each stat
    private readonly Dictionary<StatType, int> currentValues = new();
    private readonly Dictionary<StatType, int> maxValues = new();

    public int PreviousRoll { get; private set; }
    public int CurrentTurn { get; private set; } = 1;

    // Active effects applied by consumables
    public List<BaseDiceEffect> ActiveConsumableEffects { get; private set; }
        = new List<BaseDiceEffect>();

    // Effects that will be applied next turn
    public List<BaseDiceEffect> PendingDiceEffects { get; private set; }
        = new List<BaseDiceEffect>();

    // Active passive effects
    public List<BasePassiveEffect> ActivePassiveEffects { get; private set; }
        = new List<BasePassiveEffect>();

    private ShopExitManager cachedExitManager;

    // Event fired whenever stats change
    public event System.Action OnStatsChanged;

    // Turn flags
    public bool HideRollThisTurn = false;
    public bool HidePieceThisTurn = false;
    public bool HasPlayerRolledThisTurn = false;

    public bool PreventMovementThisTurn = false;
    public bool PreventEnemyMovementThisTurn = false;

    // Context used by passive effects to modify turn behavior
    public PassiveContext PassiveCtx { get; private set; } = new PassiveContext();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        cachedExitManager = FindAnyObjectByType<ShopExitManager>();

        InitializeStats();

        // ---------------------------------------------------------
        // Reset full state when entering the game scene
        // ---------------------------------------------------------
        CurrentTurn = 1;
        PreviousRoll = 0;

        ActiveConsumableEffects.Clear();
        PendingDiceEffects.Clear();
        ActivePassiveEffects.Clear();

        ResetTurnFlags();
        PassiveCtx = new PassiveContext();
        // ---------------------------------------------------------

        NotifyUI();
    }

    private void Start()
    {
        var exit = FindAnyObjectByType<ShopExitManager>();
        if (exit != null)
            exit.OnShopStateChanged += HandleShopStateChanged;

        NotifyUI();
    }

    /*
     * Initializes all stat values at the start of the game.
     */
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
     * Registers all effects contained in a consumable item.
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
     * Registers a passive effect if it is not already active.
     */
    public void RegisterPassiveEffect(BasePassiveEffect effect)
    {
        if (!ActivePassiveEffects.Contains(effect))
            ActivePassiveEffects.Add(effect);
    }

    /*
     * Modifies a stat and clamps it to valid limits.
     * Triggers UI update after the change.
     */
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

    /*
     * Returns the current value of a stat.
     */
    public int GetCurrentValue(StatType stat)
    {
        return currentValues.TryGetValue(stat, out int value) ? value : 0;
    }

    /*
     * Returns the maximum allowed value of a stat.
     */
    public int GetMaxValue(StatType stat)
    {
        return maxValues.TryGetValue(stat, out int value) ? value : int.MaxValue;
    }

    /*
     * Decreases the number of shop rerolls.
     */
    public void UseShopReroll()
    {
        ChangeStat(StatType.ShopRerolls, -1);
    }

    /*
     * Called when the final dice result is known.
     */
    public void OnDiceFinalResult(int finalRoll)
    {
        PreviousRoll = finalRoll;
        HasPlayerRolledThisTurn = true;

        ActiveConsumableEffects.RemoveAll(e => e.RemoveAfterRoll);
    }

    /*
     * Advances to the next turn and processes passive effects.
     */
    public void NextTurn()
    {
        CurrentTurn++;

        PassiveCtx.PreventMovement = false;
        PassiveCtx.PreventEnemyMovement = false;

        foreach (var eff in new List<BasePassiveEffect>(ActivePassiveEffects))
        {
            eff.OnTurnStart(PassiveCtx);
        }

        ResetTurnFlags();

        PreventMovementThisTurn = PassiveCtx.PreventMovement;
        PreventEnemyMovementThisTurn = PassiveCtx.PreventEnemyMovement;

        if (PendingDiceEffects.Count > 0)
        {
            ActiveConsumableEffects.AddRange(PendingDiceEffects);
            PendingDiceEffects.Clear();
        }

        NotifyUI();
    }

    /*
     * Resets all per-turn flags.
     */
    public void ResetTurnFlags()
    {
        HideRollThisTurn = false;
        HidePieceThisTurn = false;
        HasPlayerRolledThisTurn = false;

        PreventMovementThisTurn = false;
        PreventEnemyMovementThisTurn = false;
    }

    /*
     * Called when the player enters or exits the shop.
     */
    private void HandleShopStateChanged(bool inShop)
    {
        NotifyUI();
    }

    /*
     * Returns true if the player is currently inside the shop.
     */
    public bool IsPlayerInShop()
    {
        return cachedExitManager != null && cachedExitManager.IsInShop();
    }

    /*
     * Notifies all listeners that stats have changed.
     */
    private void NotifyUI()
    {
        OnStatsChanged?.Invoke();
    }

    /*
     * Public method for forcing a UI refresh.
     */
    public void TriggerStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }
}
