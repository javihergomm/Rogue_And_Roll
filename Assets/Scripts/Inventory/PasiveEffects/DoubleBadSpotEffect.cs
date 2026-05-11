using UnityEngine;

[CreateAssetMenu(
    fileName = "DoubleBadSpotEffect",
    menuName = "Effects/Passive/DoubleBadSpot"
)]
public class DoubleBadSpotEffect : BasePassiveEffect
{
    public override void OnTurnStart(PassiveContext ctx)
    {
      
        ctx.DoubleBadSpotEffects = true;
        
    }
}
