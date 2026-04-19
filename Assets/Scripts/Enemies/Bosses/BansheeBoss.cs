using UnityEngine;

/*
 * BansheeBoss
 * -----------
 * Enemy that does not move. Instead, it pulls the player toward its tile
 * based on a dice roll. The Banshee only kills the player if the player
 * ends on her tile *as a result of being pulled by her*.
 * If the player reaches her from behind by normal movement, it does NOT kill.
 * The Banshee also does not despawn by durability.
 */
public class BansheeBoss : EnemyBase
{
    [Header("Banshee Settings")]
    public int maxRoll = 6;

    private bool playerWasPulled = false;

    private void Update()
    {
        if (!isActive || movement == null || playerMovement == null)
            return;

        if (ShouldKillPlayer())
            KillPlayerNow();
    }

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

        playerWasPulled = true;

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

        if (ShouldKillPlayer())
        {
            KillPlayerNow();
            return;
        }

        playerWasPulled = false;

        TurnManager.Instance.NotifyEnemyFinishedMovement();
    }

    private bool ShouldKillPlayer()
    {
        return playerWasPulled &&
               movement.ActualPos == playerMovement.ActualPos;
    }
}
