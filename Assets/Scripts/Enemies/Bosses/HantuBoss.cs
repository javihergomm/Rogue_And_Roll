using UnityEngine;

/*
 * HantuBoss
 * ---------
 * Enemy that rolls 1D6 each turn and has a chance to add +2.
 * Moves forward by the final roll value.
 * If it reaches the same tile as the player, the player is killed.
 * Uses the standard spawn system (lapsToActivate) with no special spawn logic.
 */
public class HantuBoss : EnemyBase
{
    [Header("Hantu Settings")]
    [Range(0f, 1f)]
    public float chanceToAddTwo = 0.25f;

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
            Debug.LogError("HantuBoss: Token prefab has no Movement component!");
            yield break;
        }

        InitializeHantu();

        // Place behind player (Hantu uses max roll 6)
        PlaceEnemyBehindPlayer(6);

        movement.TeleportToPosition(movement.ActualPos);

        isActive = true;

        EnemyManager.Instance.ActivateEnemy(this);
    }

    // ---------------------------------------------------------

    private void InitializeHantu()
    {
        CachePlayerMovement();

        if (playerMovement == null)
            return;

        movement.SetPositions(playerMovement.Positions);

        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;
    }

    private System.Collections.IEnumerator ActivateHantuRoutine()
    {
        // Only used for testing
        SpawnEnemy();
        yield return null;
    }

    public void ActivateHantu()
    {
        StartCoroutine(ActivateHantuRoutine());
    }

    public override void StartTurn()
    {
        if (!isActive || movement == null)
            return;

        int roll = EnemyDice.ThrowDice();

        if (Random.value <= chanceToAddTwo)
            roll += 2;

        TurnManager.NotifyEnemyRoll(roll);

        movement.StartMovingFixed(roll);
    }
}
