using UnityEngine;
using System.Collections.Generic;

/*
 * EnemyManager
 * ------------
 * Central registry for all active enemies.
 * Handles:
 * - Activating enemies
 * - Preventing duplicate registrations
 * - Storing the list of active enemies
 * - Coordinating with TurnManager
 */
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    // List of all active enemies
    public List<EnemyBase> enemies = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("EnemyManager Awake: INSTANCE SET");
    }

    /*
     * ActivateEnemy
     * -------------
     * Registers an enemy in the TurnManager and in the local list.
     * Prevents duplicate registration.
     */
    public void ActivateEnemy(EnemyBase enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("EnemyManager: Tried to activate a NULL enemy.");
            return;
        }

        if (TurnManager.Instance == null)
        {
            Debug.LogError("EnemyManager: TurnManager not found.");
            return;
        }

        // Prevent duplicate registration in local list
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);

        // Prevent duplicate registration in TurnManager
        if (!TurnManager.Instance.IsEnemyRegistered(enemy))
            TurnManager.Instance.RegisterEnemy(enemy);
    }

    /*
     * RemoveEnemy
     * -----------
     * Removes an enemy from the manager and TurnManager.
     */
    public void RemoveEnemy(EnemyBase enemy)
    {
        if (enemy == null)
            return;

        if (enemies.Contains(enemy))
            enemies.Remove(enemy);

        if (TurnManager.Instance.IsEnemyRegistered(enemy))
            TurnManager.Instance.UnregisterEnemy(enemy);
    }
}
