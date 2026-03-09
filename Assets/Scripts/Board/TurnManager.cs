using UnityEngine;
using System;
using System.Collections.Generic;

/*
 * TurnManager
 * -----------
 * Controls the turn cycle between the player and all active enemies.
 * Handles:
 * - Player turn start and completion
 * - Enemy turn sequence
 * - Enemy registration
 * - UI notifications for turn changes and enemy movement
 */
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    // UI events
    public static event Action OnPlayerTurnStarted;
    public static event Action OnEnemyTurnStarted;
    public static event Action<int> OnEnemyRollCalculated;

    private enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }

    private TurnState state = TurnState.PlayerTurn;

    // List of active enemies
    private List<EnemyBase> activeEnemies = new();
    private int currentEnemyIndex = 0;

    private Movement playerMovement;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    /*
     * WaitForPlayer
     * -------------
     * Ensures the player exists before registering movement.
     */
    private System.Collections.IEnumerator WaitForPlayer()
    {
        while (playerMovement == null)
        {
            Movement[] allMovements = FindObjectsByType<Movement>(FindObjectsSortMode.None);

            foreach (Movement m in allMovements)
            {
                if (m != null && m.isPlayer)
                {
                    playerMovement = m;
                    break;
                }
            }

            yield return null;
        }

        playerMovement.OnMovementFinished += OnPlayerFinishedMovement;
        DiceRollManager.Instance.RegisterPlayerMovement(playerMovement);

        StartPlayerTurn();
    }

    /*
     * IsEnemyRegistered
     * -----------------
     * Checks if an enemy is already in the active list.
     */
    public bool IsEnemyRegistered(EnemyBase enemy)
    {
        return activeEnemies.Contains(enemy);
    }

    /*
     * RegisterEnemy
     * -------------
     * Adds an enemy to the active enemy list.
     */
    public void RegisterEnemy(EnemyBase enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    /*
     * UnregisterEnemy
     * ---------------
     * Removes an enemy from the active enemy list.
     */
    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
    }

    /*
     * StartPlayerTurn
     * ---------------
     * Begins the player's turn and notifies the UI.
     */
    public void StartPlayerTurn()
    {
        state = TurnState.PlayerTurn;
        Debug.Log("=== PLAYER TURN ===");

        OnPlayerTurnStarted?.Invoke();
    }

    /*
     * OnPlayerFinishedMovement
     * -------------------------
     * Called when the player finishes moving.
     * Starts the enemy turn sequence.
     */
    private void OnPlayerFinishedMovement()
    {
        Debug.Log("Player finished movement.");
        StartEnemyTurns();
    }

    /*
     * StartEnemyTurns
     * ---------------
     * Begins the enemy turn phase.
     */
    public void StartEnemyTurns()
    {
        Debug.Log("=== ENEMY TURN ===");

        if (activeEnemies.Count == 0)
        {
            Debug.Log("No enemies available. Returning to player.");
            StartPlayerTurn();
            return;
        }

        state = TurnState.EnemyTurn;
        currentEnemyIndex = 0;

        OnEnemyTurnStarted?.Invoke();

        StartNextEnemyTurn();
    }

    /*
     * StartNextEnemyTurn
     * ------------------
     * Starts the turn of the next enemy in the list.
     */
    private void StartNextEnemyTurn()
    {
        Debug.Log($"Processing enemy index {currentEnemyIndex}/{activeEnemies.Count}");

        if (currentEnemyIndex >= activeEnemies.Count)
        {
            Debug.Log("All enemies completed their actions.");
            StartPlayerTurn();
            return;
        }

        EnemyBase enemy = activeEnemies[currentEnemyIndex];

        if (enemy == null || !enemy.isActiveAndEnabled)
        {
            Debug.Log("Enemy missing or disabled. Skipping.");
            currentEnemyIndex++;
            StartNextEnemyTurn();
            return;
        }

        Debug.Log($"Starting turn for enemy: {enemy.name}");

        enemy.movement.OnMovementFinished += OnEnemyFinishedMovement;

        enemy.StartTurn();
    }

    /*
     * OnEnemyFinishedMovement
     * ------------------------
     * Called when an enemy finishes moving.
     * Moves to the next enemy.
     */
    private void OnEnemyFinishedMovement()
    {
        Debug.Log("Enemy finished movement.");

        EnemyBase enemy = activeEnemies[currentEnemyIndex];
        enemy.movement.OnMovementFinished -= OnEnemyFinishedMovement;

        currentEnemyIndex++;
        StartNextEnemyTurn();
    }

    /*
     * NotifyEnemyRoll
     * ---------------
     * Sends the enemy's movement value to the UI.
     */
    public static void NotifyEnemyRoll(int total)
    {
        OnEnemyRollCalculated?.Invoke(total);
    }
}
