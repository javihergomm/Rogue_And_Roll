using UnityEngine;

/*
 * EnemySO
 * -------
 * ScriptableObject that defines all data for an enemy type.
 * Contains:
 *  - Identity and prefabs
 *  - Spawn rules and activation conditions
 *  - Durability (how many laps the enemy stays alive)
 *  - Audio and effects
 */
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
    public float lapsToActivate = 1f;
    public bool requiresPlayerLap = true;
    public bool spawnOnlyOnce = true;
    [HideInInspector] public bool hasSpawnedOnce = false;

    [Header("Durability")]
    public float durabilityLaps = 1f;   // How many laps the enemy stays active after spawning

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
        GameObject enemyObj = Instantiate(enemyPrefab);

        if (!enemyObj.TryGetComponent<EnemyBase>(out var enemy))
        {
            Debug.LogError("EnemySO: Enemy prefab has no EnemyBase component!");
            return null;
        }

        enemy.data = this;

        enemyObj.SendMessage("TestSpawn", SendMessageOptions.DontRequireReceiver);

        return enemy;
    }
}
