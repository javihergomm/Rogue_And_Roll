using UnityEngine;

/*
 * BansheeBoss
 * -----------
 * Enemy that does not move. Instead, it pulls the player toward its tile
 * based on a dice roll. If the player ends on the same tile, the player dies.
 * Uses the standard lap-based spawn system with no special spawn logic.
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

    // ---------------------------------------------------------
    // Usa el sistema de EnemyBase (spawn opuesto, sonido, etc.)
    // ---------------------------------------------------------
    public override void SpawnEnemy()
    {
        base.SpawnEnemy();
    }

    public override void StartTurn()
    {
        if (!isActive || movement == null || playerMovement == null)
            return;

        if (IsEnemyMovementBlocked())
            return;

        int roll = EnemyDice.ThrowDice();

        TurnManager.NotifyEnemyRoll(roll);

        PullPlayer(roll);
    }

    private void PullPlayer(int roll)
    {
        int playerPos = playerMovement.ActualPos;
        int bansheePos = movement.ActualPos;
        int total = playerMovement.Positions.Length;

        int forwardDist = (bansheePos - playerPos + total) % total;
        int backwardDist = (playerPos - bansheePos + total) % total;

        bool moveForward = forwardDist <= backwardDist;

        int steps = moveForward ? roll : -roll;

        bool ignoreBridge =
            SpotConnectionManager.Instance.WouldBridgeMoveAway(playerPos, steps, bansheePos);

        playerMovement.ignoreBridgeThisMove = ignoreBridge;

        playerMovement.OnMovementFinished += OnPlayerPulledFinished;

        playerMovement.StartMovingFixed(steps);
    }

    private void OnPlayerPulledFinished()
    {
        playerMovement.OnMovementFinished -= OnPlayerPulledFinished;

        if (playerMovement.ActualPos == movement.ActualPos)
        {
            KillPlayerNow();
            return;
        }

        TurnManager.Instance.NotifyEnemyFinishedMovement();
    }
}
