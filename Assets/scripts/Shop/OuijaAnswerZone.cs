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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered: " + other.name);
        if (!other.CompareTag("Player")) return;

        // Check buy pedestal first
        var buyPedestal = ShopPedestalRandomizer.currentPedestal;
        if (buyPedestal != null && buyPedestal.isAwaitingDecision)
        {
            Debug.Log("Calling HandleOuijaAnswer (BUY): " + answerType);
            buyPedestal.HandleOuijaAnswer(answerType);
            return;
        }

        // Check sell pedestal
        var sellPedestal = SellPedestal.currentSellPedestal;
        if (sellPedestal != null && sellPedestal.isAwaitingDecision)
        {
            Debug.Log("Calling HandleOuijaAnswer (SELL): " + answerType);
            sellPedestal.HandleOuijaAnswer(answerType);
            return;
        }

        // Handle Goodbye zone
        if (answerType == AnswerType.Goodbye)
        {
            Debug.Log("Player triggered Goodbye zone, showing confirmation popup...");
            var exitManager = Object.FindFirstObjectByType<ShopExitManager>();
            if (exitManager != null)
            {
                // Show confirmation popup instead of exiting immediately
                exitManager.TriggerGoodbye();
            }
            return;
        }

        Debug.Log("No pedestal active or no decision pending.");
    }
}
