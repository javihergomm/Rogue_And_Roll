/*
 * PassiveContext
 * --------------
 * Context passed to passive effects each turn.
 * Only contains what is needed for effects that do NOT depend on casillas.
 */
public class PassiveContext
{
    public int turnNumber;
    public int tilesMovedThisTurn;

    // Movement control
    public bool preventMovement;   // Used by BlockMovementEffect
    public int extraMoves;         // Used by ExtraMoveEffect

    // Life system
    public int playerLives;        // Used by ExtraLifeEffect
    public bool lifeGranted;       // Ensures ExtraLifeEffect only triggers once

    public bool ignoreNegativeEffect;
}
