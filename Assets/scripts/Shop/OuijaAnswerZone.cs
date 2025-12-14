using UnityEngine;

public class OuijaAnswerZone : MonoBehaviour
{
    public enum AnswerType { Yes, No }
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

        Debug.Log("No pedestal active or no decision pending.");
    }
}
