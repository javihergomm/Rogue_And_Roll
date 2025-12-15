using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI statsText;

    [SerializeField] private int startingGold = 0;
    [SerializeField] private int maxGold = 1000;
    [SerializeField] private int startingRolls = 1;

    private Dictionary<StatType, int> currentValues = new Dictionary<StatType, int>();
    private Dictionary<StatType, int> maxValues = new Dictionary<StatType, int>();

    private ConsumableSO pendingConsumable;

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

        UpdateUI();
    }

    public void TryUseItem(ConsumableSO item)
    {
        pendingConsumable = item;
        ApplyChange(item.statToChange, item.amountToChangeStat);
        ConsumePendingItem();
    }

    public void ChangeStat(StatType stat, int amount)
    {
        ApplyChange(stat, amount);
    }

    private void ApplyChange(StatType stat, int amount)
    {
        if (!currentValues.ContainsKey(stat)) currentValues[stat] = 0;
        currentValues[stat] += amount;

        if (stat == StatType.Rolls && currentValues[stat] < 1)
            currentValues[stat] = 1;

        if (!maxValues.ContainsKey(stat)) maxValues[stat] = int.MaxValue;
        if (currentValues[stat] > maxValues[stat]) currentValues[stat] = maxValues[stat];
        if (currentValues[stat] < 0) currentValues[stat] = 0;

        UpdateUI();
    }

    private void ConsumePendingItem()
    {
        if (pendingConsumable != null)
        {
            InventoryManager.Instance.RemoveItem(pendingConsumable.itemName, 1);
            pendingConsumable = null;
        }
    }

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
                sb.AppendLine("Tiradas: " + current);
            else
                sb.AppendLine(GetDisplayName(stat) + ": " + current + "/" + max);
        }
        statsText.text = sb.ToString();
    }

    private string GetDisplayName(StatType stat)
    {
        switch (stat)
        {
            case StatType.Gold: return "Pesetas";
            case StatType.Rolls: return "Tiradas";
            case StatType.None: return "None";
            default: return stat.ToString();
        }
    }

    public int GetCurrentValue(StatType stat) =>
        currentValues.ContainsKey(stat) ? currentValues[stat] : 0;

    public int GetMaxValue(StatType stat) =>
        maxValues.ContainsKey(stat) ? maxValues[stat] : int.MaxValue;
}
