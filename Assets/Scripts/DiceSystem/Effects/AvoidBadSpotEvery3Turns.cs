using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Character/AvoidBadSpotEvery3Turns")]
public class AvoidBadSpotEvery3Turns : BaseDiceEffect
{
    public override bool RequiresAsyncResolution => false;

    // ============================================================
    // 1) LÓGICA DE TURNO (gracias a tu nueva arquitectura)
    // ============================================================
    public override void OnTurnStart()
    {
        var ctx = StatManager.Instance.PassiveCtx;

        if (!ctx.AvoidBadSpotEvery3TurnsActive)
            return;

        ctx.AvoidBadSpotTurnCounter++;

        if (ctx.AvoidBadSpotTurnCounter >= 3)
        {
            ctx.AvoidBadSpotTurnCounter = 0;
            ctx.AvoidBadSpotBoostReady = true;
        }
    }

    // ============================================================
    // 2) LÓGICA DE TIRADA (BaseDiceEffect)
    // ============================================================
    public override int ModifyRoll(int roll, DiceContext diceCtx)
    {
        var ctx = StatManager.Instance.PassiveCtx;

        // Si no hay boost, no hacemos nada
        if (!ctx.AvoidBadSpotBoostReady)
            return roll;

        // Consumimos el boost SIEMPRE
        ctx.AvoidBadSpotBoostReady = false;

        Movement player = DiceRollManager.Instance.GetPlayerMovement();
        if (player == null)
            return roll;

        int currentIndex = player.ActualPos;
        int total = player.Positions.Length;

        // Calcular destinos
        int normalDest = (currentIndex + roll - 1) % total + 1;
        int boostedDest = (currentIndex + roll) % total + 1;

        // Obtener spots reales
        Spot[] spots = FindObjectsByType<Spot>(FindObjectsSortMode.None);
        System.Array.Sort(spots, (a, b) => a.index.CompareTo(b.index));

        bool normalIsBad = spots[normalDest - 1].type == Spot.SpotType.Bad;
        bool boostedIsBad = spots[boostedDest - 1].type == Spot.SpotType.Bad;

        // Si el +1 evita casilla mala -> aplicamos boost
        if (normalIsBad && !boostedIsBad)
            return roll + 1;

        return roll;
    }
}
