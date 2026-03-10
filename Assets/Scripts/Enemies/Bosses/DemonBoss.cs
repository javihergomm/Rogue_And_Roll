using UnityEngine;

/*
 * DemonBoss
 * ---------
 * Boss that activates after the player completes a required number of laps.
 * Once active, it rolls three dice each turn and moves forward by the sum.
 * If all three dice roll a 6, the player is instantly killed.
 * If the demon reaches the same tile as the player, the player is killed.
 */
public class DemonBoss : EnemyBase
{
    private int playerLaps = 0;

    private void Update()
    {
        if (!isActive || movement == null || playerMovement == null)
            return;

        // Kill if demon reaches the same tile as the player
        if (movement.ActualPos == playerMovement.ActualPos)
            KillPlayerNow();
    }

    private void InitializeDemon()
    {
        // Cache player + movement using EnemyBase helper
        CachePlayerMovement();

        if (playerMovement == null)
        {
            Debug.LogError("DemonBoss: Player has no Movement component.");
            return;
        }

        // Copy board positions from player
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

    private System.Collections.IEnumerator ActivateDemonRoutine()
    {
        // 1. Spawn logic object
        SpawnEnemy();

        // 2. Wait one frame
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

        // 5. Initialize movement and player reference
        InitializeDemon();

        // 6. Place demon behind player
        PlaceEnemyBehindPlayer(18);

        // 7. Teleport visual to correct tile
        movement.TeleportToPosition(movement.ActualPos);

        // 8. Activate demon
        isActive = true;

        // 9. Register enemy in TurnManager
        EnemyManager.Instance.ActivateEnemy(this);
    }

    public override void StartTurn()
    {
        if (!isActive || movement == null)
            return;

        // Roll three dice
        int d1 = EnemyDice.ThrowDice();
        int d2 = EnemyDice.ThrowDice();
        int d3 = EnemyDice.ThrowDice();

        int total = d1 + d2 + d3;

        TurnManager.NotifyEnemyRoll(total);

        // Kill if triple 6
        if (d1 == 6 && d2 == 6 && d3 == 6)
        {
            KillPlayerNow();
            return;
        }

        // Move demon
        movement.StartMovingFixed(total);
    }

    public override void ActivateForTesting()
    {
        ActivateDemon();
    }
}
