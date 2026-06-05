using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/*
 * Movement
 * --------
 * Handles all board movement logic for both players and enemies.
 * Responsibilities:
 * - Moving step-by-step across board spots
 * - Handling checkpoints, bridges, and lap progression
 * - Triggering spot effects
 * - Managing visibility effects
 * - Integrating with shop entry logic
 * - Reporting real movement to UI
 */
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
    private bool ignoreInitialCheckpoint = true;

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
        /*
         * Load all board spots dynamically.
         * Spots define checkpoints, effects, and board structure.
         */
        spots = UnityEngine.Object.FindObjectsByType<Spot>(FindObjectsInactive.Exclude);

        // Sort spots by index to ensure correct board order
        Array.Sort(spots, (a, b) => a.index.CompareTo(b.index));

        /*
         * If no positions were assigned manually,
         * use the transforms of the detected spots.
         */
        if (positions == null || positions.Length == 0)
        {
            positions = new Transform[spots.Length];
            for (int i = 0; i < spots.Length; i++)
                positions[i] = spots[i].transform;
        }

        shopExitManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ShopExitManager>();
        cachedRenderer = GetComponentInChildren<Renderer>();

        /*
         * Initial placement on the board.
         */
        if (actualPos >= 1 && actualPos <= positions.Length)
            transform.position = positions[actualPos - 1].position;

        startPos = actualPos;
        lastPos = actualPos;
    }

    /*
     * StartMoving
     * -----------
     * Begins a normal movement sequence based on the dice roll.
     */
    public void StartMoving()
    {
        startPos = actualPos;
        nextCheckpoint = GetNextCheckpoint();
        effectAlreadyTriggered = false;
        turnShouldEnd = true;
        movementIsPlayerControlled = true;
        lastTotalMovement = 0;
        lastSpotEffectText = "";
        ignoreBridgeThisMove = false;

        // Player movement may be blocked by effects
        if (isPlayer && StatManager.Instance.PreventMovementThisTurn)
        {
            OnMovementFinished?.Invoke();
            SendRealMovementToUI("Movement blocked");
            return;
        }

        // Notify effect system
        if (isPlayer)
            CharacterEffectManager.Instance.NotifyMovementStart(this);

        ignoreInitialCheckpoint = false;
        StartCoroutine(MoveWithVisibilityCheck());
    }

    /*
     * StartMovingFixed
     * ----------------
     * Moves a fixed number of steps (used after leaving the shop).
     */
    public void StartMovingFixed(int steps)
    {
        nextCheckpoint = int.MaxValue;
        effectAlreadyTriggered = false;
        turnShouldEnd = true;
        ignoreBridgeThisMove = false;
        movementIsPlayerControlled = false;
        if (isPlayer)
            CharacterEffectManager.Instance.NotifyMovementStart(this);

        StartCoroutine(MoveWithVisibilityCheck(steps));
    }

    /*
     * MoveWithVisibilityCheck
     * ------------------------
     * Applies visibility effects before movement (e.g., invisibility).
     */
    private IEnumerator MoveWithVisibilityCheck(int? fixedSteps = null)
    {
        yield return null;

        // Apply visibility effects
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

        /*
         * Apply round-based movement divisor.
         * Movement becomes slower as rounds increase.
         */
        int divisor = 1;
        if (Round >= 3)
            divisor = ((Round - 3) / 2) + 2;

        if (isPlayer)
            steps /= divisor;

        yield return StartCoroutine(Move(steps));
    }

    /*
     * Move
     * ----
     * Core movement routine: moves step-by-step, handles bridges,
     * checkpoints, lap progression, and spot effects.
     */
    private IEnumerator Move(int steps)
    {
        Debug.Log($"[Movement] Move START with steps = {steps}");
        if (actualPos <= 0)
        {
            OnMovementFinished?.Invoke();
            SendRealMovementToUI(lastSpotEffectText);

            if (isPlayer)
                CharacterEffectManager.Instance.NotifyMovementEnd(this);

            yield break;
        }

        if (!isExtraMovement)
            lastTotalMovement += steps;

        /*
         * Checkpoint logic:
         * If the player would pass a checkpoint, movement is cut short
         * and the shop is entered.
         */
        if (isPlayer && nextCheckpoint > 0 && movementIsPlayerControlled)
        {
            int hypotheticalSpot = actualPos + steps;

            int total = spots.Length;
            while (hypotheticalSpot > total)
                hypotheticalSpot -= total;
            while (hypotheticalSpot < 1)
                hypotheticalSpot += total;

            if (hypotheticalSpot > nextCheckpoint)
                steps = nextCheckpoint - actualPos;
        }

        if (!isPlayer)
            yield return new WaitForSeconds(1f);

        int direction = steps >= 0 ? 1 : -1;
        int totalSteps = Mathf.Abs(steps);

        /*
         * Step-by-step movement loop.
         */
        for (int i = 0; i < totalSteps; i++)

        {
            int nextSpot = GetNextSpotIndex(actualPos, direction);
            int previousPos = actualPos;
            actualPos = nextSpot;

            /*
             * Lap progression and enemy spawn checks.
             */
            if (isPlayer && direction > 0)
            {
                LapProgress += 1f / positions.Length;

                if (EnemyManager.Instance != null)
                    EnemyManager.Instance.CheckSpawnConditions();
            }

            /*
             * Round progression when crossing the start position.
             */
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

            /*
             * Move toward the next board position.
             */
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

            /*
             * Bridge logic: instantly teleport to connected spot.
             */
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

            /*
             * Checkpoint entry: enter shop and pause movement.
             */
            if (direction > 0 && spots[actualPos - 1].checkpoint && isPlayer && movementIsPlayerControlled)
            {
                Debug.Log($"[Movement] CHECKPOINT DETECTED at spot {actualPos}");
                Debug.Log($"[Movement] Spot index = {spots[actualPos - 1].index}, checkpoint = {spots[actualPos - 1].checkpoint}");
                if (ignoreInitialCheckpoint)
                {
                    ignoreInitialCheckpoint = false;
                }
                else
                {
                    int remaining = totalSteps - (i + 1);
                    pendingSteps = remaining;
                    Debug.Log($"[Movement] pendingSteps SET on shop entry = {pendingSteps} (remaining={remaining})");
                    turnShouldEnd = false;

                    shopExitManager.EnterShop();
                    yield break;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        ignoreBridgeThisMove = false;

        /*
         * Trigger spot effect if applicable.
         */
        if (isPlayer && TurnManager.Instance.IsPlayerTurn() && !effectAlreadyTriggered)
        {
            yield return StartCoroutine(spots[actualPos - 1].TriggerSpotEffect(this));
            effectAlreadyTriggered = true;
        }

        /*
         * Restore visibility if it was hidden by an effect.
         */
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

        if (isPlayer)
            CharacterEffectManager.Instance.NotifyMovementEnd(this);
    }

    private int GetNextSpotIndex(int current, int direction)
    {
        int next = current + direction;

        if (next > 68) next = 1;
        if (next < 1) next = 68;

        return next;
    }


    /*
     * ExtraMovementRoutine
     * --------------------
     * Handles additional movement granted by effects.
     */
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

        if (isPlayer)
            CharacterEffectManager.Instance.NotifyMovementEnd(this);
    }

    /*
     * SendRealMovementToUI
     * ---------------------
     * Reports the actual movement (including wrap-around logic)
     * and all applied effects to the UI.
     */
    public void SendRealMovementToUI(string effectText = "")
    {
        int total = positions.Length;
        int realMovement = actualPos - startPos;

        if (realMovement > total / 2)
            realMovement -= total;
        if (realMovement < -total / 2)
            realMovement += total;

        // Include dice effects
        var diceEffects = DiceRollManager.Instance.GetLastAppliedEffects();
        string allEffects = effectText;

        if (diceEffects.Count > 0)
        {
            if (!string.IsNullOrEmpty(allEffects))
                allEffects += " | ";

            allEffects += string.Join(", ", diceEffects);
        }

        // Modern API: FindAnyObjectByType
        var ui = UnityEngine.Object.FindAnyObjectByType<ActiveDiceUI>();
        if (ui != null)
            ui.SetLastTurnSummary(realMovement, allEffects, false);
    }

    /*
     * GetNextCheckpoint
     * -----------------
     * Finds the next checkpoint ahead of the current position.
     */
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

    /*
     * TeleportToPosition
     * ------------------
     * Instantly moves the piece to a board position.
     */
    public void TeleportToPosition(int index)
    {
        if (positions == null || positions.Length == 0)
            return;

        if (index < 1 || index > positions.Length)
            return;

        actualPos = index;
        transform.position = positions[index - 1].position;
    }

    /*
     * ResetAfterShop
     * --------------
     * Called after leaving the shop to restore movement state.
     */
    public void ResetAfterShop()
    {
        ignoreInitialCheckpoint = true;
        isExtraMovement = false;
        lastPos = actualPos;
        startPos = actualPos;

    }

#if UNITY_EDITOR
    /*
     * Editor-only lap progression test helpers.
     */
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
