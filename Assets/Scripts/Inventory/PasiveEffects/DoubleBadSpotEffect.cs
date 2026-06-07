using UnityEngine;

[CreateAssetMenu(
    fileName = "DoubleBadSpotEffect",
    menuName = "Effects/Passive/DoubleBadSpot"
)]
public class DoubleBadSpotEffect : BasePassiveEffect
{
    [SerializeField] private bool permanent = true; // If true, never expires
    [SerializeField] private int durationTurns = 0; // Duration if not permanent

    private int remaining; // Turns left

    public override void Activate()
    {
        // Clone so each activation has its own timer
        var clone = Instantiate(this);

        // Set duration
        clone.remaining = permanent ? -1 : durationTurns;

        // Enable flag
        StatManager.Instance.PassiveCtx.DoubleBadSpotEffects = true;

        // Register effect
        CharacterEffectManager.Instance.AddPassiveEffect(clone);
    }

    public override void OnTurnStart()
    {
        // Permanent effect
        if (remaining == -1)
            return;

        // Still active
        if (remaining > 0)
        {
            remaining--;
            return;
        }

        // End effect
        StatManager.Instance.PassiveCtx.DoubleBadSpotEffects = false;
        CharacterEffectManager.Instance.RemovePassiveEffect(this);
    }
}
