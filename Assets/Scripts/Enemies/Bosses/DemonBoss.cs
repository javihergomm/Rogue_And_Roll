using UnityEngine;

/*
 * DemonBoss
 * ---------
 * Enemy behavior for the Demon Boss.
 * - Activates after a number of player laps
 * - Spawns its cup using EnemyBase logic
 * - Spawns its token on a fixed Spot
 * - Moves using dice rolls
 * - Kills the player on collision or triple six
 */
public class DemonBoss : EnemyBase
{
    public Spot fixedSpawnSpot;   // Fixed Spot where the demon token appears

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

    public void OnPlayerCompletedLap()
    {
        if (!data.requiresPlayerLap)
            return;

        playerLaps++;

        if (!isActive && playerLaps >= data.lapsToActivate)
            ActivateDemon();
    }

    void ActivateDemon()
    {
        // Spawn the cup using EnemyBase logic (opposite to the player)
        SpawnEnemy();

        // Spawn the demon token on the fixed Spot
        demonTokenInstance = Instantiate(data.tilePrefab);

        movement = demonTokenInstance.GetComponent<Movement>();

        Movement playerMovement = player.GetComponent<Movement>();
        movement.SetPositions(playerMovement.Positions);

        movement.ActualPos = fixedSpawnSpot.index;
        movement.transform.position = movement.Positions[fixedSpawnSpot.index].position;

        isActive = true;
    }

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
