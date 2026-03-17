using UnityEngine;
using System.Collections.Generic;

/*
 * RevenantBoss
 * ------------
 * Enemy that activates through the standard lap-based spawn system.
 * Moves using a single dice roll. After moving, it checks if the player
 * is in any matching board row. If they share a tile or row, the player dies.
 */
public class RevenantBoss : EnemyBase
{
    private readonly (int start, int end)[] rows = new (int, int)[]
    {
        (1, 8),
        (9, 16),
        (16, 18),
        (18, 25),
        (26, 33),
        (33, 35),
        (35, 42),
        (43, 50),
        (50, 53),
        (53, 59),
        (60, 67),
        (67, 1)
    };

    private void Update()
    {
        if (!isActive || movement == null || playerMovement == null)
            return;

        if (movement.ActualPos == playerMovement.ActualPos)
            KillPlayerNow();
    }

    // ---------------------------------------------------------
    // NEW: SpawnEnemy override so EnemyManager can activate boss
    // ---------------------------------------------------------
    public override void SpawnEnemy()
    {
        StartCoroutine(SpawnRoutine());
    }

    private System.Collections.IEnumerator SpawnRoutine()
    {
        // Wait one frame so the host prefab is fully initialized
        yield return null;

        
        // Instanciar la cup
        CupInstance = Instantiate(data.cupPrefab);

        // Instanciar la tile
        GameObject token = Instantiate(data.tilePrefab);
        movement = token.GetComponent<Movement>();


        if (movement == null)
        {
            Debug.LogError("RevenantBoss: Token prefab has no Movement component!");
            yield break;
        }

        InitializeRevenant();

        // Place behind player (Revenant uses max roll 6)
        PlaceEnemyBehindPlayer(6);

        movement.TeleportToPosition(movement.ActualPos);

        isActive = true;

        EnemyManager.Instance.ActivateEnemy(this);
    }

    // ---------------------------------------------------------

    private void InitializeRevenant()
    {
        CachePlayerMovement();

        if (playerMovement == null)
            return;

        movement.SetPositions(playerMovement.Positions);

        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;
    }

    private System.Collections.IEnumerator ActivateRevenantRoutine()
    {
        // Only used for testing
        SpawnEnemy();
        yield return null;
    }

    public void ActivateRevenant()
    {
        StartCoroutine(ActivateRevenantRoutine());
    }

    public override void StartTurn()
    {
        if (!isActive || movement == null)
            return;

        int roll = EnemyDice.ThrowDice();

        TurnManager.NotifyEnemyRoll(roll);

        movement.StartMovingFixed(roll);
        movement.OnMovementFinished += CheckCaptureAfterMove;
    }

    private void CheckCaptureAfterMove()
    {
        movement.OnMovementFinished -= CheckCaptureAfterMove;

        int revenantSpot = movement.ActualPos;
        int playerSpot = playerMovement.ActualPos;

        int[] revenantRows = GetRowsForSpot(revenantSpot);
        int[] playerRows = GetRowsForSpot(playerSpot);

        foreach (int r in revenantRows)
        {
            foreach (int p in playerRows)
            {
                if (r == p)
                {
                    KillPlayerNow();
                    return;
                }
            }
        }
    }

    private bool IsInRange(int spot, int start, int end)
    {
        return start <= end
            ? spot >= start && spot <= end
            : spot >= start || spot <= end;
    }

    private int[] GetRowsForSpot(int spot)
    {
        List<int> result = new();

        for (int i = 0; i < rows.Length; i++)
        {
            if (IsInRange(spot, rows[i].start, rows[i].end))
                result.Add(i);
        }

        return result.ToArray();
    }

    public override void ActivateForTesting()
    {
        ActivateRevenant();
    }
}
