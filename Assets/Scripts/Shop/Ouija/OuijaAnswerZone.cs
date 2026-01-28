using UnityEngine;

/*
 * OuijaAnswerZone
 * ---------------
 * Detects when the player enters a specific zone on the ouija board.
 * Zones can represent Yes, No, or Goodbye.
 * Responsibilities:
 * - Handle buy/sell pedestal decisions (Yes/No)
 * - Trigger shop exit confirmation popup when Goodbye zone is entered
 */
public class OuijaAnswerZone : MonoBehaviour
{
    public enum AnswerType { Yes, No, Goodbye }

    [SerializeField] private AnswerType answerType;

    private ShopExitManager exitManager;

    private void Awake()
    {
        // Cache reference to avoid repeated scene lookups
        exitManager = FindFirstObjectByType<ShopExitManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Buy pedestal decision
        var buyPedestal = ShopPedestalRandomizer.currentPedestal;
        if (buyPedestal != null && buyPedestal.isAwaitingDecision)
        {
            buyPedestal.HandleOuijaAnswer(answerType);
            return;
        }

        // Sell pedestal decision
        var sellPedestal = SellPedestal.currentSellPedestal;
        if (sellPedestal != null && sellPedestal.isAwaitingDecision)
        {
            sellPedestal.HandleOuijaAnswer(answerType);
            return;
        }

        // Goodbye zone
        if (answerType == AnswerType.Goodbye)
        {
            if (exitManager != null)
                exitManager.TriggerGoodbye();
            return;
        }
    }
}
