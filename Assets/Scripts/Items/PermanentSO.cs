using UnityEngine;

/*
 * PermanentSO
 * ------------
 * ScriptableObject representing a permanent inventory item.
 * Permanent items are not consumed and remain in the inventory.
 * They may provide passive effects or modify dice rolls through a diceEffect.
 */
[CreateAssetMenu(fileName = "NewPermanent", menuName = "Inventory/Permanent")]
public class PermanentSO : BaseItemSO
{
    [Header("Effects (Any Type)")]
    public BaseEffect[] effects;

    public override void UseItem()
    {
        Debug.Log("[Permanent] " + itemName + " is permanent and cannot be consumed.");
    }
}
