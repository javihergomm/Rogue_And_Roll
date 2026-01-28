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
        if (!ctx.lifeGranted)
        {
            ctx.playerLives += 1;
            ctx.lifeGranted = true;
        }
    }
}
