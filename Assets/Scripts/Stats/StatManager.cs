using UnityEngine;
using TMPro;
using System.Collections.Generic;

/*
 * StatManager
 * -----------
 * Manages player stats (gold, rolls, shop rerolls), turn count,
 * and temporary consumable dice effects.
 * Does NOT execute consumable effects. Consumables apply their own effects.
 */
public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI statsText;

    [Header("Gold Settings")]
    [SerializeField] private int startingGold = 0;
    [SerializeField] private int maxGold = 1000;

    [Header("Roll Settings")]
    [SerializeField] private int startingRolls = 1;

    [Header("Shop Reroll Settings")]
    [SerializeField] private int maxShopRerolls = 2;

    private Dictionary<StatType, int> currentValues = new Dictionary<StatType, int>();
    private Dictionary<StatType, int> maxValues = new Dictionary<StatType, int>();

    // Last dice result
    public int PreviousRoll { get; set; }

    // Turn counter
    public int CurrentTurn { get; private set; } = 1;

    // Temporary dice effects from consumables
    public List<BaseDiceEffect> ActiveConsumableEffects { get; private set; }
        = new List<BaseDiceEffect>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentValues[StatType.Gold] = startingGold;
        maxValues[StatType.Gold] = maxGold;

        currentValues[StatType.Rolls] = Mathf.Max(1, startingRolls);
        maxValues[StatType.Rolls] = int.MaxValue;

        currentValues[StatType.ShopRerolls] = maxShopRerolls;
        maxValues[StatType.ShopRerolls] = maxShopRerolls;

        UpdateUI();
    }

    // -------------------------------------------------------------------------
    // CONSUMABLE EFFECT REGISTRATION
    // -------------------------------------------------------------------------

    /*
     * Called by InventoryManager after a consumable is used.
     * ConsumableSO.UseItem() already executed the effect.
     * Here we only register temporary dice effects.
     */
    public void RegisterConsumableEffects(ConsumableSO item)
    {
        if (item == null || item.effects == null)
            return;

        foreach (var eff in item.effects)
        {
            if (eff is BaseDiceEffect diceEff)
                ActiveConsumableEffects.Add(diceEff);
        }
    }

    // -------------------------------------------------------------------------
    // STATS
    // -------------------------------------------------------------------------

    public void ChangeStat(StatType stat, int amount)
    {
        ApplyChange(stat, amount);
    }

    private void ApplyChange(StatType stat, int amount)
    {
        if (!currentValues.ContainsKey(stat))
            currentValues[stat] = 0;

        currentValues[stat] += amount;

        if (stat == StatType.Rolls && currentValues[stat] < 1)
            currentValues[stat] = 1;

        if (!maxValues.ContainsKey(stat))
            maxValues[stat] = int.MaxValue;

        if (currentValues[stat] > maxValues[stat])
            currentValues[stat] = maxValues[stat];

        if (currentValues[stat] < 0)
            currentValues[stat] = 0;

        UpdateUI();
    }

    // -------------------------------------------------------------------------
    // DICE RESULT
    // -------------------------------------------------------------------------

    public void OnDiceFinalResult(int finalRoll)
    {
        PreviousRoll = finalRoll;
        ActiveConsumableEffects.Clear();

        FindObjectOfType<Movement>().StartMoving();
    }

    // -------------------------------------------------------------------------
    // TURN MANAGEMENT
    // -------------------------------------------------------------------------

    public void NextTurn()
    {
        CurrentTurn++;
    }

    // -------------------------------------------------------------------------
    // UI
    // -------------------------------------------------------------------------

    private void UpdateUI()
    {
        if (statsText == null)
            return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var kvp in currentValues)
        {
            StatType stat = kvp.Key;
            int current = kvp.Value;
            int max = GetMaxValue(stat);

            if (stat == StatType.Rolls)
            {
                sb.AppendLine("Tiradas: " + current);
            }
            else if (stat == StatType.ShopRerolls)
            {
                if (IsPlayerInShop())
                    sb.AppendLine("Rotaciones de tienda: " + current + "/" + max);
            }
            else if (stat == StatType.Gold)
            {
                sb.AppendLine("Oro: " + current + "/" + max);
            }
        }

        statsText.text = sb.ToString();
    }

    public int GetCurrentValue(StatType stat)
    {
        return currentValues.ContainsKey(stat) ? currentValues[stat] : 0;
    }

    public int GetMaxValue(StatType stat)
    {
        return maxValues.ContainsKey(stat) ? maxValues[stat] : int.MaxValue;
    }

    public void UseShopReroll()
    {
        ChangeStat(StatType.ShopRerolls, -1);
    }

    public bool IsPlayerInShop()
    {
        var exitManager = Object.FindFirstObjectByType<ShopExitManager>();
        return exitManager != null && exitManager.IsInShop();
    }
}
