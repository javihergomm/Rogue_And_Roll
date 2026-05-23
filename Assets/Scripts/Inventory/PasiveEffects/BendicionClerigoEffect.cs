using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Passive/BendicionClerigo")]
public class BendicionClerigoEffect : BasePassiveEffect
{
    [SerializeField] private int turnsActive = 1;
    private int remaining;

    public override void Activate()
    {
        // Crear instancia independiente
        var clone = Instantiate(this);
        clone.remaining = turnsActive;

        // Registrar en el sistema de efectos
        CharacterEffectManager.Instance.AddPassiveEffect(clone);
    }

    public override void OnTurnStart()
    {
        if (remaining > 0)
        {
            // Añadir movimiento extra este turno
            StatManager.Instance.PassiveCtx.ExtraMoves += 1;
            remaining--;
        }
        else
        {
            // Eliminar la pasiva cuando termina
            CharacterEffectManager.Instance.RemovePassiveEffect(this);
        }
    }
}
