using UnityEngine;

/*
 * EnemyBase
 * ---------
 * Base class for all enemies.
 *
 * Responsibilities:
 * - Spawns the enemy's cup at the spawn point opposite to the player's spawn.
 * - Provides shared references (player, movement, data).
 * - Offers a universal method to place the enemy's token safely behind the player.
 * - Defines the turn structure through the abstract StartTurn() method.
 */
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemySO data;              // ScriptableObject containing enemy configuration

    protected GameObject cupInstance; // Spawned enemy cup
    protected GameObject tileInstance; // Spawned enemy tile (optional, depends on enemy type)

    public Transform player;          // Reference to the player
    public Movement movement;         // Movement component for the enemy token

    protected bool isActive = false;  // Whether the enemy is active in the game

    /*
     * SpawnEnemy
     * ----------
     * Spawns the enemy's cup at the spawn point opposite to the player's spawn.
     * Token spawning is handled by child classes.
     */
    public virtual void SpawnEnemy()
    {
        isActive = true;

        // Get player's spawn name
        string playerSpawnName = CharacterSelectManager.Instance.SelectedCharacter.spawnPointName;

        // Determine opposite spawn
        string enemyCupSpawnName = GetOppositeSpawn(playerSpawnName);

        // Find spawn point (case-insensitive)
        Transform cupSpawn = FindSpawnPoint(enemyCupSpawnName);

        if (cupSpawn == null)
        {
            Debug.LogError("EnemyBase: Cup spawn point missing for enemy " + data.enemyName +
                           " (searched for: " + enemyCupSpawnName + ")");
            return;
        }

        // Spawn the enemy cup
        cupInstance = Instantiate(data.cupPrefab, cupSpawn.position, cupSpawn.rotation);

        // Register cup so BoardHider can hide it inside the shop
        if (BoardHider.Instance != null)
            BoardHider.Instance.RegisterObject(cupInstance);
    }

    /*
     * PlaceEnemyBehindPlayer
     * ----------------------
     * Places the enemy token behind the player at a safe distance.
     *
     * The safe distance is calculated as:
     *      playerSpot - (maxRoll + 1)
     *
     * This guarantees that the enemy cannot reach the player
     * with its highest possible roll during the turn that begins.
     *
     * Parameters:
     * - maxRoll: the maximum roll value the enemy can achieve.
     */
    protected void PlaceEnemyBehindPlayer(int maxRoll)
    {
        if (movement == null || player == null)
        {
            Debug.LogError("EnemyBase: Missing movement or player reference.");
            return;
        }

        Movement playerMovement = player.GetComponent<Movement>();
        if (playerMovement == null)
            return;

        int playerSpot = playerMovement.ActualPos;

        // Safe distance behind the player
        int safeSpot = playerSpot - (maxRoll + 1);

        // Clamp to minimum spot
        safeSpot = Mathf.Max(1, safeSpot);

        movement.ActualPos = safeSpot;
        movement.transform.position = movement.Positions[safeSpot - 1].position;

        Debug.Log($"Enemy spawned behind player at spot {safeSpot} (player at {playerSpot}, maxRoll {maxRoll})");
    }

    /*
     * GetOppositeSpawn
     * ----------------
     * Returns the opposite spawn point name based on the player's spawn.
     * Case-insensitive and returns exact scene names.
     */
    private string GetOppositeSpawn(string playerSpawn)
    {
        string p = playerSpawn.ToLower();

        if (p.Contains("red")) return "Spawn_Yellow";
        if (p.Contains("yellow")) return "Spawn_Red";
        if (p.Contains("blue")) return "Spawn_Green";
        if (p.Contains("green")) return "Spawn_Blue";

        Debug.LogWarning("EnemyBase: Could not determine opposite spawn for " + playerSpawn);
        return playerSpawn;
    }

    /*
     * FindSpawnPoint
     * --------------
     * Finds a GameObject in the scene by name, case-insensitive.
     */
    private Transform FindSpawnPoint(string name)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.isLoaded && obj.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                return obj.transform;
        }

        return null;
    }

    /*
     * StartTurn
     * ---------
     * Called by the turn manager. Each enemy implements its own behavior.
     */
    public virtual void StartTurn() { }
}
