using UnityEngine;

/*
 * EnemyTester
 * -----------
 * Utility class used to manually spawn any enemy for testing.
 * It instantiates the enemy from its EnemySO and triggers the
 * enemy's own activation method so it behaves exactly as in real gameplay.
 */
public class EnemyTester : MonoBehaviour
{
    public EnemySO enemyToTest;

    [ContextMenu("Spawn Enemy")]
    void SpawnEnemy()
    {
        if (enemyToTest == null)
        {
            Debug.LogError("EnemyTester: No EnemySO assigned!");
            return;
        }

        // Instantiate the enemy prefab defined in the EnemySO
        GameObject obj = Instantiate(enemyToTest.enemyPrefab);

        EnemyBase enemy = obj.GetComponent<EnemyBase>();
        if (enemy == null)
        {
            Debug.LogError("EnemyTester: The prefab does not contain an EnemyBase component!");
            return;
        }

        Debug.Log("EnemyTester: Enemy instantiated.");

        // Trigger the enemy's testing activation
        enemy.ActivateForTesting();

        Debug.Log("EnemyTester: Enemy activated for testing.");
    }
}
