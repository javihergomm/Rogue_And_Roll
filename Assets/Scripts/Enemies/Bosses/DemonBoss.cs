using UnityEngine;

/*
 * DemonBoss
 * ---------
 * Enemy behavior for the Demon Boss.
 *
 * Summary:
 * - Activates after the player completes a required number of laps.
 * - Spawns its cup using EnemyBase logic (opposite to the player's spawn).
 * - Spawns its board token and places it safely behind the player.
 * - Moves using three six-sided dice (3d6).
 * - Instantly kills the player if all three dice roll a six (666).
 * - Also kills the player if it lands on the same board spot as the player.
 */
public class DemonBoss : EnemyBase
{
    private GameObject demonTokenInstance;
    private int playerLaps = 0;

    void Update()
    {
        if (!isActive || movement == null)
            return;

        Movement playerMovement = player.GetComponent<Movement>();

        if (playerMovement != null && movement.ActualPos == playerMovement.ActualPos)
            KillPlayer();
    }

    /*
     * InitializeDemon
     * ---------------
     * Prepares the demon's Movement component so it behaves exactly
     * as it would during a real game. This is required for tests to work.
     */
    void InitializeDemon()
    {
        if (player == null)
        {
            Movement[] allMovements = FindObjectsByType<Movement>(FindObjectsSortMode.None);

            foreach (Movement m in allMovements)
            {
                if (m != null && m.isPlayer)
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
     * OnPlayerCompletedLap
     * --------------------
     * Called whenever the player completes a lap.
     */
    public void OnPlayerCompletedLap()
    {
        if (!data.requiresPlayerLap)
            return;

        playerLaps++;

        if (!isActive && playerLaps >= data.lapsToActivate)
            ActivateDemon();
    }

    /*
     * ActivateDemon
     * -------------
     * Spawns the Demon cup and token, initializes Movement,
     * and places the token safely behind the player.
     */
    void ActivateDemon()
    {
        SpawnEnemy();

        demonTokenInstance = Instantiate(data.tilePrefab);
        movement = demonTokenInstance.GetComponent<Movement>();

        InitializeDemon();

        PlaceEnemyBehindPlayer(18);

        isActive = true;
    }

    /*
     * StartTurn
     * ---------
     * Rolls 3d6 and moves the Demon.
     */
    public override void StartTurn()
    {
        if (!isActive || movement == null)
            return;

        int d1 = EnemyDice.ThrowDice();
        int d2 = EnemyDice.ThrowDice();
        int d3 = EnemyDice.ThrowDice();

        if (d1 == 6 && d2 == 6 && d3 == 6)
        {
            KillPlayer();
            return;
        }

        movement.StartMovingFixed(d1 + d2 + d3);
    }

    void KillPlayer()
    {
        Debug.Log("Player killed by the Demon.");
    }

    // ---------------- TEST BUTTONS ----------------

    [ContextMenu("Test: Spawn Demon")]
    void TestSpawn()
    {
        ActivateDemon();
    }

    [ContextMenu("Test: Start Turn")]
    void TestStartTurn()
    {
        StartTurn();
    }

    [ContextMenu("Test: Add 1 Lap")]
    void TestAddLap()
    {
        OnPlayerCompletedLap();
    }

    [ContextMenu("Test: Force 666 (Demon)")]
    void TestForce666()
    {
        KillPlayer();
    }

    [ContextMenu("Test: Player Force 666")]
    void TestPlayerForce666()
    {
        if (!isActive)
            ActivateDemon();
    }
}
