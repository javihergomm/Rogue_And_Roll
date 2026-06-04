using UnityEngine;

public class ShopPurchaseHook : MonoBehaviour
{
    private bool alreadyDebugged = false;

    private void Update()
    {
        var ped = ShopPedestalRandomizer.currentPedestal;

        if (ped == null)
        {
            alreadyDebugged = false;
            return;
        }

        if (!ped.isAwaitingDecision)
        {
            alreadyDebugged = false;
            return;
        }

        if (alreadyDebugged)
            return;

        var item = ped.GetChosenItem();

        if (item != null)
        {
            InventoryPurchaseDebugger.DebugPurchase(item);
            alreadyDebugged = true;
        }
    }
}
