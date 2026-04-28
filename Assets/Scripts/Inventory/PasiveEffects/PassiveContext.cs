public class PassiveContext
{
    public int TurnNumber { get; set; }
    public int TilesMovedThisTurn { get; set; }

    public bool PreventMovement { get; set; }         
    public bool PreventEnemyMovement { get; set; }    
    public int ExtraMoves { get; set; }

    public int PlayerLives { get; set; } = 1;
    public bool LifeGranted { get; set; }
    public bool ExtraLifeUsed { get; set; }
    public bool IgnoreNegativeEffect { get; set; }
    public bool DoubleBadSpotEffects { get; set; }
}
