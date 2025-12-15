using UnityEngine;

/*
 * PermanentSO
 * ------------
 * Specialized ScriptableObject for permanent items.
 * These items are not consumed and usually provide passive effects.
 */
[CreateAssetMenu(fileName = "NewPermanent", menuName = "Inventory/Permanent")]
public class PermanentSO : BaseItemSO
{
    public override void UseItem()
    {
        Debug.Log("[Permanent] " + itemName + " is permanent. It is not consumed.");
        // Future: apply passive buffs or unlock abilities here
    }
}
