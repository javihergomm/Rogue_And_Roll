using UnityEngine;
using System.Collections.Generic;

/*
 * Revenant
 * --------
 * Enemy that activates after a required number of player laps.
 * Once active, it moves using a single d6 roll.
 * After moving, if the Revenant ends up in any row that the player occupies,
 * the player is instantly killed.
 * Direct collision with the player also results in instant kill.
 */
public class RevenantBoss : EnemyBase
{
    private GameObject revenantTokenInstance;
    private int playerLaps = 0;

    // Board rows defined by spot index ranges (supports overlapping and wrap-around)
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
        (67, 1) // wrap-around
    };

    void Update()
    {
        if (!isActive || movement == null)
            return;

        // Direct collision capture
        Movement playerMovement = player.GetComponent<Movement>();
        if (playerMovement != null && movement.ActualPos == playerMovement.ActualPos)
            KillPlayer();
    }

    /*
     * Called when the player completes a lap.
     * Activates the Revenant once the required number of laps is reached.
     */
    public void OnPlayerCompletedLap()
    {
        if (!data.requiresPlayerLap)
            return;

        playerLaps++;

        if (!isActive && playerLaps >= data.lapsToActivate)
            ActivateRevenant();
    }

    /*
     * Spawns the Revenant cup and token, initializes movement,
     * and places the token safely behind the player.
     */
    private void ActivateRevenant()
    {
        SpawnEnemy();

        revenantTokenInstance = Instantiate(data.tilePrefab);
        movement = revenantTokenInstance.GetComponent<Movement>();

        InitializeRevenant();

        // Revenant uses 1d6 -> max roll = 6
        PlaceEnemyBehindPlayer(6);

        isActive = true;
    }

    /*
     * Prepares the Revenant's movement component so it behaves correctly
     * both in gameplay and when using test buttons.
     */
    private void InitializeRevenant()
    {
        if (player == null)
        {
            Movement[] all = FindObjectsByType<Movement>(FindObjectsSortMode.None);
            foreach (Movement m in all)
            {
                if (m.isPlayer)
                {
                    player = m.transform;
                    break;
                }
            }
        }

        Movement playerMovement = player.GetComponent<Movement>();

        movement.SetPositions(playerMovement.Positions);

        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;
    }

    /*
     * Rolls 1d6, moves the Revenant, and checks for row-based capture.
     */
    public override void StartTurn()
    {
        if (!isActive || movement == null || player == null)
            return;

        int roll = EnemyDice.ThrowDice(); // 1-6

        movement.StartMovingFixed(roll);

        movement.OnMovementFinished += CheckCaptureAfterMove;
    }

    /*
     * After movement finishes, checks whether the Revenant and player
     * share any row. If so, the player is instantly killed.
     */
    private void CheckCaptureAfterMove()
    {
        movement.OnMovementFinished -= CheckCaptureAfterMove;

        Movement playerMovement = player.GetComponent<Movement>();

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
                    KillPlayer();
                    return;
                }
            }
        }
    }

    /*
     * Returns true if a spot index is inside a row range.
     * Supports wrap-around ranges such as 67-1.
     */
    private bool IsInRange(int spot, int start, int end)
    {
        if (start <= end)
            return spot >= start && spot <= end;

        return spot >= start || spot <= end;
    }

    /*
     * Returns all rows that a given spot belongs to.
     * Supports overlapping rows.
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

    /*
     * Handles Revenant killing the player.
     */
    private void KillPlayer()
    {
        Debug.Log("Player killed by Revenant.");
    }

    // ---------------- TEST BUTTONS ----------------

    [ContextMenu("Test: Spawn Revenant")]
    private void TestSpawn()
    {
        ActivateRevenant();
    }

    [ContextMenu("Test: Start Turn")]
    private void TestStartTurn()
    {
        StartTurn();
    }

    [ContextMenu("Test: Add 1 Lap")]
    private void TestAddLap()
    {
        OnPlayerCompletedLap();
    }
}
