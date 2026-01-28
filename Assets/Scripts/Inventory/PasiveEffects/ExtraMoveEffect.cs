using UnityEngine;

/*
 * ExtraMoveEffect
 * ---------------
 * Grants 1 extra movement per turn for a number of turns.
 */
[CreateAssetMenu(fileName = "ExtraMoveEffect", menuName = "Effects/Passive/ExtraMove")]
public class ExtraMoveEffect : BasePassiveEffect
{
    [SerializeField] private int turns = 2;
    private int remaining;

    public void Activate()
    {
        remaining = turns;
    }

    public override void OnTurnStart(PassiveContext ctx)
    {
        if (remaining > 0)
        {
            ctx.ExtraMoves += 1;
            remaining--;
        }
    }
}
