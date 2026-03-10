using UnityEngine;

/*
 * EnemyBase
 * ---------
 * Abstract base class for all enemy types.
 * Provides:
 * - Shared enemy data reference
 * - Spawn logic for logic-only enemy objects
 * - Positioning helpers for placing enemies on the board
 * - Cached references to Movement and player
 * - Shared kill logic used by all enemies
 * - Movement blocking support through StatManager flags
 */
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemySO data;

    // Movement component for the enemy token
    [HideInInspector] public Movement movement;

    // Reference to the player transform
    [HideInInspector] public Transform player;

    // Cached player movement to avoid repeated GetComponent calls
    protected Movement playerMovement;

    // Visual instance for enemies that spawn a token
    public GameObject CupInstance { get; protected set; }

    // Whether the enemy is active and should take turns
    protected bool isActive = false;

    /*
     * SpawnEnemy
     * ----------
     * Logic-only spawn point. Visuals are created by each boss.
     */
    public void SpawnEnemy()
    {
        // Intentionally empty: visual instantiation is handled by each boss.
    }

    /*
     * RegisterEnemy
     * -------------
     * Registers this enemy in the EnemyManager and TurnManager.
     */
    protected void RegisterEnemy()
    {
        EnemyManager.Instance.ActivateEnemy(this);
    }

    /*
     * CachePlayerMovement
     * -------------------
     * Finds and caches the player Movement component.
     * Called by all enemies before performing movement or kill logic.
     */
    protected void CachePlayerMovement()
    {
        if (player == null)
        {
            Movement[] allMovements = FindObjectsByType<Movement>(FindObjectsSortMode.None);

            foreach (Movement m in allMovements)
            {
                if (m != null && m.isPlayer)
                {
                    player = m.transform;
                    break;
                }
            }

            if (player == null)
            {
                Debug.LogError("EnemyBase: No player found.");
                return;
            }
        }

        if (playerMovement == null)
            playerMovement = player.GetComponent<Movement>();

        if (playerMovement == null)
            Debug.LogError("EnemyBase: Player has no Movement component.");
    }

    /*
     * IsEnemyMovementBlocked
     * ----------------------
     * Returns true if enemy movement is blocked this turn.
     * Used by all enemies before moving or pulling the player.
     */
    protected bool IsEnemyMovementBlocked()
    {
        return StatManager.Instance.PreventEnemyMovementThisTurn;
    }

    /*
     * PlaceEnemyBehindPlayer
     * ----------------------
     * Places the enemy behind the player by a given maximum roll distance.
     */
    protected void PlaceEnemyBehindPlayer(int maxRoll)
    {
        CachePlayerMovement();

        if (playerMovement == null)
            return;

        int playerPos = playerMovement.ActualPos;
        int enemyPos = playerPos - maxRoll;

        if (enemyPos < 0)
            enemyPos += playerMovement.Positions.Length;

        movement.ActualPos = enemyPos;

        Debug.Log(
            "Enemy spawned behind player at spot " + enemyPos +
            " (player at " + playerPos + ", maxRoll " + maxRoll + ")"
        );
    }

    /*
     * KillPlayerNow
     * -------------
     * Shared kill logic for all enemies.
     * Handles:
     * - Extra life consumption
     * - Respawn logic
     * - Enemy repositioning after respawn
     * - Final player death
     */
    protected void KillPlayerNow()
    {
        CachePlayerMovement();

        PassiveContext ctx = StatManager.Instance.PassiveCtx;

        // Extra life logic
        if (ctx.PlayerLives > 1)
        {
            ctx.PlayerLives--;
            ctx.ExtraLifeUsed = true;

            Debug.Log("Player consumed an extra life!");

            // Respawn player at starting position
            playerMovement.TeleportToPosition(playerMovement.startPos);

            // Determine enemy max roll
            int maxRoll = 6;
            if (this is DemonBoss)
                maxRoll = 18;

            int distance = Mathf.RoundToInt(maxRoll * 0.66f);

            // Reposition enemy behind player
            PlaceEnemyBehindPlayer(distance);

            // Teleport enemy visual
            movement.TeleportToPosition(movement.ActualPos);

            return;
        }

        Debug.Log("Player killed.");

        if (player != null)
            Destroy(player.gameObject);
    }

    /*
     * StartTurn
     * ---------
     * Called by TurnManager when it is this enemy's turn.
     * Must be implemented by each enemy type.
     */
    public abstract void StartTurn();

    public virtual void ActivateForTesting() { }
}
