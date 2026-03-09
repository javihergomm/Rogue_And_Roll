/*
 * PassiveContext
 * --------------
 * Context passed to passive effects each turn.
 * Contains only the data needed by passive effects that do not depend on tiles.
 */
public class PassiveContext
{
    // Turn info
    public int TurnNumber { get; set; }
    public int TilesMovedThisTurn { get; set; }

    // Movement control
    public bool PreventMovement { get; set; }
    public int ExtraMoves { get; set; }

    // Life system
    public int PlayerLives { get; set; } = 1;     // Base life
    public bool LifeGranted { get; set; }         // ExtraLifeEffect triggers once
    public bool ExtraLifeUsed { get; set; }       // True once consumed

    // Negative effect control
    public bool IgnoreNegativeEffect { get; set; }
}
