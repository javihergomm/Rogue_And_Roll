using UnityEngine;

/*
 * EnemyBase
 * ---------
 * Base class for all enemies.
 */
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemySO data;

    protected GameObject cupInstance;
    protected GameObject tileInstance;

    public Transform player;
    public Movement movement;

    private Movement cachedPlayerMovement;
    protected bool isActive = false;

    /*
     * SpawnEnemy
     * ----------
     * Spawns the enemy's cup at the spawn point opposite to the player's spawn.
     */
    public virtual void SpawnEnemy()
    {
        isActive = true;

        if (player != null)
            cachedPlayerMovement = player.GetComponent<Movement>();

        string playerSpawnName = CharacterSelectManager.Instance.SelectedCharacter.spawnPointName;
        string enemyCupSpawnName = GetOppositeSpawn(playerSpawnName);

        Transform cupSpawn = FindSpawnPoint(enemyCupSpawnName);

        if (cupSpawn == null)
        {
            Debug.LogError("EnemyBase: Cup spawn point missing for enemy " + data.enemyName +
                           " (searched for: " + enemyCupSpawnName + ")");
            return;
        }

        cupInstance = Instantiate(data.cupPrefab, cupSpawn.position, cupSpawn.rotation);

        if (BoardHider.Instance != null)
            BoardHider.Instance.RegisterObject(cupInstance);
    }

    /*
     * PlaceEnemyBehindPlayer
     * ----------------------
     * Places the enemy token behind the player at a safe distance.
     */
    protected void PlaceEnemyBehindPlayer(int maxRoll)
    {
        if (movement == null || player == null)
        {
            Debug.LogError("EnemyBase: Missing movement or player reference.");
            return;
        }

        // Usar el componente cacheado (sin GetComponent)
        if (cachedPlayerMovement == null)
            return;

        int playerSpot = cachedPlayerMovement.ActualPos;

        int safeSpot = playerSpot - (maxRoll + 1);
        safeSpot = Mathf.Max(1, safeSpot);

        movement.ActualPos = safeSpot;
        movement.transform.position = movement.Positions[safeSpot - 1].position;

        Debug.Log($"Enemy spawned behind player at spot {safeSpot} (player at {playerSpot}, maxRoll {maxRoll})");
    }

    /*
     * GetOppositeSpawn
     * ----------------
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
     */
    private Transform FindSpawnPoint(string name)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.isLoaded &&
                obj.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                return obj.transform;
        }

        return null;
    }

    /*
     * StartTurn
     * ---------
     */
    public virtual void StartTurn() { }
}
