using UnityEngine;
using System.Collections.Generic;

/*
 * StatManager
 * -----------
 * Core stat logic:
 * - Stores and updates stat values
 * - Tracks turns and dice results
 * - Registers temporary consumable dice effects
 *
 * Does NOT handle UI. StatsUI.cs is responsible for presentation.
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

    public List<BaseDiceEffect> ActiveConsumableEffects { get; private set; }
        = new List<BaseDiceEffect>();

    private ShopExitManager cachedExitManager;

    public event System.Action OnStatsChanged;

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
        if (item?.Effects == null)
            return;

        foreach (var eff in item.Effects)
        {
            if (eff is BaseDiceEffect diceEff)
                ActiveConsumableEffects.Add(diceEff);
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

    public void OnDiceFinalResult(int finalRoll)
    {
        PreviousRoll = finalRoll;
        ActiveConsumableEffects.Clear();
    }

    public void NextTurn()
    {
        CurrentTurn++;
        NotifyUI();
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
