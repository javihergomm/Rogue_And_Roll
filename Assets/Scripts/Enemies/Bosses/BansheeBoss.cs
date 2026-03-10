using UnityEngine;

/*
 * BansheeBoss
 * -----------
 * Handles a boss that does not move toward the player.
 * The boss pulls the player toward its tile based on a dice roll.
 * If the player ends on the same tile as the boss, the player is killed.
 * This class is responsible for spawning the visual token, initializing
 * movement references, performing the pull action, and notifying the
 * turn system when the enemy turn is finished.
 */
public class BansheeBoss : EnemyBase
{
    [Header("Banshee Settings")]
    public int maxRoll = 6;

    private void Update()
    {
        if (!isActive || movement == null || playerMovement == null)
            return;

        if (movement.ActualPos == playerMovement.ActualPos)
            KillPlayerNow();
    }

    public void ActivateBanshee()
    {
        StartCoroutine(ActivateBansheeRoutine());
    }

    private System.Collections.IEnumerator ActivateBansheeRoutine()
    {
        // Spawn logic hook for non-visual setup
        SpawnEnemy();
        yield return null;

        // Instantiate visual token
        CupInstance = Instantiate(data.tilePrefab);

        if (CupInstance == null)
        {
            Debug.LogError("BansheeBoss: Failed to instantiate tilePrefab.");
            yield break;
        }

        // Ensure the spawned object is active and has a valid scale
        if (!CupInstance.activeSelf)
            CupInstance.SetActive(true);

        if (CupInstance.transform.localScale == Vector3.zero)
            CupInstance.transform.localScale = Vector3.one;

        // Get Movement component from the token
        movement = CupInstance.GetComponent<Movement>();

        if (movement == null)
        {
            Debug.LogError("BansheeBoss: The token prefab has NO Movement component!");
            yield break;
        }

        // Ensure the movement GameObject is active
        if (!movement.gameObject.activeSelf)
            movement.gameObject.SetActive(true);

        // Try to enable any renderer to avoid accidental invisibility
        var rend = CupInstance.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.enabled = true;
            Debug.Log("BansheeBoss: Renderer enabled on spawned token.");
        }
        else
        {
            Debug.LogWarning("BansheeBoss: No Renderer found on token prefab.");
        }

        // Mark this enemy active so StartTurn checks pass
        isActive = true;

        // Cache player movement reference and wait until positions are available
        CachePlayerMovement();

        int safetyFrames = 0;
        while ((playerMovement == null || playerMovement.Positions == null || playerMovement.Positions.Length == 0) && safetyFrames < 10)
        {
            // Wait a frame for other systems to initialize
            yield return null;
            CachePlayerMovement();
            safetyFrames++;
        }

        if (playerMovement == null)
        {
            Debug.LogError("BansheeBoss: Could not find player movement after waiting.");
            yield break;
        }

        // Assign positions to this token's movement
        movement.SetPositions(playerMovement.Positions);

        // Initialize movement indices
        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;

        // Place enemy behind player using a safe normalization to 1..N indexing
        PlaceEnemyBehindPlayerSafe(maxRoll);

        // Teleport visual token to the computed position if valid
        int targetIndex = movement.ActualPos;
        if (movement.Positions != null && movement.Positions.Length > 0 &&
            targetIndex >= 1 && targetIndex <= movement.Positions.Length)
        {
            movement.TeleportToPosition(targetIndex);
        }
        else
        {
            Debug.LogWarning("BansheeBoss: Teleport skipped, invalid positions or index.");
        }

        // Register the enemy with managers now that it is fully initialized
        EnemyManager.Instance.ActivateEnemy(this);

        yield break;
    }

    // Safe placement that normalizes to 1..total indexing
    private void PlaceEnemyBehindPlayerSafe(int maxRoll)
    {
        CachePlayerMovement();

        if (playerMovement == null)
            return;

        int playerPos = playerMovement.ActualPos; // expected 1..N
        int total = playerMovement.Positions.Length;

        // Compute enemy position behind player
        int enemyPos = playerPos - maxRoll;

        // Normalize to 1..total
        while (enemyPos < 1)
            enemyPos += total;
        while (enemyPos > total)
            enemyPos -= total;

        movement.ActualPos = enemyPos;

        Debug.Log("Enemy spawned behind player at spot " + enemyPos +
                  " (player at " + playerPos + ", maxRoll " + maxRoll + ")");
    }

    private void InitializeBanshee()
    {
        // Keep for compatibility; ensure playerMovement is cached and positions are set
        CachePlayerMovement();

        if (playerMovement == null)
            return;

        movement.SetPositions(playerMovement.Positions);

        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;
    }

    public override void StartTurn()
    {
        if (!isActive || movement == null || playerMovement == null)
            return;

        if (IsEnemyMovementBlocked())
        {
            Debug.Log("Banshee movement blocked this turn.");
            // Notify TurnManager that this enemy finished if needed by your flow.
            // If TurnManager expects explicit notification from enemies that do not move,
            // call TurnManager.Instance.NotifyEnemyFinishedMovement() here.
            return;
        }

        int roll = EnemyDice.ThrowDice();
        Debug.Log("Banshee rolled: " + roll);

        TurnManager.NotifyEnemyRoll(roll);

        PullPlayer(roll);
    }

    private void PullPlayer(int roll)
    {
        int playerPos = playerMovement.ActualPos;
        int bansheePos = movement.ActualPos;

        int totalSpots = playerMovement.Positions.Length;

        int forwardDist = (bansheePos - playerPos + totalSpots) % totalSpots;
        int backwardDist = (playerPos - bansheePos + totalSpots) % totalSpots;

        bool moveForward = forwardDist <= backwardDist;

        int steps = moveForward ? roll : -roll;

        bool ignoreBridge =
            SpotConnectionManager.Instance.WouldBridgeMoveAway(playerPos, steps, bansheePos);

        playerMovement.ignoreBridgeThisMove = ignoreBridge;

        Debug.Log("Banshee pulls player " + steps + " steps. Ignore bridge: " + ignoreBridge);

        playerMovement.OnMovementFinished += OnPlayerPulledFinished;

        playerMovement.StartMovingFixed(steps);
    }

    private void OnPlayerPulledFinished()
    {
        playerMovement.OnMovementFinished -= OnPlayerPulledFinished;

        if (playerMovement.ActualPos == movement.ActualPos)
        {
            Debug.Log("Banshee has pulled the player into her tile!");
            KillPlayerNow();
            return;
        }

        // Notify TurnManager that the enemy turn is finished
        TurnManager.Instance.NotifyEnemyFinishedMovement();
    }

    public override void ActivateForTesting()
    {
        ActivateBanshee();
    }
}
