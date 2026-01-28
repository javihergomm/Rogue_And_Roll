using UnityEngine;
using TMPro;
using System.Text;
using System.Collections;

public class ActiveDiceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI diceText;

    private void OnEnable()
    {
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return null;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnActiveDiceChanged += RefreshUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnActiveDiceChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        if (diceText == null || InventoryManager.Instance == null)
            return;

        var inv = InventoryManager.Instance;
        var slots = inv.ActiveDice.Slots;

        StringBuilder sb = new();

        foreach (var slot in slots)
        {
            if (slot == null || slot.Quantity == 0)
                continue;

            var rollInfo = DiceRollManager.Instance.GetRollInfo(slot);

            if (rollInfo.HasValue)
            {
                sb.AppendLine(slot.ItemName + ": " +
                    rollInfo.Value.baseRoll + " -> " +
                    rollInfo.Value.finalRoll);
            }
            else
            {
                sb.AppendLine(slot.ItemName + ": sin tirar");
            }
        }

        diceText.text = sb.Length == 0
            ? "Dados activos: ninguno"
            : sb.ToString();
    }
}
