using UnityEngine;

/*
 * HantuBoss
 * ---------
 * Enemy that rolls 1D6 each turn and has a chance to add +2 to the result.
 * Moves forward by the final roll value.
 * If the Hantu reaches the same tile as the player, the player is killed.
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

        // Kill if Hantu reaches the same tile as the player
        if (movement.ActualPos == playerMovement.ActualPos)
            KillPlayerNow();
    }

    public void ActivateHantu()
    {
        StartCoroutine(ActivateHantuRoutine());
    }

    private System.Collections.IEnumerator ActivateHantuRoutine()
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
            Debug.LogError("HantuBoss: The token prefab has NO Movement component!");
            yield break;
        }

        // 5. Initialize references and board positions
        InitializeHantu();

        // 6. Place Hantu behind player (max roll = 6)
        PlaceEnemyBehindPlayer(6);

        // 7. Teleport visual to correct tile
        movement.TeleportToPosition(movement.ActualPos);

        // 8. Activate Hantu
        isActive = true;

        // 9. Register enemy in TurnManager
        EnemyManager.Instance.ActivateEnemy(this);
    }

    private void InitializeHantu()
    {
        // Cache player transform and movement
        CachePlayerMovement();

        if (playerMovement == null)
            return;

        // Copy board positions from player
        movement.SetPositions(playerMovement.Positions);

        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;
    }

    public override void StartTurn()
    {
        if (!isActive || movement == null)
            return;

        // 1. Roll D6
        int roll = EnemyDice.ThrowDice();

        // 2. Chance to add +2
        if (Random.value <= chanceToAddTwo)
        {
            roll += 2;
            Debug.Log("HantuBoss: +2 applied! New roll = " + roll);
        }

        // 3. Notify UI
        TurnManager.NotifyEnemyRoll(roll);

        // 4. Move Hantu
        movement.StartMovingFixed(roll);
    }

    public override void ActivateForTesting()
    {
        ActivateHantu();
    }
}
