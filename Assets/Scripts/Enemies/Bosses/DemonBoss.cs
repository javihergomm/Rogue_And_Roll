using UnityEngine;

// Controls the demon boss enemy: activation, spawning, token creation and movement.
public class DemonBoss : EnemyBase
{
    [Header("Spawn Areas")]
    public Transform[] demonSpawnAreas;
    public Spot fixedSpawnSpot;

    [Header("Demon Token")]
    public GameObject demonTokenPrefab;
    private GameObject demonTokenInstance;

    [Header("Activation Settings")]
    public int lapsToSpawn = 1;
    private int playerLaps = 0;

    void Update()
    {
        if (!isActive || movement == null)
            return;

        Movement playerMovement = player.GetComponent<Movement>();

        if (playerMovement != null && movement.ActualPos == playerMovement.ActualPos)
            KillPlayer();
    }

    int GetValidSpawnArea()
    {
        return Random.Range(0, demonSpawnAreas.Length);
    }

    public void OnPlayerCompletedLap()
    {
        playerLaps++;

        if (!isActive && playerLaps >= lapsToSpawn)
            ActivateDemon();
    }

    void ActivateDemon()
    {
        int spawnIndex = GetValidSpawnArea();

        SpawnEnemy(demonSpawnAreas[spawnIndex]);

        demonTokenInstance = Instantiate(demonTokenPrefab);

        movement = demonTokenInstance.GetComponent<Movement>();

        // USE THE SAME POSITIONS AS THE PLAYER
        Movement playerMovement = player.GetComponent<Movement>();
        movement.positions = playerMovement.positions;

        movement.ActualPos = fixedSpawnSpot.index;
        movement.transform.position = movement.positions[fixedSpawnSpot.index].position;
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

        int total = d1 + d2 + d3;

        movement.StartMovingFixed(total);
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
