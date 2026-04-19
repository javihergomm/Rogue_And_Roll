using UnityEngine;
using System;
using System.Collections.Generic;

/*
 * Controls the turn cycle between the player and enemies, including player turn start,
 * enemy sequencing, and integration with movement and shop logic. The turn only ends
 * when the player movement explicitly indicates that the turn should end.
 */
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public static event Action OnPlayerTurnStarted;
    public static event Action OnEnemyTurnStarted;
    public static event Action<int> OnEnemyRollCalculated;

    // NUEVO: notificar el movimiento total del jugador
    public static event Action<int, List<string>> OnPlayerRollCalculated;

    private enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }

    private TurnState state = TurnState.PlayerTurn;

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

    public bool IsPlayerTurn()
    {
        return state == TurnState.PlayerTurn;
    }

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

    public bool IsEnemyRegistered(EnemyBase enemy)
    {
        return activeEnemies.Contains(enemy);
    }

    public void RegisterEnemy(EnemyBase enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
    }

    public void StartPlayerTurn()
    {
        state = TurnState.PlayerTurn;

        StatManager.Instance.NextTurn();

        Debug.Log("=== PLAYER TURN ===");
        OnPlayerTurnStarted?.Invoke();
    }

    private void OnPlayerFinishedMovement()
    {
        if (state != TurnState.PlayerTurn)
            return;

        if (!playerMovement.turnShouldEnd)
        {
            Debug.Log("Player movement finished but turn should not end.");
            return;
        }

        Debug.Log("Player finished movement.");

        // NUEVO: notificar el movimiento total del jugador
        int totalMovement = playerMovement.lastTotalMovement;
        List<string> efectos = DiceRollManager.Instance.GetLastAppliedEffects();

        OnPlayerRollCalculated?.Invoke(totalMovement, efectos);

        StartEnemyTurns();
    }

    public void ForcePlayerTurnEnd()
    {
        OnPlayerFinishedMovement();
    }

    public void NotifyEnemyFinishedMovement()
    {
        OnEnemyFinishedMovement();
    }

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

        if (StatManager.Instance.PreventEnemyMovementThisTurn)
        {
            Debug.Log($"Enemy movement blocked: {enemy.name}");
            currentEnemyIndex++;
            StartNextEnemyTurn();
            return;
        }

        Debug.Log($"Starting turn for enemy: {enemy.name}");

        enemy.movement.OnMovementFinished -= OnEnemyFinishedMovement;
        enemy.movement.OnMovementFinished += OnEnemyFinishedMovement;

        enemy.StartTurn();
    }

    private void OnEnemyFinishedMovement()
    {
        Debug.Log("Enemy finished movement.");

        EnemyBase enemy = activeEnemies[currentEnemyIndex];
        enemy.movement.OnMovementFinished -= OnEnemyFinishedMovement;

        currentEnemyIndex++;
        StartNextEnemyTurn();
    }

    public static void NotifyEnemyRoll(int total)
    {
        OnEnemyRollCalculated?.Invoke(total);
    }
}
