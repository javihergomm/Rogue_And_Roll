using UnityEngine;

/*
 * ExitShieldEffect
 * ----------------
 * Se activa cuando el jugador usa el objeto Escudo de Salida.
 * Arma un escudo que anula el efecto de la próxima casilla mala
 * y después se consume.
 *
 * Comportamiento:
 * - SpotController debe comprobar si el escudo está activo.
 * - Si está activo y el jugador cae en casilla mala:
 *      · Se anula el efecto negativo.
 *      · Se consume el escudo.
 */
[CreateAssetMenu(
    fileName = "ExitShieldEffect",
    menuName = "Effects/Consumables/ExitShield"
)]
public class ExitShieldEffect : BaseConsumableEffect
{
    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null || ctx.Player == null)
            return;

        SpotController ctrl = SpotController.Instance;

        // Activar modo escudo: la próxima casilla mala será ignorada.
        ctrl.exitShieldActive = true;


        ctx.WasUsed = true;
    }
}
