using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/*
 * Handles all board movement logic, including checkpoints, shop entry,
 * pending steps, lap counting only on player-controlled forward movement,
 * and ignoring lap count on enemy-forced movement.
 */
public class Movement : MonoBehaviour
{
    private Spot[] spots;

    [SerializeField] private Transform[] positions;
    public Transform[] Positions => positions;

    private ShopExitManager shopExitManager;

    public bool ignoreBridgeThisMove = false;

    public void SetPositions(Transform[] newPositions)
    {
        positions = newPositions;
    }

    [SerializeField] private float speed = 5f;

    [SerializeField] private int actualPos = 0;
    public int ActualPos
    {
        get => actualPos;
        set => actualPos = value;
    }

    public bool isPlayer;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;

    private int nextCheckpoint;
    public Action OnMovementFinished;

    private Renderer cachedRenderer;
    private bool wasHiddenByEffect = false;
    public int Round { get; private set; } = 1;
    public float LapProgress { get; private set; } = 0f;

    public int startPos;
    public int lastPos;

    private bool effectAlreadyTriggered = false;

    public int probabilityExtraSteps = 50;
    public int probabilityBlockEnemy = 50;

    public int probabilityNegativeSteps = 50;
    public int probabilityBlockPlayer = 50;

    public int pendingSteps = 0;
    public bool turnShouldEnd = true;
    public bool movementIsPlayerControlled = true;

    // NUEVO: total de movimiento del turno
    public int lastTotalMovement = 0;

    private void Start()
    {
        spots = FindObjectsByType<Spot>(FindObjectsSortMode.None);
        Array.Sort(spots, (a, b) => a.index.CompareTo(b.index));

        if (positions == null || positions.Length == 0)
        {
            positions = new Transform[spots.Length];
            for (int i = 0; i < spots.Length; i++)
                positions[i] = spots[i].transform;
        }

        shopExitManager = FindFirstObjectByType<ShopExitManager>();
        cachedRenderer = GetComponentInChildren<Renderer>();

        if (actualPos >= 1 && actualPos <= positions.Length)
            transform.position = positions[actualPos - 1].position;

        startPos = actualPos;
        lastPos = actualPos;
    }

    public void StartMoving()
    {
        nextCheckpoint = GetNextCheckpoint();
        effectAlreadyTriggered = false;
        turnShouldEnd = true;
        movementIsPlayerControlled = true;

        // Reiniciar el total del movimiento del turno
        lastTotalMovement = 0;

        if (isPlayer && StatManager.Instance.PreventMovementThisTurn)
        {
            OnMovementFinished?.Invoke();
            return;
        }

        StartCoroutine(MoveWithVisibilityCheck());
    }

    public void StartMovingFixed(int steps)
    {
        nextCheckpoint = int.MaxValue;
        effectAlreadyTriggered = false;
        turnShouldEnd = true;

        // Reiniciar el total del movimiento del turno
        lastTotalMovement = 0;

        StartCoroutine(MoveWithVisibilityCheck(steps));
    }

    private IEnumerator MoveWithVisibilityCheck(int? fixedSteps = null)
    {
        yield return null;

        if (isPlayer && cachedRenderer != null)
        {
            if (StatManager.Instance.HidePieceThisTurn)
            {
                cachedRenderer.enabled = false;
                wasHiddenByEffect = true;
            }
            else
            {
                cachedRenderer.enabled = true;
                wasHiddenByEffect = false;
            }
        }

        int steps = fixedSteps ?? InventoryManager.Instance.GetFinalDiceNumber();

        int divisor = 1;
        if (Round >= 3)
            divisor = ((Round - 3) / 2) + 2;

        if (isPlayer)
            steps /= divisor;

        yield return StartCoroutine(Move(steps));
    }

