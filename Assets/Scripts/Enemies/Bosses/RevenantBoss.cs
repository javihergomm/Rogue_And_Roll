using UnityEngine;
using System.Collections.Generic;

/*
 * RevenantBoss
 * ------------
 * Enemy type that activates after a required number of player laps.
 * Once active, it moves using a single dice roll.
 * After moving, it checks if the player is in any matching board row.
 * If the revenant reaches the same spot or row as the player, the player is killed.
 */
public class RevenantBoss : EnemyBase
{
    private GameObject revenantTokenInstance;
    private int playerLaps = 0;

    // Board rows defined by spot index ranges
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
        if (!isActive || movement == null)
            return;

        Movement playerMovement = player.GetComponent<Movement>();
        if (playerMovement != null && movement.ActualPos == playerMovement.ActualPos)
        {
            Debug.Log("Revenant collided directly with player.");
            KillPlayer();
        }
    }

    /*
     * OnPlayerCompletedLap
     * --------------------
     * Tracks how many laps the player has completed and activates the revenant
     * when the required number is reached.
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
     * ActivateRevenant
     * ----------------
     * Starts the revenant activation process.
     */
    private void ActivateRevenant()
    {
        SpawnEnemy();

        revenantTokenInstance = Instantiate(data.tilePrefab);

        if (BoardHider.Instance != null)
            BoardHider.Instance.RegisterObject(revenantTokenInstance);

        movement = revenantTokenInstance.GetComponent<Movement>();

        InitializeRevenant();

        PlaceEnemyBehindPlayer(6);

        isActive = true;

        EnemyManager.Instance.ActivateEnemy(this);
    }

    /*
     * InitializeRevenant
     * ------------------
     * Sets up the revenant's movement system using the player's board positions.
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

        if (player == null)
        {
            Debug.LogError("RevenantBoss: Player not found.");
            return;
        }

        Movement playerMovement = player.GetComponent<Movement>();
        movement.SetPositions(playerMovement.Positions);

        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;
    }

    /*
     * StartTurn
     * ---------
     * Rolls one dice and moves the revenant accordingly.
     * After movement, it checks for row-based capture.
     */
    public override void StartTurn()
    {
        if (!isActive || movement == null || player == null)
            return;

        int roll = EnemyDice.ThrowDice();

        TurnManager.NotifyEnemyRoll(roll);

        movement.StartMovingFixed(roll);
        movement.OnMovementFinished += CheckCaptureAfterMove;
    }

    /*
     * CheckCaptureAfterMove
     * ---------------------
     * After movement, checks if the revenant and player share any board row.
     * If so, the player is killed.
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
     * IsInRange
     * ---------
     * Checks if a board spot is inside a row range.
     */
    private bool IsInRange(int spot, int start, int end)
    {
        return start <= end
            ? spot >= start && spot <= end
            : spot >= start || spot <= end;
    }

    /*
     * GetRowsForSpot
     * --------------
     * Returns all row indices that contain the given board spot.
     */
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

    /*
     * KillPlayer
     * ----------
     * Called when the revenant catches the player.
     */
    private void KillPlayer()
    {
        Debug.LogError("PLAYER KILLED BY REVENANT");
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
    public override void ActivateForTesting()
    {
        TestSpawn(); // uses the same activation flow as normal gameplay
    }

}
