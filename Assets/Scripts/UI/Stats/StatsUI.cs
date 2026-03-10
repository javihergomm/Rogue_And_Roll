using UnityEngine;
using TMPro;
using System.Text;
using System.Collections;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;

    private void OnEnable()
    {
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return null;

        if (StatManager.Instance != null)
            StatManager.Instance.OnStatsChanged += RefreshUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (StatManager.Instance != null)
            StatManager.Instance.OnStatsChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        if (statsText == null || StatManager.Instance == null)
            return;

        var sm = StatManager.Instance;
        StringBuilder sb = new();

        int rolls = sm.GetCurrentValue(StatType.Rolls);
        sb.AppendLine("Tiradas: " + rolls);

        int gold = sm.GetCurrentValue(StatType.Gold);
        int maxGold = sm.GetMaxValue(StatType.Gold);
        sb.AppendLine("Pesetas: " + gold + "/" + maxGold);

        if (sm.IsPlayerInShop())
        {
            int rerolls = sm.GetCurrentValue(StatType.ShopRerolls);
            int maxRerolls = sm.GetMaxValue(StatType.ShopRerolls);
            sb.AppendLine("Rotaciones de tienda: " + rerolls + "/" + maxRerolls);
        }

        statsText.text = sb.ToString();
    }
}
