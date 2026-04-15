using UnityEngine;

[CreateAssetMenu(fileName = "ClericBlessingEffect", menuName = "Effects/Passive/ClericBlessing")]
public class ClericBlessingEffect : BasePassiveEffect
{
    [SerializeField] private int turnsActive = 1;
    private int remaining;

    public override void Activate()
    {
        var clone = Instantiate(this);
        clone.remaining = clone.turnsActive;

        StatManager.Instance.RegisterPassiveEffect(clone);
    }

    public override void OnTurnStart(PassiveContext ctx)
    {
        if (remaining > 0)
        {
            // Permite un movimiento extra este turno
            ctx.ExtraMoves += 1;
            remaining--;
        }
        else
        {
            // El efecto termina
            StatManager.Instance.ActivePassiveEffects.Remove(this);
        }
    }
}
