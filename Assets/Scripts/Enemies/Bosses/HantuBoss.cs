using UnityEngine;

/*
 * HantuBoss
 * ---------
 * Enemy that rolls 1D6 and has a probability of adding +2 to its roll.
 * Kills the player if it reaches the same tile.
 */
public class HantuBoss : EnemyBase
{
    [Header("Hantu Settings")]
    [Range(0f, 1f)]
    public float chanceToAddTwo = 0.25f; // 25 percent by default

    private void Update()
    {
        if (!isActive || movement == null || player == null)
            return;

        Movement playerMovement = player.GetComponent<Movement>();

        if (playerMovement != null && movement.ActualPos == playerMovement.ActualPos)
            KillPlayerNow();
    }

    public void ActivateHantu()
    {
        StartCoroutine(ActivateHantuRoutine());
    }

    private System.Collections.IEnumerator ActivateHantuRoutine()
    {
        // 1. Spawn logic
        SpawnEnemy();

        // 2. Wait a frame
        yield return null;

        // 3. Instantiate visual
        CupInstance = Instantiate(data.tilePrefab);

        // 4. Assign movement
        movement = CupInstance.GetComponent<Movement>();

        if (movement == null)
        {
            Debug.LogError("HantuBoss: The token prefab has NO Movement component!");
            yield break;
        }

        // 5. Find player + set positions
        InitializeHantu();

        // 6. Place behind player (max roll = 6)
        PlaceEnemyBehindPlayer(6);

        // 7. Teleport visual
        movement.TeleportToPosition(movement.ActualPos);

        // 8. Activate
        isActive = true;

        // 9. Register
        EnemyManager.Instance.ActivateEnemy(this);
    }

    private void InitializeHantu()
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
                Debug.LogWarning("HantuBoss: No player found.");
                return;
            }
        }

        Movement playerMovement = player.GetComponent<Movement>();
        if (playerMovement == null)
        {
            Debug.LogError("HantuBoss: Player has no Movement component.");
            return;
        }

        movement.SetPositions(playerMovement.Positions);
        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;
    }

    public override void StartTurn()
    {
        if (!isActive || movement == null)
            return;

        // 1. Roll D6 using EnemyDice
        int roll = EnemyDice.ThrowDice();

        // 2. Chance to add +2
        if (Random.value <= chanceToAddTwo)
        {
            roll += 2;
            Debug.Log("HantuBoss: +2 applied! New roll = " + roll);
        }

        // 3. Notify UI
        TurnManager.NotifyEnemyRoll(roll);

        // 4. Move
        movement.StartMovingFixed(roll);
    }


    public override void ActivateForTesting()
    {
        ActivateHantu();
    }
}
