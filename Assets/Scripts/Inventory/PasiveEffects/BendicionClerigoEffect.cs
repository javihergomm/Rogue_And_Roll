using UnityEngine;

/*
 * BendicionClerigoEffect
 * ----------------------
 * Passive effect that grants the player extra dice rolls per turn.
 * Each turn while active, it adds +1 to the allowed rolls for that turn.
 * When its duration expires, the effect removes itself.
 */
[CreateAssetMenu(menuName = "Effects/Passive/Bendicion del Clerigo")]
public class BendicionClerigoEffect : BasePassiveEffect
{
    [Header("Duración en turnos")]
    [SerializeField] private int turnsActive = 1;

    // Internal counter for remaining turns
    private int remaining;

    /*
     * Called when the effect is first applied.
     * Creates a runtime clone so each instance tracks its own duration.
     */
    public override void Activate()
    {
        // Create a clone so the ScriptableObject asset is not modified
        var clone = Instantiate(this);
        clone.remaining = turnsActive;

        // Register the clone as an active passive effect
        CharacterEffectManager.Instance.AddPassiveEffect(clone);
    }

    /*
     * Called at the start of each player turn.
     * Grants +1 roll this turn and decreases remaining duration.
     */
    public override void OnTurnStart()
    {
        if (remaining > 0)
        {
            // Adds +1 roll allowed this turn
            TurnManager.Instance.AddExtraRolls(1);

            remaining--;
        }
        else
        {
            // Remove the effect when duration ends
            CharacterEffectManager.Instance.RemovePassiveEffect(this);
        }
    }
}
