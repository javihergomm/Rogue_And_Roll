using UnityEngine;

/*
 * CharacterSO (Cup Character)
 * ---------------------------
 * Characters apply their own effects when activated.
 * CharacterEffectManager only stores the effects; it does not apply them.
 */
[CreateAssetMenu(fileName = "NewCupCharacter", menuName = "Game/Cup Character")]
public class CharacterSO : ScriptableObject
{
    [Header("Identity")]
    public string characterID;
    public string characterName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Visuals")]
    public Color characterColor = Color.white;

    [Header("Spawn")]
    public string spawnPointName;

    [Header("Effects")]
    [Tooltip("All effects applied while this cup is active (dice, passive, etc.).")]
    public BaseEffect[] effects;

    [Header("Special Cup Behaviors")]
    public bool isBasicCup;
    public bool hasRandomBonus;
    public bool avoidsBadTileEvery3;
    public bool isMetalCup;

    /*
     * Called when this character becomes active.
     * Registers all dice and passive effects in CharacterEffectManager.
     */
    public void ApplyCharacterEffects()
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

    /*
     * Called when switching characters.
     * Removes all previously applied effects.
     */
    public void RemoveCharacterEffects()
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
