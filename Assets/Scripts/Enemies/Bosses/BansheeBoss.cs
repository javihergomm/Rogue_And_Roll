using UnityEngine;

/*
 * BansheeBoss
 * -----------
 * Enemy that does not move. Instead, it pulls the player toward its tile
 * based on a dice roll. The Banshee only kills the player if the player
 * ends on her tile as a result of being pulled by her.
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
        {
            Debug.Log("[Banshee] Player pulled into her tile. Killing player.");
            KillPlayerNow();
        }
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
        {
            Debug.Log("[Banshee] Movement blocked this turn.");
            TurnManager.Instance.ForceEnemyTurnEnd();
            return;
        }

        int roll = EnemyDice.ThrowDice();
        Debug.Log("[Banshee] Rolled: " + roll);
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

        Debug.Log("[Banshee] Pulling player " + steps + " steps. Ignore bridge: " + ignoreBridge);

        playerMovement.OnMovementFinished += OnPlayerPulledFinished;
        playerMovement.StartMovingFixed(steps);
    }

    private void OnPlayerPulledFinished()
    {
        playerMovement.OnMovementFinished -= OnPlayerPulledFinished;

        if (ShouldKillPlayer())
        {
            Debug.Log("[Banshee] Player ended on Banshee tile after pull. Killing player.");
            KillPlayerNow();
            return;
        }

        playerWasPulled = false;

        Debug.Log("[Banshee] Pull finished. Enemy turn ends.");
        TurnManager.Instance.ForceEnemyTurnEnd();
    }

    private bool ShouldKillPlayer()
    {
        return playerWasPulled &&
               movement.ActualPos == playerMovement.ActualPos;
    }
}
