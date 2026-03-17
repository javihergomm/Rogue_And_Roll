using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public EnemySO data;

    public Movement movement;
    public Transform player;

    protected Movement playerMovement;

    public GameObject CupInstance;
    public GameObject TokenInstance;

    public bool isActive;

    // ============================================================
    // REGISTRO
    // ============================================================
    protected void RegisterEnemy()
    {
        EnemyManager.Instance.ActivateEnemy(this);
    }

    // ============================================================
    // CACHEO DEL JUGADOR
    // ============================================================
    protected void CachePlayerMovement()
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
        }

        if (playerMovement == null && player != null)
            playerMovement = player.GetComponent<Movement>();
    }

    // ============================================================
    // SPAWN OPUESTO
    // ============================================================
    protected string GetOppositeSpawn(string playerSpawn)
    {
        string p = playerSpawn.ToLower();

        if (p.Contains("red")) return "Spawn_Yellow";
        if (p.Contains("yellow")) return "Spawn_Red";
        if (p.Contains("blue")) return "Spawn_Green";
        if (p.Contains("green")) return "Spawn_Blue";

        Debug.LogWarning("EnemyBase: Could not determine opposite spawn for " + playerSpawn);
        return playerSpawn;
    }

    protected Transform FindSpawnPoint(string spawnName)
    {
        GameObject obj = GameObject.Find(spawnName);
        if (obj == null)
        {
            Debug.LogError("EnemyBase: Spawn point not found: " + spawnName);
            return null;
        }
        return obj.transform;
    }

    // ============================================================
    // SPAWN UNIFICADO PARA TODOS LOS JEFES
    // ============================================================
    public virtual void SpawnEnemy()
    {
        StartCoroutine(SpawnRoutine());
    }

    private System.Collections.IEnumerator SpawnRoutine()
    {
        yield return null;

        CachePlayerMovement();

        if (playerMovement == null)
        {
            Debug.LogError("EnemyBase: PlayerMovement not found.");
            yield break;
        }

        // 1. Obtener spawn del jugador desde CharacterSelectManager
        string playerSpawnName = CharacterSelectManager.Instance.SelectedCharacter.spawnPointName;

        // 2. Obtener spawn opuesto
        string enemyCupSpawnName = GetOppositeSpawn(playerSpawnName);

        // 3. Buscar spawn opuesto
        Transform cupSpawn = FindSpawnPoint(enemyCupSpawnName);

        if (cupSpawn == null)
        {
            Debug.LogError("EnemyBase: Opposite spawn not found: " + enemyCupSpawnName);
            yield break;
        }

        // 4. Instanciar CUP en el spawn opuesto
        CupInstance = Instantiate(data.cupPrefab, cupSpawn.position, cupSpawn.rotation);

        // 5. Instanciar TILE
        TokenInstance = Instantiate(data.tilePrefab);
        movement = TokenInstance.GetComponent<Movement>();

        if (movement == null)
        {
            Debug.LogError("EnemyBase: tilePrefab has no Movement component!");
            yield break;
        }

        // 6. Configurar Movement
        movement.SetPositions(playerMovement.Positions);
        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;

        // 7. Colocar detrás del jugador
        int maxRoll = this is DemonBoss ? 18 : 6;
        PlaceEnemyBehindPlayer(maxRoll);

        movement.TeleportToPosition(movement.ActualPos);

        isActive = true;
        RegisterEnemy();
    }

    // ============================================================
    // COLOCAR DETRÁS DEL JUGADOR
    // ============================================================
    protected void PlaceEnemyBehindPlayer(int maxRoll)
    {
        CachePlayerMovement();

        if (playerMovement == null)
            return;

        int playerPos = playerMovement.ActualPos;
        int enemyPos = playerPos - maxRoll;

        if (enemyPos < 0)
            enemyPos += playerMovement.Positions.Length;

        movement.ActualPos = enemyPos;
    }

    // ============================================================
    // MUERTE DEL JUGADOR
    // ============================================================
    protected void KillPlayerNow()
    {
        CachePlayerMovement();

        PassiveContext ctx = StatManager.Instance.PassiveCtx;

        if (ctx.PlayerLives > 1)
        {
            ctx.PlayerLives--;
            ctx.ExtraLifeUsed = true;

            playerMovement.TeleportToPosition(playerMovement.startPos);

            int maxRoll = this is DemonBoss ? 18 : 6;
            int distance = Mathf.RoundToInt(maxRoll * 0.66f);

            PlaceEnemyBehindPlayer(distance);
            movement.TeleportToPosition(movement.ActualPos);

            return;
        }

        if (player != null)
            Destroy(player.gameObject);
    }
    // ============================================================
    // BLOQUEO DE MOVIMIENTO ENEMIGO (efectos de casillas)
    // ============================================================
    protected bool IsEnemyMovementBlocked()
    {
        return StatManager.Instance.PreventEnemyMovementThisTurn;
    }

    // ============================================================
    // MÉTODOS ABSTRACTOS
    // ============================================================
    public abstract void StartTurn();
    public virtual void ActivateForTesting() { }
}
