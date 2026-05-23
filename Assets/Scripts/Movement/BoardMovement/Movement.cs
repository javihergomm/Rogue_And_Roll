using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Spot[] spots;

    [SerializeField] private Transform[] positions;
    public Transform[] Positions => positions;

    private ShopExitManager shopExitManager;

    public bool ignoreBridgeThisMove = false;

    public string lastSpotEffectText = "";

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

    private bool isExtraMovement = false;

    public int Round { get; private set; } = 1;
    public float LapProgress { get; private set; } = 0f;

    public int startPos;
    public int lastPos;

    public bool effectAlreadyTriggered = false;

    public int pendingSteps = 0;
    public bool turnShouldEnd = true;
    public bool movementIsPlayerControlled = true;

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
        startPos = actualPos;
        nextCheckpoint = GetNextCheckpoint();
        effectAlreadyTriggered = false;
        turnShouldEnd = true;
        movementIsPlayerControlled = true;
        lastTotalMovement = 0;
        lastSpotEffectText = "";

        if (isPlayer && StatManager.Instance.PreventMovementThisTurn)
        {
            OnMovementFinished?.Invoke();
            SendRealMovementToUI("Movimiento bloqueado");
            return;
        }

        // ============================================================
        // Notificar inicio de movimiento
        // ============================================================
        if (isPlayer)
            CharacterEffectManager.Instance.NotifyMovementStart(this);

        StartCoroutine(MoveWithVisibilityCheck());
    }

    public void StartMovingFixed(int steps)
    {
        nextCheckpoint = int.MaxValue;
        effectAlreadyTriggered = false;
        turnShouldEnd = true;

        // ============================================================
        // Notificar inicio de movimiento fijo
        // ============================================================
        if (isPlayer)
            CharacterEffectManager.Instance.NotifyMovementStart(this);

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
            SendRealMovementToUI(lastSpotEffectText);

            // ============================================================
            // Notificar fin de movimiento
            // ============================================================
            if (isPlayer)
                CharacterEffectManager.Instance.NotifyMovementEnd(this);

            yield break;
        }

        if (!isExtraMovement)
            lastTotalMovement += steps;

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
                        Unlocks.Unlock("ID_DEL_OBJETO");

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

            if (direction > 0 && spots[actualPos - 1].checkpoint && isPlayer && movementIsPlayerControlled)
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

        var type = spots[actualPos - 1].type;

        if (isPlayer && TurnManager.Instance.IsPlayerTurn() && !effectAlreadyTriggered)
        {
            yield return StartCoroutine(spots[actualPos - 1].TriggerSpotEffect(this));
            effectAlreadyTriggered = true;
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
        SendRealMovementToUI(lastSpotEffectText);

        // ============================================================
        // Notificar fin de movimiento
        // ============================================================
        if (isPlayer)
            CharacterEffectManager.Instance.NotifyMovementEnd(this);
    }

    public IEnumerator ExtraMovementRoutine(int extraSteps)
    {
        if (extraSteps != 0)
        {
            isExtraMovement = true;
            yield return StartCoroutine(Move(extraSteps));
            isExtraMovement = false;
        }

        lastPos = actualPos;

        OnMovementFinished?.Invoke();
        SendRealMovementToUI(lastSpotEffectText);

        // ============================================================
        // Notificar fin de movimiento extra
        // ============================================================
        if (isPlayer)
            CharacterEffectManager.Instance.NotifyMovementEnd(this);
    }

    public void SendRealMovementToUI(string effectText = "")
    {
        int total = positions.Length;
        int realMovement = actualPos - startPos;

        if (realMovement > total / 2)
            realMovement -= total;
        if (realMovement < -total / 2)
            realMovement += total;

        // ================================
        // Incluir efectos de dados
        // ================================
        var diceEffects = DiceRollManager.Instance.GetLastAppliedEffects();
        string allEffects = effectText;

        if (diceEffects.Count > 0)
        {
            if (!string.IsNullOrEmpty(allEffects))
                allEffects += " | ";

            allEffects += string.Join(", ", diceEffects);
        }

        var ui = FindFirstObjectByType<ActiveDiceUI>();
        if (ui != null)
            ui.SetLastTurnSummary(realMovement, allEffects, false);
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

    public void ResetAfterShop()
    {
        startPos = actualPos;
        lastTotalMovement = 0;
        effectAlreadyTriggered = false;
        movementIsPlayerControlled = false;
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
            return;

        LapProgress += amount;

        if (LapProgress >= 1f)
        {
            LapProgress -= 1f;
            Round++;

            if (Round == 2)
                Unlocks.Unlock("ID_DEL_OBJETO");

            if (EnemyManager.Instance != null)
                EnemyManager.Instance.CheckSpawnConditions();

            if (StatManager.Instance != null)
                StatManager.Instance.TriggerStatsChanged();
        }
    }

#endif
}
