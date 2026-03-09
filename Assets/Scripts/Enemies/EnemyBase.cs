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
 * - Shared kill logic (KillPlayerNow)
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

    public void SpawnEnemy()
    {
        // No auto-registration here.
    }

    protected void RegisterEnemy()
    {
        EnemyManager.Instance.ActivateEnemy(this);
    }

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

        Debug.Log("Enemy spawned behind player at spot " + enemyPos +
                  " (player at " + playerPos + ", maxRoll " + maxRoll + ")");
    }

    /*
     * KillPlayerNow
     * -------------
     * Shared kill logic for ALL enemies.
     * Each enemy decides WHEN to call this.
     * No GameManager required.
     */
    protected void KillPlayerNow()
    {
        Debug.Log("Player killed.");

        if (player != null)
            Destroy(player.gameObject);

        // No GameManager yet, so we stop here.
        // Later you can add:
        // GameManager.Instance.EndGame();
    }

    public abstract void StartTurn();

    public virtual void ActivateForTesting() { }
}
