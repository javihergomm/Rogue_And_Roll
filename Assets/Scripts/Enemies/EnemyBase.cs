using UnityEngine;

/*
 * EnemyBase
 * ---------
 * Base class for all enemies.
 * Handles:
 * - Storing enemy data
 * - Spawning the enemy logic object
 * - Positioning the enemy on the board
 * - Providing references to Movement and player
 */
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemySO data;

    [HideInInspector] public Movement movement;
    [HideInInspector] public Transform player;

    // Visual instance for enemies that spawn a token (cup, demon, etc.)
    public GameObject CupInstance { get; protected set; }

    protected bool isActive = false;

    /*
     * SpawnEnemy
     * ----------
     * Creates the enemy logic object.
     * Does NOT register the enemy automatically.
     */
    public void SpawnEnemy()
    {
        // No auto-registration here.
        // EnemyManager.ActivateEnemy() must be called explicitly by the enemy.
    }

    /*
     * RegisterEnemy
     * -------------
     * Registers this enemy in the EnemyManager.
     */
    protected void RegisterEnemy()
    {
        EnemyManager.Instance.ActivateEnemy(this);
    }

    /*
     * PlaceEnemyBehindPlayer
     * ----------------------
     * Places the enemy a fixed number of steps behind the player.
     */
    protected void PlaceEnemyBehindPlayer(int maxRoll)
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
                Debug.LogError("EnemyBase: No player found when placing enemy.");
                return;
            }
        }

        Movement playerMovement = player.GetComponent<Movement>();
        if (playerMovement == null)
        {
            Debug.LogError("EnemyBase: Player has no Movement component.");
            return;
        }

        int playerPos = playerMovement.ActualPos;
        int enemyPos = playerPos - maxRoll;

        if (enemyPos < 0)
            enemyPos += playerMovement.Positions.Length;

        movement.ActualPos = enemyPos;

        Debug.Log($"Enemy spawned behind player at spot {enemyPos} (player at {playerPos}, maxRoll {maxRoll})");
    }

    /*
     * StartTurn
     * ---------
     * Must be implemented by each enemy type.
     */
    public abstract void StartTurn();

    /*
     * ActivateForTesting
     * ------------------
     * Allows EnemyTester to activate the enemy manually.
     */
    public virtual void ActivateForTesting() { }
}
