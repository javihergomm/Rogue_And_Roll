using UnityEngine;
using System.Collections.Generic;

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
        {
            Debug.Log("[Revenant] Landed on player tile. Killing player.");
            KillPlayerNow();
        }
    }

    public override void SpawnEnemy()
    {
        base.SpawnEnemy();
    }

    public override void StartTurn()
    {
        if (!isActive || movement == null)
            return;

        // BLOQUEO REAL
        if (IsEnemyMovementBlocked())
        {
            Debug.Log("[Revenant] Movement blocked this turn.");
            TurnManager.Instance.ForceEnemyTurnEnd();
            return;
        }

        int roll = EnemyDice.ThrowDice();

        Debug.Log("[Revenant] Rolled: " + roll);
        TurnManager.NotifyEnemyRoll(roll);

        movement.OnMovementFinished += CheckCaptureAfterMove;
        movement.StartMovingFixed(roll);
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
                    Debug.Log("[Revenant] Player and Revenant share a row. Killing player.");
                    KillPlayerNow();
                    TurnManager.Instance.ForceEnemyTurnEnd();
                    return;
                }
            }
        }

        Debug.Log("[Revenant] No shared row. Enemy turn ends.");
        TurnManager.Instance.ForceEnemyTurnEnd();
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
}
