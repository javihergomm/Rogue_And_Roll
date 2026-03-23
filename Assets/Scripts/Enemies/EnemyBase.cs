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

    // Audio
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    // ============================================================
    // REGISTER
    // ============================================================
    protected void RegisterEnemy()
    {
        EnemyManager.Instance.ActivateEnemy(this);
    }

    // ============================================================
    // CACHE PLAYER
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
    // OPPOSITE SPAWN
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
    // UNIFIED SPAWN FOR ALL BOSSES
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

        // 1. Get player spawn from CharacterSelectManager
        string playerSpawnName = CharacterSelectManager.Instance.SelectedCharacter.spawnPointName;
        Debug.Log("ENEMYBASE PLAYER SPAWN NAME = " + playerSpawnName);

        // 2. Get opposite spawn
        string enemyCupSpawnName = GetOppositeSpawn(playerSpawnName);
        Debug.Log("ENEMYBASE OPPOSITE SPAWN = " + enemyCupSpawnName);

        // 3. Find opposite spawn
        Transform cupSpawn = FindSpawnPoint(enemyCupSpawnName);
        Debug.Log("ENEMYBASE FOUND SPAWN TRANSFORM = " + (cupSpawn != null ? cupSpawn.name : "NULL"));

        if (cupSpawn == null)
        {
            Debug.LogError("EnemyBase: Opposite spawn not found: " + enemyCupSpawnName);
            yield break;
        }

        // 4. Instantiate CUP at opposite spawn
        CupInstance = Instantiate(data.cupPrefab, cupSpawn.position, cupSpawn.rotation);

        // 5. Instantiate TILE
        TokenInstance = Instantiate(data.tilePrefab);
        movement = TokenInstance.GetComponent<Movement>();

        if (movement == null)
        {
            Debug.LogError("EnemyBase: tilePrefab has no Movement component!");
            yield break;
        }

        // 6. Configure Movement
        movement.SetPositions(playerMovement.Positions);
        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;

        // 7. Place behind the player
        int maxRoll = this is DemonBoss ? 18 : 6;
        PlaceEnemyBehindPlayer(maxRoll);

        movement.TeleportToPosition(movement.ActualPos);

        isActive = true;
        RegisterEnemy();

        // Play spawn sound AFTER everything is placed
        PlaySpawnSound();
    }

    // ============================================================
    // PLACE BEHIND PLAYER
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
    // PLAYER DEATH
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
    // ENEMY MOVEMENT BLOCK (tile effects)
    // ============================================================
    protected bool IsEnemyMovementBlocked()
    {
        return StatManager.Instance.PreventEnemyMovementThisTurn;
    }

    // ============================================================
    // AUDIO
    // ============================================================
    protected void PlaySpawnSound()
    {
        if (data.spawnSFX == null || audioSource == null)
            return;

        audioSource.PlayOneShot(data.spawnSFX);
    }

    // ============================================================
    // ABSTRACT METHODS
    // ============================================================
    public abstract void StartTurn();
    public virtual void ActivateForTesting() { SpawnEnemy(); }
}
