using UnityEngine;

/*
 * InventoryPermanentEffects
 * -------------------------
 * Activates and deactivates permanent effects when items are added/removed.
 */
[System.Serializable]
public class InventoryPermanentEffects
{
    public void TryActivate(BaseItemSO item)
    {
        if (item is not PermanentSO perm || perm.Effects == null)
            return;

        foreach (var eff in perm.Effects)
        {
            // Dice effects (these go to CharacterEffectManager)
            if (eff is BaseDiceEffect diceEff)
            {
                CharacterEffectManager.Instance.AddDiceEffect(diceEff);
            }
            // Passive effects (these MUST go to StatManager)
            else if (eff is BasePassiveEffect passiveEff)
            {
                if (!StatManager.Instance.ActivePassiveEffects.Contains(passiveEff))
                {
                    StatManager.Instance.ActivePassiveEffects.Add(passiveEff);
                    Debug.Log("[PermanentEffects] Activado efecto pasivo: " + passiveEff.name);
                }
            }
        }
    }

    public void TryDeactivate(BaseItemSO item)
    {
        if (item is not PermanentSO perm || perm.Effects == null)
            return;

        foreach (var eff in perm.Effects)
        {
            // Dice effects
            if (eff is BaseDiceEffect diceEff)
            {
                CharacterEffectManager.Instance.RemoveDiceEffect(diceEff);
            }
            // Passive effects
            else if (eff is BasePassiveEffect passiveEff)
            {
                if (StatManager.Instance.ActivePassiveEffects.Contains(passiveEff))
                {
                    StatManager.Instance.ActivePassiveEffects.Remove(passiveEff);
                    Debug.Log("[PermanentEffects] Desactivado efecto pasivo: " + passiveEff.name);
                }
            }
        }
    }
}
