using UnityEngine;
using System.Collections.Generic;

/*
 * RevenantBoss
 * ------------
 * Enemy that kills the player if:
 * - It lands on the same tile as the player.
 * - After moving, both share at least one "row" defined by index ranges.
 *
 * Movement:
 * - Rolls a fixed enemy dice.
 * - Moves forward by the rolled amount.
 * - After movement, checks row overlap to determine if the player dies.
 */
public class RevenantBoss : EnemyBase
{
    // Row definitions: each tuple represents a start-end index range.
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

        // Instant kill if both are on the same tile
        if (movement.ActualPos == playerMovement.ActualPos)
        {
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

        // Movement blocked by effects
        if (IsEnemyMovementBlocked())
        {
            TurnManager.Instance.ForceEnemyTurnEnd();
            return;
        }

        int roll = EnemyDice.ThrowDice();
        TurnManager.NotifyEnemyRoll(roll);

        movement.OnMovementFinished += CheckCaptureAfterMove;
        movement.StartMovingFixed(roll);
    }

    /*
     * After movement, checks if Revenant and player share any row.
     * If they do, the player dies.
     */
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
                    TurnManager.Instance.ForceEnemyTurnEnd();
                    return;
                }
            }
        }

        TurnManager.Instance.ForceEnemyTurnEnd();
    }

    /*
     * Checks if a spot index is inside a start-end range.
     * Supports wrap-around ranges (e.g., 67 -> 1).
     */
    private bool IsInRange(int spot, int start, int end)
    {
        return start <= end
            ? spot >= start && spot <= end
            : spot >= start || spot <= end;
    }

    /*
     * Returns all row indices that contain the given spot.
     */
    private int[] GetRowsForSpot(int spot)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < rows.Length; i++)
        {
            if (IsInRange(spot, rows[i].start, rows[i].end))
                result.Add(i);
        }

        return result.ToArray();
    }
}
