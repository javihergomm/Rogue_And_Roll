using UnityEngine;

/*
 * DemonBoss
 * ---------
 * Enemy type that activates after a required number of player laps.
 * Once active, it moves using three dice rolls.
 * If all three dice roll a 6, the player is instantly killed.
 * If the demon reaches the same spot as the player, the player is killed.
 */
public class DemonBoss : EnemyBase
{
    private int playerLaps = 0;

    private void Update()
    {
        if (!isActive || movement == null || player == null)
            return;

        Movement playerMovement = player.GetComponent<Movement>();

        if (playerMovement != null && movement.ActualPos == playerMovement.ActualPos)
            KillPlayer();
    }

    /*
     * InitializeDemon
     * ---------------
     * Sets up the demon's movement system using the player's board positions.
     */
    private void InitializeDemon()
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

            if (player == null)
            {
                Debug.LogWarning("DemonBoss: InitializeDemon called but no player found.");
                return;
            }
        }

        Movement playerMovement = player.GetComponent<Movement>();
        if (playerMovement == null)
        {
            Debug.LogError("DemonBoss: Player has no Movement component.");
            return;
        }

        movement.SetPositions(playerMovement.Positions);

        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;
    }

    public void OnPlayerCompletedLap()
    {
        if (!data.requiresPlayerLap)
            return;

        playerLaps++;

        if (!isActive && playerLaps >= data.lapsToActivate)
            ActivateDemon();
    }

    private void ActivateDemon()
    {
        StartCoroutine(ActivateDemonRoutine());
    }

    /*
     * ActivateDemonRoutine
     * --------------------
     * FIXED ORDER:
     * 1. Spawn logic
     * 2. Wait a frame
     * 3. Instantiate visual
     * 4. Assign movement
     * 5. InitializeDemon
     * 6. PlaceEnemyBehindPlayer
     * 7. Teleport visual
     * 8. Register enemy
     */
    private System.Collections.IEnumerator ActivateDemonRoutine()
    {
        // 1. Spawn logic object (no visual yet)
        SpawnEnemy();

        // 2. Wait one frame so everything exists
        yield return null;

        // 3. Instantiate visual token
        CupInstance = Instantiate(data.tilePrefab);

        // 4. Assign movement
        movement = CupInstance.GetComponent<Movement>();

        if (movement == null)
        {
            Debug.LogError("DemonBoss: The demon token prefab has NO Movement component!");
            yield break;
        }

        // 5. Initialize movement positions
        InitializeDemon();

        // 6. Place demon behind player (sets ActualPos)
        PlaceEnemyBehindPlayer(18);

        // 7. Teleport visual to correct tile
        movement.TeleportToPosition(movement.ActualPos);

        // 8. Activate demon
        isActive = true;

        // 9. Register enemy only once
        EnemyManager.Instance.ActivateEnemy(this);
    }

    public override void StartTurn()
    {
        if (!isActive || movement == null)
            return;

        int d1 = EnemyDice.ThrowDice();
        int d2 = EnemyDice.ThrowDice();
        int d3 = EnemyDice.ThrowDice();

        int total = d1 + d2 + d3;

        TurnManager.NotifyEnemyRoll(total);

        if (d1 == 6 && d2 == 6 && d3 == 6)
        {
            KillPlayer();
            return;
        }

        movement.StartMovingFixed(total);
    }

    private void KillPlayer()
    {
        Debug.Log("Player killed by the Demon.");
    }

    public override void ActivateForTesting()
    {
        ActivateDemon();
    }
}
