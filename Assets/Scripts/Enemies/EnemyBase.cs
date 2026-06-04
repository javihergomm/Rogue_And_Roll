using UnityEngine;
using System.Collections.Generic;

/*
 * EnemyBase
 * ---------
 * Base class for all enemy types.
 * Handles:
 *  - Opposite-spawn logic based on the player's spawn point
 *  - Cup and token instantiation
 *  - Movement setup and positioning
 *  - Spawn distance based on 2/3 of max roll
 *  - Automatic registration in EnemyManager
 *  - Automatic despawn after a durability-lap duration
 *  - Player kill logic and extra-life handling
 *  - Shared audio system for spawn SFX
 *
 * Individual enemies only need to implement StartTurn().
 */
public abstract class EnemyBase : MonoBehaviour
{
    public EnemySO data;

    public Movement movement;
    public Transform player;

    protected Movement playerMovement;

    public GameObject CupInstance;
    public GameObject TokenInstance;

    public bool isActive;

    // Track if this enemy has already spawned once
    public bool HasSpawnedOnce { get; private set; } = false;

    public void MarkAsSpawned()
    {
        HasSpawnedOnce = true;
    }


    // Durability tracking
    private float spawnLap;
    private float despawnLap;

    // Audio
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    // Registers the enemy in EnemyManager
    protected void RegisterEnemy()
    {
        EnemyManager.Instance.ActivateEnemy(this);
    }

    // Finds and caches the player's Movement component
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

    // Determines the opposite spawn point name
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

    // Finds a spawn point in the scene
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

    // Unified spawn logic for all enemies
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

        // Get player spawn
        string playerSpawnName = CharacterSelectManager.Instance.SelectedCharacter.spawnPointName;

        // Get opposite spawn
        string enemyCupSpawnName = GetOppositeSpawn(playerSpawnName);
        Transform cupSpawn = FindSpawnPoint(enemyCupSpawnName);

        if (cupSpawn == null)
            yield break;

        // Instantiate cup
        CupInstance = Instantiate(data.cupPrefab, cupSpawn.position, cupSpawn.rotation);

        // Instantiate token
        TokenInstance = Instantiate(data.tilePrefab);
        movement = TokenInstance.GetComponent<Movement>();

        if (movement == null)
        {
            Debug.LogError("EnemyBase: tilePrefab has no Movement component!");
            yield break;
        }

        // Configure movement
        movement.SetPositions(playerMovement.Positions);
        movement.startPos = movement.ActualPos;
        movement.lastPos = movement.ActualPos;

        // Spawn distance = 2/3 of max roll
        int maxRoll = this is DemonBoss ? 18 : 6;
        int spawnDistance = Mathf.RoundToInt(maxRoll * 0.66f);

        PlaceEnemyBehindPlayer(spawnDistance);

        movement.TeleportToPosition(movement.ActualPos);

        // Activate
        isActive = true;
        data.hasSpawnedOnce = true;
        RegisterEnemy();
        MarkAsSpawned();
        // Durability setup
        spawnLap = playerMovement.Round - 1;
        despawnLap = spawnLap + data.durabilityLaps;

        PlaySpawnSound();
    }

    // Places the enemy behind the player by a given distance
    protected void PlaceEnemyBehindPlayer(int distance)
    {
        CachePlayerMovement();

        if (playerMovement == null)
            return;

        int playerPos = playerMovement.ActualPos;
        int enemyPos = playerPos - distance;

        if (enemyPos < 0)
            enemyPos += playerMovement.Positions.Length;

        movement.ActualPos = enemyPos;
    }

    // Checks if the enemy should despawn based on durability
    public bool ShouldDespawn(float currentLap)
    {
        if (this is BansheeBoss)
            return false;

        return currentLap >= despawnLap;
    }

    // Handles player death and extra-life logic
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

        if (player != null){
            Destroy(player.gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Muerte");
            }
    }

    // Checks if enemy movement is blocked by tile effects
    protected bool IsEnemyMovementBlocked()
    {
        return StatManager.Instance.PreventEnemyMovementThisTurn;
    }

    // Plays the spawn sound
    protected void PlaySpawnSound()
    {
        if (data.spawnSFX == null || audioSource == null)
            return;

        audioSource.PlayOneShot(data.spawnSFX);
    }

    // Abstract turn logic for each enemy type
    public abstract void StartTurn();

    public virtual void ActivateForTesting()
    {
        SpawnEnemy();
    }
}
