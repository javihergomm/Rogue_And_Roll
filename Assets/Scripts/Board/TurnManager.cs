using UnityEngine;
using System;
using System.Collections.Generic;

/*
 * TurnManager
 * -----------
 * Controls the turn flow between player and enemies.
 * Tracks turn number, enemy order, and player roll limits per turn.
 * Ensures the player can only roll a limited number of times each turn,
 * unless passive effects grant additional rolls.
 */
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    // Event fired when an enemy roll is calculated
    public static event Action<int> OnEnemyRollCalculated;

    // Internal turn state
    private enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }

    private TurnState state = TurnState.PlayerTurn;

    // Current turn number
    public int TurnNumber { get; private set; } = 0;

    // Active enemies in the scene
    private List<EnemyBase> activeEnemies = new();
    private int currentEnemyIndex = 0;

    // Reference to the player movement component
    private Movement playerMovement;

    // Roll tracking for the current turn
    private int rollsUsedThisTurn = 0;
    private int rollsAllowedThisTurn = 1;

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

    // Returns true if it's currently the player's turn
    public bool IsPlayerTurn()
    {
        return state == TurnState.PlayerTurn;
    }

    /*
     * Waits until the player Movement component is found in the scene.
     * Uses the updated Unity API without deprecated parameters.
     */
    private System.Collections.IEnumerator WaitForPlayer()
    {
        while (playerMovement == null)
        {
            Movement[] allMovements = FindObjectsByType<Movement>(FindObjectsInactive.Exclude);

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

        // Subscribe to movement finished event
        playerMovement.OnMovementFinished += OnPlayerFinishedMovement;

        // Register player movement in DiceRollManager
        DiceRollManager.Instance.RegisterPlayerMovement(playerMovement);

        // Start the first player turn
        StartPlayerTurn();
    }

    // ============================================================
    // PLAYER TURN
    // ============================================================

    /*
     * Starts the player's turn:
     * - Increments turn number
     * - Resets roll counters
     * - Notifies passive effects
     * - Handles special passives (e.g., AvoidBadSpot)
     */
    public void StartPlayerTurn()
    {
        TurnNumber++;
        state = TurnState.PlayerTurn;

        // Reset roll counters
        rollsUsedThisTurn = 0;
        rollsAllowedThisTurn = 1;

        StatManager.Instance.NextTurn();
        var ctx = StatManager.Instance.PassiveCtx;

        CharacterEffectManager.Instance.NotifyTurnStart();

        if (ctx.AvoidBadSpotEvery3TurnsActive)
        {
            ctx.AvoidBadSpotTurnCounter++;

            if (ctx.AvoidBadSpotTurnCounter >= 3)
            {
                ctx.AvoidBadSpotBoostReady = true;
                ctx.AvoidBadSpotTurnCounter = 0;
            }
        }
    }

    /*
     * Called when the player finishes movement.
     * Ends the player's turn if movement rules allow it.
     */
    private void OnPlayerFinishedMovement()
    {
        if (state != TurnState.PlayerTurn)
            return;

        if (!playerMovement.turnShouldEnd)
            return;

        CharacterEffectManager.Instance.NotifyTurnEnd();

        StartEnemyTurns();
    }

    public void ForcePlayerTurnEnd()
    {
        OnPlayerFinishedMovement();
    }

    // ============================================================
    // ENEMY TURNS
    // ============================================================

    public void StartEnemyTurns()
    {
        if (activeEnemies.Count == 0)
        {
            StartPlayerTurn();
            return;
        }

        state = TurnState.EnemyTurn;
        currentEnemyIndex = 0;

        StartNextEnemyTurn();
    }

    /*
     * Starts the next enemy's turn.
     * Skips invalid or disabled enemies.
     */
    private void StartNextEnemyTurn()
    {
        if (activeEnemies.Count == 0)
        {
            StartPlayerTurn();
            return;
        }

        if (currentEnemyIndex < 0 || currentEnemyIndex >= activeEnemies.Count)
        {
            StartPlayerTurn();
            return;
        }

        EnemyBase enemy = activeEnemies[currentEnemyIndex];

        if (enemy == null || !enemy.isActiveAndEnabled)
        {
            currentEnemyIndex++;
            StartNextEnemyTurn();
            return;
        }

        if (StatManager.Instance.PreventEnemyMovementThisTurn)
        {
            currentEnemyIndex++;
            StartNextEnemyTurn();
            return;
        }

        CharacterEffectManager.Instance.NotifyEnemyTurnStart(enemy);

        enemy.movement.OnMovementFinished -= OnEnemyFinishedMovement;
        enemy.movement.OnMovementFinished += OnEnemyFinishedMovement;

        enemy.StartTurn();
    }

    /*
     * Called when an enemy finishes movement.
     * Moves to the next enemy or returns to the player turn.
     */
    private void OnEnemyFinishedMovement()
    {
        if (activeEnemies.Count == 0)
        {
            StartPlayerTurn();
            return;
        }

        if (currentEnemyIndex < 0 || currentEnemyIndex >= activeEnemies.Count)
        {
            StartPlayerTurn();
            return;
        }

        EnemyBase enemy = activeEnemies[currentEnemyIndex];

        if (enemy != null && enemy.movement != null)
            enemy.movement.OnMovementFinished -= OnEnemyFinishedMovement;

        CharacterEffectManager.Instance.NotifyEnemyTurnEnd(enemy);

        currentEnemyIndex++;

        if (currentEnemyIndex >= activeEnemies.Count)
        {
            StartPlayerTurn();
            return;
        }

        StartNextEnemyTurn();
    }

    public void ForceEnemyTurnEnd()
    {
        OnEnemyFinishedMovement();
    }

    public static void NotifyEnemyRoll(int total)
    {
        OnEnemyRollCalculated?.Invoke(total);
    }

    // ============================================================
    // ENEMY REGISTRATION (FIXED)
    // ============================================================

    /*
     * Returns true if the given enemy is already registered
     * in the active enemy list.
     */
    public bool IsEnemyRegistered(EnemyBase enemy)
    {
        return enemy != null && activeEnemies.Contains(enemy);
    }

    /*
     * Registers an enemy into the active enemy list
     * so it participates in enemy turns.
     */
    public void RegisterEnemy(EnemyBase enemy)
    {
        if (enemy == null)
            return;

        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    /*
     * Removes an enemy from the active enemy list
     * so it no longer participates in enemy turns.
     */
    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (enemy == null)
            return;

        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
    }

    // ============================================================
    // ROLL CONTROL
    // ============================================================

    public int GetRollsUsed() => rollsUsedThisTurn;

    public int GetRollsAllowed() => rollsAllowedThisTurn;

    public void AddExtraRolls(int amount)
    {
        rollsAllowedThisTurn += amount;
    }

    public bool CanPlayerRoll()
    {
        return rollsUsedThisTurn < rollsAllowedThisTurn;
    }

    public void RegisterPlayerRoll()
    {
        rollsUsedThisTurn++;
    }
}
