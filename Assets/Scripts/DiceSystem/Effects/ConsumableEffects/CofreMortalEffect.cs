using UnityEngine;

[CreateAssetMenu(menuName = "Effects/CofreMortalEffect")]
public class CofreMortalEffect : BaseConsumableEffect
{
    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null || ctx.Player == null)
        {
            Debug.Log("Cofre Mortal: contexto inválido.");
            return;
        }

        int startIndex = ctx.Player.ActualPos;

        // Buscar hacia delante hasta que no existan más casillas
        for (int i = startIndex + 1; ; i++)
        {
            Spot spot = SpotController.Instance.GetSpotByIndex(i);

            if (spot == null)
                break; // No hay más casillas

            if (spot.GetSpotType() == Spot.SpotType.Normal)
            {
                spot.AssignType(Spot.SpotType.Bad);

                Debug.Log("Cofre Mortal: La casilla " + i + " ha sido convertida en casilla mala permanentemente.");

                ctx.WasUsed = true;
                return;
            }
        }

        Debug.Log("Cofre Mortal: No se encontró ninguna casilla normal hacia delante.");
    }
}
