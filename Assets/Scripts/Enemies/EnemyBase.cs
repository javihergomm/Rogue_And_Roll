using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject cupPrefab;      // Prefab of the enemy's cup

    [Header("Runtime Instances")]
    protected GameObject cupInstance; // Instantiated cup during gameplay

    [Header("References")]
    public Transform player;          // Reference to the player
    public Movement movement;         // Movement component of the enemy token

    protected bool isActive = false;  // Indicates whether the enemy is active

    // Spawns the enemy's cup at the given spawn point and activates the enemy
    public virtual void SpawnEnemy(Transform cupSpawnPoint)
    {
        isActive = true;

        cupInstance = Instantiate(
            cupPrefab,
            cupSpawnPoint.position,
            cupSpawnPoint.rotation
        );
    }

    // Executes the enemy's turn logic
    public abstract void StartTurn();
}
