using UnityEngine;

/*
 * EnemyManager
 * ------------
 * Holds references to all enemies in the scene.
 * Called after the playerObject finishes moving to trigger enemy turns.
 */
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Enemies")]
    public EnemyBase[] enemies;   // All enemies in the scene

    private void Awake()
    {
        Instance = this;
    }

    /*
     * Called after the playerObject finishes their movement.
     * Each enemy performs its own StartTurn() logic.
     */
    public void StartEnemyTurns()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
                enemy.StartTurn();
        }
    }
}
