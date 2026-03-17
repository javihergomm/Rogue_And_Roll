using UnityEngine;

public class DemonBoss : EnemyBase
{
    public bool ShouldSpawnByRoll(int roll)
    {
        return roll == 18; // 6+6+6
    }

    private void Update()
    {
        if (!isActive || movement == null || playerMovement == null)
            return;

        if (movement.ActualPos == playerMovement.ActualPos)
            KillPlayerNow();
    }

    // ---------------------------------------------------------
    // NEW: SpawnEnemy override so EnemyManager can activate boss
    // ---------------------------------------------------------
    public override void SpawnEnemy()
    {
        StartCoroutine(SpawnRoutine());
    }

    private System.Collections.IEnumerator SpawnRoutine()
    {
        // Wait one frame so the host prefab is fully initialized
        yield return null;

        // Instanciar la cup
        CupInstance = Instantiate(data.cupPrefab);

        // Instanciar la tile
        GameObject token = Instantiate(data.tilePrefab);
        movement = token.GetComponent<Movement>();


        if (movement == null)
        {
            Debug.LogError("DemonBoss: Token has no Movement component!");
            yield break;
        }

        InitializeDemon();

        // Place behind player
        PlaceEnemyBehindPlayer(18);

        movement.TeleportToPosition(movement.ActualPos);

        isActive = true;

        EnemyManager.Instance.ActivateEnemy(this);
    }

    // ---------------------------------------------------------

    private void InitializeDemon()
    {
        CachePlayerMovement();

        movement.SetPositions(playerMovement.Positions);

        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;
    }

    private void ActivateDemon()
    {
        StartCoroutine(ActivateDemonRoutine());
    }

    private System.Collections.IEnumerator ActivateDemonRoutine()
    {
        // This is only used for testing
        SpawnEnemy();
        yield return null;
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
            KillPlayerNow();
            return;
        }

        movement.StartMovingFixed(total);
    }

    public override void ActivateForTesting()
    {
        ActivateDemon();
    }
}
