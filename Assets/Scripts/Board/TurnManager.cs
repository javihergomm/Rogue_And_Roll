using UnityEngine;
using System;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    // Mantengo solo el evento de roll enemigo porque tu UI lo usa
    public static event Action<int> OnEnemyRollCalculated;

    private enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }

    private TurnState state = TurnState.PlayerTurn;
    public int TurnNumber { get; private set; } = 0;

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

    // ============================================================
    // PLAYER TURN
    // ============================================================
    public void StartPlayerTurn()
    {
        TurnNumber++;
        state = TurnState.PlayerTurn;

        StatManager.Instance.NextTurn();
        var ctx = StatManager.Instance.PassiveCtx;

        // Notificar efectos
        CharacterEffectManager.Instance.NotifyTurnStart();

        // Lógica especial tuya
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
    // ENEMY TURNS (turnos reales)
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

    private void StartNextEnemyTurn()
    {
        if (currentEnemyIndex >= activeEnemies.Count)
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

        // Bloqueo global de movimiento enemigo
        if (StatManager.Instance.PreventEnemyMovementThisTurn)
        {
            currentEnemyIndex++;
            StartNextEnemyTurn();
            return;
        }

        // ============================================================
        // Notificar inicio de turno enemigo
        // ============================================================
        CharacterEffectManager.Instance.NotifyEnemyTurnStart(enemy);

        enemy.movement.OnMovementFinished -= OnEnemyFinishedMovement;
        enemy.movement.OnMovementFinished += OnEnemyFinishedMovement;

        enemy.StartTurn();
    }

    private void OnEnemyFinishedMovement()
    {
        EnemyBase enemy = activeEnemies[currentEnemyIndex];
        enemy.movement.OnMovementFinished -= OnEnemyFinishedMovement;

        // ============================================================
        // Notificar fin de turno enemigo
        // ============================================================
        CharacterEffectManager.Instance.NotifyEnemyTurnEnd(enemy);

        currentEnemyIndex++;
        StartNextEnemyTurn();
    }

    // ============================================================
    // Método requerido por BansheeBoss
    // ============================================================
    public void ForceEnemyTurnEnd()
    {
        OnEnemyFinishedMovement();
    }

    // ============================================================
    // ENEMY ROLL NOTIFICATION
    // ============================================================
    public static void NotifyEnemyRoll(int total)
    {
        OnEnemyRollCalculated?.Invoke(total);
    }
}