    private IEnumerator Move(int steps)
    {
        if (actualPos <= 0)
        {
            OnMovementFinished?.Invoke();
            yield break;
        }

        // Guardar movimiento base
        lastTotalMovement += Mathf.Abs(steps);

        if (isPlayer && nextCheckpoint > 0)
        {
            int hypotheticalSpot = actualPos + steps;
            if (hypotheticalSpot > nextCheckpoint)
                steps = nextCheckpoint - actualPos;
        }

        if (!isPlayer)
            yield return new WaitForSeconds(1f);

        int direction = steps >= 0 ? 1 : -1;
        int totalSteps = Mathf.Abs(steps);

        for (int i = 0; i < totalSteps; i++)
        {
            int previousPos = actualPos;

            actualPos += direction;

            if (actualPos > positions.Length)
                actualPos = 1;
            if (actualPos < 1)
                actualPos = positions.Length;

            if (isPlayer && direction > 0)
            {
                LapProgress += 1f / positions.Length;

                if (EnemyManager.Instance != null)
                    EnemyManager.Instance.CheckSpawnConditions();
            }

            if (isPlayer && direction > 0 && movementIsPlayerControlled)
            {
                bool crossedSpawn = previousPos < startPos && actualPos >= startPos;

                if (crossedSpawn)
                {
                    Round++;

                    if (Round == 2)
                    {
                        Unlocks.Unlock("ID_DEL_OBJETO");
                        Debug.Log("Objeto desbloqueado por completar 1 vuelta");
                    }

                    if (EnemyManager.Instance != null)
                        EnemyManager.Instance.CheckSpawnConditions();
                }
            }

            Vector3 target = positions[actualPos - 1].position;
            PlayMovementSound();

            while (Vector3.Distance(transform.position, target) > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            transform.position = target;

            var connections = SpotConnectionManager.Instance.GetConnections(actualPos);

            if (!ignoreBridgeThisMove && connections.Count > 0)
            {
                int targetSpot = connections[0];
                int prev = actualPos;
                actualPos = targetSpot;

                Vector3 bridgeTarget = positions[targetSpot - 1].position;

                while (Vector3.Distance(transform.position, bridgeTarget) > 0.0001f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        bridgeTarget,
                        speed * Time.deltaTime
                    );
                    yield return null;
                }

                transform.position = bridgeTarget;

                if (isPlayer && direction > 0 && movementIsPlayerControlled)
                {
                    bool crossedSpawn = prev < startPos && actualPos >= startPos;

                    if (crossedSpawn)
                    {
                        Round++;

                        if (EnemyManager.Instance != null)
                            EnemyManager.Instance.CheckSpawnConditions();
                    }
                }
            }

            if (spots[actualPos - 1].checkpoint && isPlayer)
            {
                int remaining = totalSteps - (i + 1);
                pendingSteps = remaining;
                turnShouldEnd = false;

                shopExitManager.EnterShop();

                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        ignoreBridgeThisMove = false;

        var type = spots[actualPos - 1].GetSpotType();

        if (isPlayer && TurnManager.Instance.IsPlayerTurn() && !effectAlreadyTriggered)
        {
            if (type == Spot.SpotType.Good)
            {
                effectAlreadyTriggered = true;

                int roll = UnityEngine.Random.Range(0, 100);

                if (roll < probabilityExtraSteps)
                {
                    int extra = UnityEngine.Random.Range(3, 6);

                    // Acumular movimiento extra
                    lastTotalMovement += Mathf.Abs(extra);

                    yield return StartCoroutine(ExtraMovementRoutine(extra));
                }
                else
                {
                    ScriptableObject.CreateInstance<BlockEnemyMovementEffect>().Activate();
                }

                OnMovementFinished?.Invoke();
                yield break;
            }
            else if (type == Spot.SpotType.Bad)
            {
                effectAlreadyTriggered = true;

                int roll = UnityEngine.Random.Range(0, 100);

                if (roll < probabilityNegativeSteps)
                {
                    int extra = UnityEngine.Random.Range(-3, -6);

                    // Acumular movimiento extra
                    lastTotalMovement += Mathf.Abs(extra);

                    yield return StartCoroutine(ExtraMovementRoutine(extra));
                }
                else
                {
                    ScriptableObject.CreateInstance<BlockPlayerMovementEffect>().Activate();
                }

                OnMovementFinished?.Invoke();
                yield break;
            }
        }

        if (isPlayer && cachedRenderer != null && wasHiddenByEffect)
        {
            cachedRenderer.enabled = true;
            wasHiddenByEffect = false;
        }

        if (isPlayer)
            DiceRollManager.Instance.ResetDiceTurnState();

        lastPos = actualPos;
        OnMovementFinished?.Invoke();
    }

    private IEnumerator ExtraMovementRoutine(int extraSteps)
    {
        if (extraSteps != 0)
            yield return StartCoroutine(Move(extraSteps));

        lastPos = actualPos;
        OnMovementFinished?.Invoke();
    }

    public int GetNextCheckpoint()
    {
        int total = spots.Length;

        for (int i = 1; i <= total; i++)
        {
            int nextPos = ((actualPos - 1 + i) % total);

            if (spots[nextPos].checkpoint)
                return spots[nextPos].index;
        }

        return -1;
    }

    private void PlayMovementSound()
    {
        if (audioSource != null && moveSound != null)
            audioSource.PlayOneShot(moveSound);
    }

    public void TeleportToPosition(int index)
    {
        if (positions == null || positions.Length == 0)
            return;

        if (index < 1 || index > positions.Length)
            return;

        actualPos = index;
        transform.position = positions[index - 1].position;
    }

#if UNITY_EDITOR

    [ContextMenu("TEST: +0.05 Lap")]
    private void TestAddLap005() => AddLapProgressTest(0.05f);

    [ContextMenu("TEST: +0.10 Lap")]
    private void TestAddLap010() => AddLapProgressTest(0.10f);

    [ContextMenu("TEST: +0.25 Lap")]
    private void TestAddLap025() => AddLapProgressTest(0.25f);

    [ContextMenu("TEST: +0.50 Lap")]
    private void TestAddLap050() => AddLapProgressTest(0.50f);

    private void AddLapProgressTest(float amount)
    {
        if (!isPlayer)
        {
            Debug.LogWarning("Solo funciona en el jugador.");
            return;
        }

        LapProgress += amount;

        Debug.Log("TEST: LapProgress = " + LapProgress);

        if (LapProgress >= 1f)
        {
            LapProgress -= 1f;
            Round++;

            Debug.Log("TEST: Vuelta completada! Round = " + Round);

            if (Round == 2)
            {
                Unlocks.Unlock("ID_DEL_OBJETO");
                Debug.Log("TEST: Objeto desbloqueado por completar 1 vuelta");
            }

            if (EnemyManager.Instance != null)
                EnemyManager.Instance.CheckSpawnConditions();

            if (StatManager.Instance != null)
                StatManager.Instance.TriggerStatsChanged();
        }
    }

#endif

}
