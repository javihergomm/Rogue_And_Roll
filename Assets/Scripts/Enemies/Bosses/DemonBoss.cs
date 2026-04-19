using UnityEngine;

/*
 * DemonBoss
 * ---------
 * Enemigo especial que puede aparecer:
 *  - Por vueltas (lapsToActivate en EnemySO)
 *  - Por tirada 6+6+6 (roll == 18)
 *
 * Usa EnemyBase.SpawnEnemy(), por lo que:
 *  - Se instancia en el spawn opuesto al jugador
 *  - Se instancia la cup correctamente
 *  - Se instancia la tile correctamente
 *  - Se reproduce el sonido de spawn
 *  - Se colocan posiciones y movimiento automaticamente
 */
public class DemonBoss : EnemyBase
{
    /*
     * Determina si el demonio debe aparecer por tirada especial.
     * El EnemyManager llama a este metodo cuando el jugador tira los dados.
     */
    public bool ShouldSpawnByRoll(int roll)
    {
        return roll == 18; // 6+6+6
    }

    private void Update()
    {
        // Si el demonio no esta activo, no hace nada
        if (!isActive || movement == null || playerMovement == null)
            return;

        // Si cae encima del jugador, lo mata
        if (movement.ActualPos == playerMovement.ActualPos)
            KillPlayerNow();
    }

    /*
     * SpawnEnemy
     * ----------
     * Usamos la version de EnemyBase, que ya hace:
     *  - Buscar spawn opuesto
     *  - Instanciar cup en el spawn correcto
     *  - Instanciar tile
     *  - Configurar Movement
     *  - Colocar al enemigo detras del jugador
     *  - Reproducir sonido
     */
    public override void SpawnEnemy()
    {
        base.SpawnEnemy();
    }

    /*
     * StartTurn
     * ---------
     * El demonio tira 3 dados.
     * Si saca 6-6-6, mata al jugador inmediatamente.
     * Si no, avanza la suma total.
     */
    public override void StartTurn()
    {
        if (!isActive || movement == null)
            return;

        int d1 = EnemyDice.ThrowDice();
        int d2 = EnemyDice.ThrowDice();
        int d3 = EnemyDice.ThrowDice();

        int total = d1 + d2 + d3;

        // Notificar la tirada al TurnManager
        TurnManager.NotifyEnemyRoll(total);

        // Muerte instantanea si saca 6-6-6
        if (d1 == 6 && d2 == 6 && d3 == 6)
        {
            KillPlayerNow();
            return;
        }

        // Movimiento normal
        movement.StartMovingFixed(total);
    }
}
