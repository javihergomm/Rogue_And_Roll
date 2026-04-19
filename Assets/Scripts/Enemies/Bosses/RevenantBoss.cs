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
            KillPlayerNow();
    }
    public override void SpawnEnemy()
    {
        base.SpawnEnemy();
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
}
