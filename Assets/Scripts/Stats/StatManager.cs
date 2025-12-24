using UnityEngine;
using TMPro;
using System.Collections.Generic;

/*
 * StatManager
 * -----------
 * Centralized manager for player stats such as Gold, Rolls, and ShopRerolls.
 * Responsibilities:
 * - Initialize stats with starting and maximum values
 * - Apply changes to stats while enforcing min/max limits
 * - Update UI text to reflect current values
 * - Provide helper methods to query and consume stats
 * - Consult ShopExitManager to know if the player is inside the shop
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

    // Pending consumable and its slot (needed for correct removal)
    private ConsumableSO pendingConsumable;
    private ItemSlot pendingSlot;

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
     * Called when a consumable is used.
     * Receives the slot so the correct item instance can be removed.
     */
    public void TryUseItem(ConsumableSO item, ItemSlot slot)
    {
        pendingConsumable = item;
        pendingSlot = slot;

        ApplyChange(item.statToChange, item.amountToChangeStat);
        ConsumePendingItem();
    }

    public void ChangeStat(StatType stat, int amount)
    {
        ApplyChange(stat, amount);
    }

    /*
     * Applies a stat change while enforcing limits.
     */
    private void ApplyChange(StatType stat, int amount)
    {
        if (!currentValues.ContainsKey(stat))
            currentValues[stat] = 0;

        currentValues[stat] += amount;

        // Rolls can never drop below 1
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
     * Removes the pending consumable from the correct slot.
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
                // Only show rerolls if player is inside the shop
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

    /*
     * Returns the display name for a stat (internal use only).
     * Player-visible text is handled in UpdateUI.
     */
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
