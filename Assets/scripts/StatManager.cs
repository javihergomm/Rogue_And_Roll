using UnityEngine;
using TMPro;
using System.Collections.Generic;

/* 
 * Serializable binding so you can connect each stat type
 * to a TextMeshProUGUI element in the Inspector.
 */
[System.Serializable]
public class StatUIBinding
{
    public ItemSO.StatType statType;       // Which stat this binding represents
    public TextMeshProUGUI statText;       // UI text element to update
}

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    [Header("UI Bindings")]
    [SerializeField] private List<StatUIBinding> statBindings;

    [Header("Starting Values")]
    [SerializeField] private int startingGold = 0;
    [SerializeField] private int maxGold = 1000;

    [SerializeField] private int startingRolls = 1; // Rolls always start at minimum 1

    private Dictionary<ItemSO.StatType, int> currentValues = new Dictionary<ItemSO.StatType, int>();
    private Dictionary<ItemSO.StatType, int> maxValues = new Dictionary<ItemSO.StatType, int>();

    private Dictionary<ItemSO.StatType, TextMeshProUGUI> statTextLookup = new Dictionary<ItemSO.StatType, TextMeshProUGUI>();

    private ItemSO pendingItem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var binding in statBindings)
        {
            if (!statTextLookup.ContainsKey(binding.statType))
                statTextLookup[binding.statType] = binding.statText;
        }

        currentValues[ItemSO.StatType.gold] = startingGold;
        maxValues[ItemSO.StatType.gold] = maxGold;

        currentValues[ItemSO.StatType.rolls] = Mathf.Max(1, startingRolls);
        maxValues[ItemSO.StatType.rolls] = int.MaxValue;

        UpdateAllUI();
    }

    public void TryUseItem(ItemSO item)
    {
        pendingItem = item;
        var stat = item.statToChange;

        if (stat == ItemSO.StatType.rolls)
        {
            ApplyChange(stat, item.amountToChangeStat);
            ConsumePendingItem();
            return;
        }

        int current = GetCurrentValue(stat);
        int max = GetMaxValue(stat);

        if (current + item.amountToChangeStat > max)
        {
            if (OptionPopupManager.Instance != null)
            {
                OptionPopupManager.Instance.ShowPopup(
                    $"Usar este objeto superará el máximo de {GetDisplayName(stat)} ({max}). ¿Quieres usarlo?",
                    new Dictionary<string, System.Action> {
                        { "Sí", () => {
                            ApplyChange(stat, item.amountToChangeStat);
                            ConsumePendingItem();
                        }},
                        { "No", () => { pendingItem = null; }}
                    }
                );
            }
            else
            {
                ApplyChange(stat, item.amountToChangeStat);
                ConsumePendingItem();
            }
        }
        else
        {
            ApplyChange(stat, item.amountToChangeStat);
            ConsumePendingItem();
        }
    }

    public void ChangeStat(ItemSO.StatType stat, int amount)
    {
        ApplyChange(stat, amount);
    }

    private void ApplyChange(ItemSO.StatType stat, int amount)
    {
        if (!currentValues.ContainsKey(stat)) currentValues[stat] = 0;

        currentValues[stat] += amount;

        if (stat == ItemSO.StatType.rolls)
        {
            if (currentValues[stat] < 1)
                currentValues[stat] = 1;

            UpdateAllUI();
            return;
        }

        if (!maxValues.ContainsKey(stat)) maxValues[stat] = int.MaxValue;

        if (currentValues[stat] > maxValues[stat]) currentValues[stat] = maxValues[stat];
        if (currentValues[stat] < 0) currentValues[stat] = 0;

        UpdateAllUI();
    }

    private void ConsumePendingItem()
    {
        if (pendingItem != null)
        {
            InventoryManager.Instance.RemoveItem(pendingItem.itemName, 1);
            pendingItem = null;
        }
    }

    private void UpdateAllUI()
    {
        foreach (var kvp in statTextLookup)
        {
            ItemSO.StatType stat = kvp.Key;
            TextMeshProUGUI text = kvp.Value;

            int current = GetCurrentValue(stat);

            if (stat == ItemSO.StatType.rolls)
            {
                text.text = $"Tiradas: {current}";
            }
            else
            {
                int max = GetMaxValue(stat);
                string displayName = GetDisplayName(stat);
                text.text = $"{displayName}: {current}/{max}";
            }
        }
    }

    private string GetDisplayName(ItemSO.StatType stat)
    {
        switch (stat)
        {
            case ItemSO.StatType.gold: return "Pesetas";
            case ItemSO.StatType.rolls: return "Tiradas";
            case ItemSO.StatType.None: return "None";
            default: return stat.ToString();
        }
    }

    public int GetCurrentValue(ItemSO.StatType stat) =>
        currentValues.ContainsKey(stat) ? currentValues[stat] : 0;

    public int GetMaxValue(ItemSO.StatType stat) =>
        maxValues.ContainsKey(stat) ? maxValues[stat] : int.MaxValue;
}
