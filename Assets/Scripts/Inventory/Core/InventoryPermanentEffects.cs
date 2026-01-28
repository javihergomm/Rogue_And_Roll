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
            if (eff is BaseDiceEffect diceEff)
                CharacterEffectManager.Instance.AddDiceEffect(diceEff);

            else if (eff is BasePassiveEffect passiveEff)
                CharacterEffectManager.Instance.AddPassiveEffect(passiveEff);
        }
    }

    public void TryDeactivate(BaseItemSO item)
    {
        if (item is not PermanentSO perm || perm.Effects == null)
            return;

        foreach (var eff in perm.Effects)
        {
            if (eff is BaseDiceEffect diceEff)
                CharacterEffectManager.Instance.RemoveDiceEffect(diceEff);

            else if (eff is BasePassiveEffect passiveEff)
                CharacterEffectManager.Instance.RemovePassiveEffect(passiveEff);
        }
    }
}

