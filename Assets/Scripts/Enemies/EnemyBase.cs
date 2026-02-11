using UnityEngine;

/*
 * EnemyBase
 * ---------
 * Base class for all enemies.
 * - Spawns the enemy cup at the spawn opposite to the player's spawn
 * - Spawns the enemy tile at a fixed Spot (handled by child classes)
 * - Provides activation and turn structure for enemy behaviors
 */
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemySO data;              // ScriptableObject containing enemy configuration

    protected GameObject cupInstance; // Spawned enemy cup
    protected GameObject tileInstance; // Spawned enemy tile (optional, depends on enemy type)

    public Transform player;          // Reference to the player
    public Movement movement;         // Movement component for the enemy tile

    protected bool isActive = false;  // Whether the enemy is active in the game

    /*
     * SpawnEnemy
     * ----------
     * Spawns the enemy's cup at the spawn point opposite to the player's spawn.
     * Tile spawning is handled by child classes (e.g., DemonBoss).
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
    public abstract void StartTurn();
}
