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
    public bool PreventMovement { get; set; }     // Used by BlockMovementEffect
    public int ExtraMoves { get; set; }           // Used by ExtraMoveEffect

    // Life system
    public int PlayerLives { get; set; }          // Used by ExtraLifeEffect
    public bool LifeGranted { get; set; }         // Ensures ExtraLifeEffect triggers once

    // Negative effect control
    public bool IgnoreNegativeEffect { get; set; }
}
