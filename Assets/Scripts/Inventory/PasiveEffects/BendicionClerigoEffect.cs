using UnityEngine;

[CreateAssetMenu(menuName = "Effects/BendicionClerigoEffect")]
public class BendicionClerigoEffect : BasePassiveEffect
{
    [SerializeField] private int turnsActive = 1;
    private int remaining;

    public override void Activate()
    {
        // Clonar para que cada activación tenga su propio contador independiente
        var clone = Instantiate(this);
        clone.remaining = clone.turnsActive;

        StatManager.Instance.RegisterPassiveEffect(clone);
    }

    public override void OnTurnStart(PassiveContext ctx)
    {
        if (remaining > 0)
        {
            // Añade un movimiento extra este turno
            ctx.ExtraMoves += 1;
            remaining--;
        }
        else
        {
            // Eliminar pasiva cuando termina
            StatManager.Instance.ActivePassiveEffects.Remove(this);
        }
    }
}
