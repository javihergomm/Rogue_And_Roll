using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game/Enemy")]
public class EnemySO : ScriptableObject
{
    [Header("Identity")]
    public string enemyID;
    public string enemyName;

    [Header("Prefabs")]
    public GameObject enemyPrefab;   // The EMPTY prefab containing DemonBoss (logic only)
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

    // ============================================================
    // UNIVERSAL SPAWNER — FOR TESTING ONLY
    // Instantiates ONLY the enemy logic prefab (EMPTY)
    // The enemy itself will spawn its cup and token.
    // ============================================================
    public EnemyBase SpawnForTesting()
    {
        // 1. Instantiate the enemy logic prefab (EMPTY with DemonBoss)
        GameObject enemyObj = Instantiate(enemyPrefab);

        // 2. Get the EnemyBase component
        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();

        if (enemy == null)
        {
            Debug.LogError("EnemySO: Enemy prefab has no EnemyBase component!");
            return null;
        }

        // 3. Assign this SO to the enemy
        enemy.data = this;

        // 4. Trigger the real activation flow
        // Each enemy type implements its own TestSpawn() method
        enemyObj.SendMessage("TestSpawn", SendMessageOptions.DontRequireReceiver);

        return enemy;
    }
}
