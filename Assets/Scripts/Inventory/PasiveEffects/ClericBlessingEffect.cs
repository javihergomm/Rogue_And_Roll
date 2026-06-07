using UnityEngine;

[CreateAssetMenu(fileName = "ClericBlessingEffect", menuName = "Effects/Passive/ClericBlessing")]
public class ClericBlessingEffect : BasePassiveEffect
{
    [SerializeField] private int turnsActive = 1; // Total turns active
    private int remaining;                        // Turns left

    public override void Activate()
    {
        // Clone so each activation has its own timer
        var clone = Instantiate(this);
        clone.remaining = clone.turnsActive;

        // Register effect
        StatManager.Instance.RegisterPassiveEffect(clone);
    }

    public override void OnTurnStart(PassiveContext ctx)
    {
        if (remaining > 0)
        {
            // Add one extra move this turn
            ctx.ExtraMoves += 1;
            remaining--;
        }
        else
        {
            // Remove when finished
            StatManager.Instance.ActivePassiveEffects.Remove(this);
        }
    }
}
