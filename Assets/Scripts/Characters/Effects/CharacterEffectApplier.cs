using UnityEngine;

public static class CharacterEffectApplier
{
    public static void ApplyEffects(CharacterSO character)
    {
        if (character == null || character.effects == null)
            return;

        foreach (var eff in character.effects)
        {
            if (eff == null)
                continue;

            if (eff is BaseDiceEffect diceEff)
                CharacterEffectManager.Instance.AddDiceEffect(diceEff);

            else if (eff is BasePassiveEffect passiveEff)
                CharacterEffectManager.Instance.AddPassiveEffect(passiveEff);
        }
    }

    public static void RemoveEffects(CharacterSO character)
    {
        if (character == null || character.effects == null)
            return;

        foreach (var eff in character.effects)
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
