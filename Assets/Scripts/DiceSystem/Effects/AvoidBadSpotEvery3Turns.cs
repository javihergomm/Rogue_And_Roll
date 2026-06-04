using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Character/AvoidBadSpotEvery3Turns")]
public class AvoidBadSpotEvery3Turns : BaseDiceEffect
{
    public override bool RequiresAsyncResolution => false;

    // ============================================================
    // 1) TURN LOGIC
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
    // 2) ROLL MODIFICATION LOGIC
    // ============================================================
    public override int ModifyRoll(int roll, DiceContext diceCtx)
    {
        var ctx = StatManager.Instance.PassiveCtx;

        // No boost available
        if (!ctx.AvoidBadSpotBoostReady)
            return roll;

        // Consume boost
        ctx.AvoidBadSpotBoostReady = false;

        Movement player = DiceRollManager.Instance.GetPlayerMovement();
        if (player == null)
            return roll;

        int currentIndex = player.ActualPos;
        int total = player.Positions.Length;

        // Calculate destinations
        int normalDest = (currentIndex + roll - 1) % total + 1;
        int boostedDest = (currentIndex + roll) % total + 1;

        // Retrieve ordered spots
        Spot[] spots = FindObjectsByType<Spot>(FindObjectsInactive.Exclude);
        System.Array.Sort(spots, (a, b) => a.index.CompareTo(b.index));

        bool normalIsBad = spots[normalDest - 1].type == Spot.SpotType.Bad;
        bool boostedIsBad = spots[boostedDest - 1].type == Spot.SpotType.Bad;

        // Apply +1 only if it avoids a bad spot
        if (normalIsBad && !boostedIsBad)
            return roll + 1;

        return roll;
    }
}
