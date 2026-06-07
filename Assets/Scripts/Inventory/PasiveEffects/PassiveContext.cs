public class PassiveContext
{
    // Blocks player movement
    public bool PreventMovement { get; set; }

    // Blocks enemy movement
    public bool PreventEnemyMovement { get; set; }

    // Extra moves for this turn
    public int ExtraMoves { get; set; }

    // Life system
    public int PlayerLives { get; set; } = 1;
    public bool LifeGranted { get; set; }
    public bool ExtraLifeUsed { get; set; }

    // Doubles bad spot effects
    public bool DoubleBadSpotEffects { get; set; }

    // Avoid bad spot every 3 turns
    public bool AvoidBadSpotEvery3TurnsActive { get; set; }
    public int AvoidBadSpotTurnCounter { get; set; }
    public bool AvoidBadSpotBoostReady { get; set; }
}
