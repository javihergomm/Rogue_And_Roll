using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game/Enemy")]
public class EnemySO : ScriptableObject
{
    [Header("Identity")]
    public string enemyID;
    public string enemyName;

    [Header("Prefabs")]
    public GameObject enemyPrefab;   // Logic-only prefab containing the EnemyBase script
    public GameObject cupPrefab;     // Visual cup prefab
    public GameObject tilePrefab;    // Visual token prefab (Movement + Mesh)

    [Header("Effects")]
    public BaseEffect[] effects;

    [Header("Spawn Settings")]
    public string cupSpawnPointName;
    public int tileSpotIndex;

    [Header("Behavior")]
    public int lapsToActivate = 1;
    public bool requiresPlayerLap = true;
    public bool spawnOnlyOnce = true;

    [Header("Audio")]
    public AudioClip spawnSFX;

    /*
     * SpawnForTesting
     * ---------------
     * Instantiates only the logic prefab for testing.
     * The enemy itself will spawn its own visuals and token.
     */
    public EnemyBase SpawnForTesting()
    {
        // Instantiate the logic-only enemy prefab
        GameObject enemyObj = Instantiate(enemyPrefab);

        // Use TryGetComponent to avoid allocation warnings
        if (!enemyObj.TryGetComponent<EnemyBase>(out var enemy))
        {
            Debug.LogError("EnemySO: Enemy prefab has no EnemyBase component!");
            return null;
        }

        // Assign this ScriptableObject to the enemy
        enemy.data = this;

        // Trigger the enemy's testing spawn method
        enemyObj.SendMessage("TestSpawn", SendMessageOptions.DontRequireReceiver);

        return enemy;
    }
}
