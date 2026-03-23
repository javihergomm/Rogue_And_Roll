using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Enemy Prefabs (SO)")]
    public List<EnemySO> enemyDefinitions = new();

    [Header("Active Enemies")]
    public List<EnemyBase> enemies = new();

    private Movement playerMovement;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    private IEnumerator WaitForPlayer()
    {
        while (playerMovement == null)
        {
            Movement[] allMovements = FindObjectsByType<Movement>(FindObjectsSortMode.None);

            foreach (Movement m in allMovements)
            {
                if (m != null && m.isPlayer)
                {
                    playerMovement = m;
                    break;
                }
            }

            yield return null;
        }

        Debug.Log("EnemyManager: PlayerMovement encontrado correctamente.");
    }

    // ============================================================
    // SPAWN POR VUELTAS + SPAWN ESPECIAL DEL DEMONIO
    // ============================================================
    public void CheckSpawnConditions()
    {
        if (playerMovement == null)
            return;

        float currentLap = playerMovement.LapProgress;

        // ============================================================
        // DESPAWN AUTOMATICO POR DURABILIDAD (excepto Banshee)
        // ============================================================
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            EnemyBase e = enemies[i];

            if (e != null && e.isActive && e.ShouldDespawn(currentLap))
            {
                Debug.Log("EnemyManager: Despawning enemy " + e.name + " por durabilidad.");

                if (e.CupInstance != null)
                    Destroy(e.CupInstance);

                if (e.TokenInstance != null)
                    Destroy(e.TokenInstance);

                Destroy(e.gameObject);

                RemoveEnemy(e);


                RemoveEnemy(e);
            }
        }
        foreach (var enemySO in enemyDefinitions)
        {
            if (enemySO == null)
                continue;

            // Buscar si ya existe una instancia activa de este enemigo
            EnemyBase existing = enemies.Find(e => e.data == enemySO);
            bool alreadySpawned = existing != null && existing.isActive;
            if (enemySO.spawnOnlyOnce && enemySO.hasSpawnedOnce)
                continue;

            // ====================================================
            // 1. DEMONIO — Spawn por vueltas o por tirada 666
            // ====================================================

            if (enemySO.enemyPrefab.TryGetComponent<DemonBoss>(out var demonPrefab))
            {
                int lastRoll = StatManager.Instance.PreviousRoll;

                bool spawnByRoll = (lastRoll == 18); // 6+6+6
                bool spawnByLaps =
                    currentLap >= enemySO.lapsToActivate &&
                    (!enemySO.requiresPlayerLap || playerMovement.Round > 1);

                // NEW — Buscar instancia previa del demonio
                EnemyBase existingDemon = enemies.Find(e => e.data == enemySO);

                // NEW — Respetar Spawn Only Once
                if (enemySO.spawnOnlyOnce && enemySO.hasSpawnedOnce)
                    continue;

                bool demonIsActive = existingDemon != null && existingDemon.isActive;

                if (!demonIsActive && (spawnByRoll || spawnByLaps))
                {
                    Debug.Log("EnemyManager: Demonio aparece (vueltas o 666).");
                    SpawnEnemy(enemySO);
                }

                continue;
            }



            // ====================================================
            // 2. ENEMIGOS NORMALES — Spawn por vueltas
            // ====================================================
            bool canSpawnByLaps =
                currentLap >= enemySO.lapsToActivate &&
                (!enemySO.requiresPlayerLap || playerMovement.Round > 1);

            if (!canSpawnByLaps)
                continue;

            if (enemySO.spawnOnlyOnce && enemySO.hasSpawnedOnce)
                continue;

            if (!alreadySpawned)
            {
                Debug.Log($"EnemyManager: {enemySO.name} aparece por vueltas ({currentLap}).");
                SpawnEnemy(enemySO);
            }
        }
    }

    // ============================================================
    // SPAWN REAL DEL ENEMIGO
    // ============================================================
    private void SpawnEnemy(EnemySO enemySO)
    {
        GameObject enemyObj = Instantiate(enemySO.enemyPrefab);

        if (!enemyObj.TryGetComponent<EnemyBase>(out EnemyBase enemy))
        {
            Debug.LogError("EnemyManager: Enemy prefab missing EnemyBase.");
            Destroy(enemyObj);
            return;
        }

        enemy.data = enemySO;
        enemy.isActive = true;

        enemies.Add(enemy);
        TurnManager.Instance.RegisterEnemy(enemy);

        enemy.SpawnEnemy();
    }

    // ============================================================
    // REGISTRO / DESREGISTRO
    // ============================================================
    public void ActivateEnemy(EnemyBase enemy)
    {
        if (enemy == null)
            return;

        if (!enemies.Contains(enemy))
            enemies.Add(enemy);

        if (!TurnManager.Instance.IsEnemyRegistered(enemy))
            TurnManager.Instance.RegisterEnemy(enemy);
    }

    public void RemoveEnemy(EnemyBase enemy)
    {
        if (enemy == null)
            return;

        if (enemies.Contains(enemy))
            enemies.Remove(enemy);

        if (TurnManager.Instance.IsEnemyRegistered(enemy))
            TurnManager.Instance.UnregisterEnemy(enemy);
    }
}
