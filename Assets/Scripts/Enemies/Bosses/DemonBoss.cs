using UnityEngine;

/*
 * DemonBoss
 * ---------
 * Special enemy that can appear:
 *  - By laps (lapsToActivate in EnemySO)
 *  - By roll 6+6+6 (total 18)
 */
public class DemonBoss : EnemyBase
{
    public bool ShouldSpawnByRoll(int roll)
    {
        return roll == 18;
    }

    private void Update()
    {
        if (!isActive || movement == null || playerMovement == null)
            return;

        if (movement.ActualPos == playerMovement.ActualPos)
        {
            Debug.Log("[Demon] Landed on player tile. Killing player.");
            KillPlayerNow();
        }
    }

    public override void SpawnEnemy()
    {
        base.SpawnEnemy();
    }

    public override void StartTurn()
    {
        if (!isActive || movement == null)
            return;

        if (IsEnemyMovementBlocked())
        {
            Debug.Log("[Demon] Movement blocked this turn.");
            TurnManager.Instance.ForceEnemyTurnEnd();
            return;
        }

        int d1 = EnemyDice.ThrowDice();
        int d2 = EnemyDice.ThrowDice();
        int d3 = EnemyDice.ThrowDice();

        int total = d1 + d2 + d3;

        Debug.Log("[Demon] Rolled: " + d1 + " + " + d2 + " + " + d3 + " = " + total);
        TurnManager.NotifyEnemyRoll(total);

        if (d1 == 6 && d2 == 6 && d3 == 6)
        {
            Debug.Log("[Demon] Rolled 6-6-6. Killing player instantly.");
            KillPlayerNow();
            TurnManager.Instance.ForceEnemyTurnEnd();
            return;
        }

        movement.OnMovementFinished += OnMovementFinished;
        movement.StartMovingFixed(total);
    }


    private void OnMovementFinished()
    {
        movement.OnMovementFinished -= OnMovementFinished;
        Debug.Log("[Demon] Movement finished. Enemy turn ends.");
        TurnManager.Instance.ForceEnemyTurnEnd();
    }
}
