using UnityEngine;

/*
 * ExtraLifeEffect
 * ---------------
 * Grants the player one extra life.
 */
[CreateAssetMenu(fileName = "ExtraLifeEffect", menuName = "Effects/Passive/ExtraLife")]
public class ExtraLifeEffect : BasePassiveEffect
{
    public override void OnTurnStart(PassiveContext ctx)
    {
        if (!ctx.LifeGranted)
        {
            ctx.PlayerLives += 1;
            ctx.LifeGranted = true;
        }
    }
}
