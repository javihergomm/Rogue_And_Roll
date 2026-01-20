public class PassiveContext
{
    // Turn information
    public int turnNumber;
    public int tilesMovedThisTurn;

    // Tile interaction
    public Tile tile;                 // Tile the player is entering / revealing
    public bool ignoreNegativeEffect; // Passive effects can set this to true
    public bool amplifyDangerEffect;  // For effects like Linterna Potenciada

    // Player reference (optional but extremely useful)
    public Player player;
}
