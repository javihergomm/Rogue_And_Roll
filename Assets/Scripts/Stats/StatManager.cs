using UnityEngine;
using TMPro;
using System.Collections.Generic;

/*
 * StatManager
 * -----------
 * Central manager for player-related stats such as gold, rolls, and shop rerolls.
 * Stores global turn information and the last dice result.
 * Handles consumable usage, stat changes, and UI updates.
 * Provides data required by dice effects through the roll context.
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

    // Stores current and maximum values for each stat
    private Dictionary<StatType, int> currentValues = new Dictionary<StatType, int>();
    private Dictionary<StatType, int> maxValues = new Dictionary<StatType, int>();

    // Tracks consumables waiting to be removed after use
    private ConsumableSO pendingConsumable;
    private ItemSlot pendingSlot;

    // Stores the last dice result
    public int PreviousRoll { get; set; }

    // Tracks the current turn number
    public int CurrentTurn { get; private set; } = 1;

    // Active consumable effects applied to the next roll
    public List<BaseDiceEffect> ActiveConsumableEffects { get; private set; } = new List<BaseDiceEffect>();

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

    /*
     * Uses a consumable that modifies stats or roll behavior.
     * Stores the slot so the correct item instance can be removed.
     */
    public void TryUseItem(ConsumableSO item, ItemSlot slot)
    {
        Debug.Log("[DEBUG] TryUseItem CALLED for: " + item.itemName + " en slot: " + slot.name);

        pendingConsumable = item;
        pendingSlot = slot;

        // If the consumable modifies stats
        if (item.statToChange != StatType.None)
            ApplyChange(item.statToChange, item.amountToChangeStat);

        // If the consumable modifies dice rolls
        if (item.diceEffect != null)
        {
            Debug.Log("[DEBUG] Añadiendo efecto consumible: " + item.diceEffect.GetType().Name);
            ActiveConsumableEffects.Add(item.diceEffect);
        }

        ConsumePendingItem();
    }


    public void ChangeStat(StatType stat, int amount)
    {
        ApplyChange(stat, amount);
    }

    /*
     * Applies a stat change while enforcing minimum and maximum limits.
     */
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

    /*
     * Removes the consumable that was just used.
     */
    private void ConsumePendingItem()
    {
        if (pendingConsumable != null && pendingSlot != null)
        {
            InventoryManager.Instance.RemoveItem(pendingSlot, 1);
            pendingConsumable = null;
            pendingSlot = null;
        }
    }

    /*
     * Called by DiceRollManager after all effects have been applied.
     * Stores the final roll and clears temporary consumable effects.
     */
    public void OnDiceFinalResult(int finalRoll)
    {
        PreviousRoll = finalRoll;
        ActiveConsumableEffects.Clear();
    }

    /*
     * Advances the global turn counter.
     */
    public void NextTurn()
    {
        CurrentTurn++;
    }

    /*
     * Updates the UI text shown to the player.
     * All player-visible text must be in Spanish.
     */
    private void UpdateUI()
    {
        if (statsText == null) return;

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
                    sb.AppendLine("Reintentos de tienda: " + current + "/" + max);
            }
            else if (stat == StatType.Gold)
            {
                sb.AppendLine("Oro: " + current + "/" + max);
            }
        }

        statsText.text = sb.ToString();
    }

    private string GetDisplayName(StatType stat)
    {
        switch (stat)
        {
            case StatType.Gold: return "Gold";
            case StatType.Rolls: return "Rolls";
            case StatType.ShopRerolls: return "ShopRerolls";
            case StatType.None: return "None";
            default: return stat.ToString();
        }
    }

    public int GetCurrentValue(StatType stat) =>
        currentValues.ContainsKey(stat) ? currentValues[stat] : 0;

    public int GetMaxValue(StatType stat) =>
        maxValues.ContainsKey(stat) ? maxValues[stat] : int.MaxValue;

    /*
     * Consumes one shop reroll.
     */
    public void UseShopReroll()
    {
        ChangeStat(StatType.ShopRerolls, -1);
    }

    /*
     * Checks if the player is currently inside the shop.
     */
    public bool IsPlayerInShop()
    {
        var exitManager = Object.FindFirstObjectByType<ShopExitManager>();
        return exitManager != null && exitManager.IsInShop();
    }
}
