using UnityEngine;

/*
 * CharacterEffectApplier
 * ----------------------
 * Applies and removes character specific effects when a character
 * is activated or deactivated. Effects are registered in the
 * CharacterEffectManager so they remain active during gameplay.
 */
public static class CharacterEffectApplier
{
    /*
     * ApplyEffects
     * ------------
     * Registers all effects defined in the CharacterSO.
     * Dice effects and passive effects are stored separately.
     */
    public static void ApplyEffects(CharacterSO character)
    {
        if (character == null || character.effects == null)
            return;

        foreach (var eff in character.effects)
        {
            if (eff == null)
                continue;

            // Dice related effects (modify rolls)
            if (eff is BaseDiceEffect diceEff)
                CharacterEffectManager.Instance.AddDiceEffect(diceEff);

            // Passive effects (modify stats, behavior, etc.)
            else if (eff is BasePassiveEffect passiveEff)
                CharacterEffectManager.Instance.AddPassiveEffect(passiveEff);
        }
    }

    /*
     * RemoveEffects
     * -------------
     * Unregisters all effects previously applied by this character.
     * Called when switching characters or resetting the game state.
     */
    public static void RemoveEffects(CharacterSO character)
    {
        if (character == null || character.effects == null)
            return;

        foreach (var eff in character.effects)
        {
            if (eff == null)
                continue;

            // Remove dice effects
            if (eff is BaseDiceEffect diceEff)
                CharacterEffectManager.Instance.RemoveDiceEffect(diceEff);

            // Remove passive effects
            else if (eff is BasePassiveEffect passiveEff)
                CharacterEffectManager.Instance.RemovePassiveEffect(passiveEff);
        }
    }
}
