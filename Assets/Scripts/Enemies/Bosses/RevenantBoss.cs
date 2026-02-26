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

        Movement playerMovement = player.GetComponent<Movement>();
        if (playerMovement != null && movement.ActualPos == playerMovement.ActualPos)
        {
            Debug.Log("Revenant collided directly with player.");
            KillPlayer();
        }
    }

    public void OnPlayerCompletedLap()
    {
        Debug.Log("Player completed lap. requiresPlayerLap=" + data.requiresPlayerLap);

        if (!data.requiresPlayerLap)
            return;

        playerLaps++;
        Debug.Log("Player laps: " + playerLaps + " / " + data.lapsToActivate);

        if (!isActive && playerLaps >= data.lapsToActivate)
            ActivateRevenant();
    }

    private void ActivateRevenant()
    {
        Debug.Log("ActivateRevenant() called");

        SpawnEnemy();
        Debug.Log("SpawnEnemy() finished");

        if (data.tilePrefab == null)
            Debug.LogError("ERROR: tilePrefab is NULL!");

        revenantTokenInstance = Instantiate(data.tilePrefab);
        Debug.Log("Instantiated revenant token: " + revenantTokenInstance);

        // Register revenant token so BoardHider can hide it inside the shop
        if (BoardHider.Instance != null)
            BoardHider.Instance.RegisterObject(revenantTokenInstance);

        movement = revenantTokenInstance.GetComponent<Movement>();
        Debug.Log("Movement component found: " + movement);

        InitializeRevenant();
        Debug.Log("Initialized Revenant");

        PlaceEnemyBehindPlayer(6);
        Debug.Log("Placed Revenant behind player");

        isActive = true;
        Debug.Log("Revenant is now ACTIVE");
    }

    private void InitializeRevenant()
    {
        Debug.Log("InitializeRevenant()");

        if (player == null)
        {
            Debug.Log("Player reference missing. Searching...");
            Movement[] all = FindObjectsByType<Movement>(FindObjectsSortMode.None);
            foreach (Movement m in all)
            {
                if (m.isPlayer)
                {
                    player = m.transform;
                    Debug.Log("Player found: " + player.name);
                    break;
                }
            }
        }

        if (player == null)
        {
            Debug.LogError("ERROR: Player NOT FOUND!");
            return;
        }

        Movement playerMovement = player.GetComponent<Movement>();
        movement.SetPositions(playerMovement.Positions);

        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;

        Debug.Log("Revenant movement initialized at pos " + movement.ActualPos);
    }

    public override void StartTurn()
    {
        Debug.Log("StartTurn() called. isActive=" + isActive + " movement=" + movement + " player=" + player);

        if (!isActive || movement == null || player == null)
        {
            Debug.LogWarning("StartTurn aborted due to missing references.");
            return;
        }

        int roll = EnemyDice.ThrowDice();
        Debug.Log("Revenant rolled: " + roll);

        movement.StartMovingFixed(roll);
        movement.OnMovementFinished += CheckCaptureAfterMove;
    }

    private void CheckCaptureAfterMove()
    {
        movement.OnMovementFinished -= CheckCaptureAfterMove;

        Movement playerMovement = player.GetComponent<Movement>();

        int revenantSpot = movement.ActualPos;
        int playerSpot = playerMovement.ActualPos;

        Debug.Log("Movement finished. Revenant at " + revenantSpot + ", Player at " + playerSpot);

        int[] revenantRows = GetRowsForSpot(revenantSpot);
        int[] playerRows = GetRowsForSpot(playerSpot);

        Debug.Log("Revenant rows: " + string.Join(",", revenantRows));
        Debug.Log("Player rows: " + string.Join(",", playerRows));

        foreach (int r in revenantRows)
        {
            foreach (int p in playerRows)
            {
                if (r == p)
                {
                    Debug.Log("Revenant captured player by row overlap.");
                    KillPlayer();
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

    private void KillPlayer()
    {
        Debug.LogError("PLAYER KILLED BY REVENANT");
    }

    // ---------------- TEST BUTTONS ----------------

    [ContextMenu("Test: Spawn Revenant")]
    private void TestSpawn()
    {
        Debug.Log("TEST BUTTON: Spawn Revenant");
        ActivateRevenant();
    }

    [ContextMenu("Test: Start Turn")]
    private void TestStartTurn()
    {
        Debug.Log("TEST BUTTON: Start Turn");
        StartTurn();
    }

    [ContextMenu("Test: Add 1 Lap")]
    private void TestAddLap()
    {
        Debug.Log("TEST BUTTON: Add Lap");
        OnPlayerCompletedLap();
    }
}
