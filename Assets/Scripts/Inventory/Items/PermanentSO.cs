using UnityEngine;

/*
 * PermanentSO
 * ------------
 * Applies effects automatically when added to inventory.
 */
[CreateAssetMenu(fileName = "NewPermanent", menuName = "Inventory/Permanent")]
public class PermanentSO : BaseItemSO
{
    [Header("Effects")]
    [SerializeField] private BaseEffect[] effects;

    public BaseEffect[] Effects => effects;

    public override void UseItem()
    {
        Debug.Log("[Permanent] " + ItemName + " cannot be consumed.");
    }

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
