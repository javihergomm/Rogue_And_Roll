using UnityEngine;

/*
 * PermanentSO
 * ------------
 * Permanent items apply their effects automatically when added
 * to the inventory, and remove them when removed.
 * They do NOT get "used" manually.
 */
[CreateAssetMenu(fileName = "NewPermanent", menuName = "Inventory/Permanent")]
public class PermanentSO : BaseItemSO
{
    [Header("Effects (Any Type)")]
    public BaseEffect[] effects;

    // Permanents are not manually used
    public override void UseItem()
    {
        Debug.Log("[Permanent] " + itemName + " is permanent and cannot be consumed.");
    }

    // Called by InventoryManager when the item is added
    public void ActivatePermanentEffects()
    {
        if (effects == null)
            return;

        foreach (var eff in effects)
        {
            if (eff == null)
                continue;

            if (eff is BaseDiceEffect diceEff)
                CharacterEffectManager.Instance.AddDiceEffect(diceEff);

            else if (eff is BasePassiveEffect passiveEff)
                CharacterEffectManager.Instance.AddPassiveEffect(passiveEff);
        }
    }

    // Called by InventoryManager when the item is removed
    public void DeactivatePermanentEffects()
    {
        if (effects == null)
            return;

        foreach (var eff in effects)
        {
            if (eff == null)
                continue;

            if (eff is BaseDiceEffect diceEff)
                CharacterEffectManager.Instance.RemoveDiceEffect(diceEff);

            else if (eff is BasePassiveEffect passiveEff)
                CharacterEffectManager.Instance.RemovePassiveEffect(passiveEff);
        }
    }
}
