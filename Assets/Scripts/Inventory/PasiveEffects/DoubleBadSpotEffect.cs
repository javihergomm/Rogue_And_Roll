using UnityEngine;

[CreateAssetMenu(
    fileName = "DoubleBadSpotEffect",
    menuName = "Effects/Passive/DoubleBadSpot"
)]
public class DoubleBadSpotEffect : BasePassiveEffect
{
    public override void OnTurnStart(PassiveContext ctx)
    {
        // Activar el flag cada turno mientras el objeto este equipado
        ctx.DoubleBadSpotEffects = true;

        
    }
}
