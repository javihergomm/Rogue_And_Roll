using UnityEngine;
using TMPro;
using System.Text;
using System.Collections;

public class ActiveDiceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;

    private void OnEnable()
    {
        StartCoroutine(DelayedInit());

        TurnManager.OnPlayerTurnStarted += HandlePlayerTurn;
        TurnManager.OnEnemyTurnStarted += HandleEnemyTurn;
        TurnManager.OnEnemyRollCalculated += HandleEnemyRoll;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnActiveDiceChanged -= RefreshPlayerDice;

        TurnManager.OnPlayerTurnStarted -= HandlePlayerTurn;
        TurnManager.OnEnemyTurnStarted -= HandleEnemyTurn;
        TurnManager.OnEnemyRollCalculated -= HandleEnemyRoll;
    }

    private IEnumerator DelayedInit()
    {
        yield return null;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnActiveDiceChanged += RefreshPlayerDice;

        RefreshPlayerDice();
    }

    // -------------------------------------------------------------------------
    // PLAYER TURN
    // -------------------------------------------------------------------------
    private void HandlePlayerTurn()
    {
        RefreshPlayerDice();
    }

    private void RefreshPlayerDice()
    {
        if (statusText == null || InventoryManager.Instance == null)
            return;

        var inv = InventoryManager.Instance;
        var slots = inv.ActiveDice.Slots;

        StringBuilder sb = new();
        sb.AppendLine("Turno del jugador");
        sb.AppendLine("Dados activos:");

        foreach (var slot in slots)
        {
            if (slot == null || slot.Quantity == 0)
                continue;

            var rollInfo = DiceRollManager.Instance.GetRollInfo(slot);

            if (StatManager.Instance.HideRollThisTurn)
            {
                sb.AppendLine("- " + slot.ItemName + ": ???");
                continue;
            }

            if (rollInfo.HasValue)
            {
                sb.AppendLine("- " + slot.ItemName + ": " +
                    rollInfo.Value.baseRoll + " -> " +
                    rollInfo.Value.finalRoll);
            }
            else
            {
                sb.AppendLine("- " + slot.ItemName + ": sin tirar");
            }
        }

        statusText.text = sb.ToString();
    }

    // -------------------------------------------------------------------------
    // ENEMY TURN
    // -------------------------------------------------------------------------
    private void HandleEnemyTurn()
    {
        if (statusText != null)
            statusText.text = "Turno del enemigo...";
    }

    private void HandleEnemyRoll(int total)
    {
        if (statusText != null)
            statusText.text = "Turno del enemigo: se movera " + total + " casillas";
    }
}
