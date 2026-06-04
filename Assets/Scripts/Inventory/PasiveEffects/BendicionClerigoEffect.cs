using UnityEngine;

/*
 * BendicionClerigoEffect
 * ----------------------
 * Passive effect that grants the player +1 extra roll per turn.
 * The effect lasts a configurable number of turns.
 * Each turn:
 *   - Adds +1 to the allowed rolls for that turn
 *   - Decreases remaining duration
 * When duration reaches zero, the effect removes itself.
 */
[CreateAssetMenu(menuName = "Effects/Passive/Bendicion del Clerigo")]
public class BendicionClerigoEffect : BasePassiveEffect
{
    [Header("Duration in turns")]
    [SerializeField] private int turnsActive = 1;

    // Runtime counter for remaining turns
    private int remaining;

    /*
     * Called when the effect is first applied.
     * Creates a runtime clone so each instance tracks its own duration.
     */
    public override void Activate()
    {
        var clone = Instantiate(this);
        clone.remaining = turnsActive;

        CharacterEffectManager.Instance.AddPassiveEffect(clone);

        // Update UI after registering the effect
        StatManager.Instance.TriggerStatsChanged();
    }

    /*
     * Called at the start of each player turn.
     * Grants +1 roll and decreases remaining duration.
     */
    public override void OnTurnStart()
    {
        if (remaining > 0)
        {
            TurnManager.Instance.AddExtraRolls(1);
            remaining--;

            // Update UI so the new roll count is visible
            StatManager.Instance.TriggerStatsChanged();
        }
        else
        {
            CharacterEffectManager.Instance.RemovePassiveEffect(this);

            // Update UI after removal
            StatManager.Instance.TriggerStatsChanged();
        }
    }
}
