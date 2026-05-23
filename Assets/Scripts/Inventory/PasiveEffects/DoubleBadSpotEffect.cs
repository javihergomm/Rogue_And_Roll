using UnityEngine;

[CreateAssetMenu(
    fileName = "DoubleBadSpotEffect",
    menuName = "Effects/Passive/DoubleBadSpot"
)]
public class DoubleBadSpotEffect : BasePassiveEffect
{
    [SerializeField] private bool permanent = true;
    [SerializeField] private int durationTurns = 0;

    private int remaining;

    public override void Activate()
    {
        // Crear instancia independiente
        var clone = Instantiate(this);

        // Configurar duración
        clone.remaining = permanent ? -1 : durationTurns;

        // Activar flag
        StatManager.Instance.PassiveCtx.DoubleBadSpotEffects = true;

        // Registrar en CharacterEffectManager
        CharacterEffectManager.Instance.AddPassiveEffect(clone);
    }

    public override void OnTurnStart()
    {
        // Permanente -> no expira nunca
        if (remaining == -1)
            return;

        // Todavía quedan turnos
        if (remaining > 0)
        {
            remaining--;
            return;
        }

        // Expiró -> desactivar flag
        StatManager.Instance.PassiveCtx.DoubleBadSpotEffects = false;

        // Eliminar efecto
        CharacterEffectManager.Instance.RemovePassiveEffect(this);
    }
}
