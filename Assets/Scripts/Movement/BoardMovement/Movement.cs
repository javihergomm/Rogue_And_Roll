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
    public bool pausedByShop = false;

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
        // Prevent movement if paused by the shop
        if (pausedByShop)
        {
            while (pausedByShop)
                yield return null;
        }

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
        Debug.Log("[MOVE DEBUG] =====================================");
        Debug.Log("[MOVE DEBUG] Starting Move()");
        Debug.Log("[MOVE DEBUG] actualPos BEFORE movement = " + actualPos);
        Debug.Log("[MOVE DEBUG] steps requested = " + steps);
        Debug.Log("[MOVE DEBUG] nextCheckpoint = " + nextCheckpoint);

        // Stop movement immediately if paused by the shop
        if (pausedByShop)
            yield break;

        if (actualPos <= 0)
        {
            OnMovementFinished?.Invoke();
            SendRealMovementToUI(lastSpotEffectText);

            if (isPlayer)
                CharacterEffectManager.Instance.NotifyMovementEnd(this);

            // End the player's turn if movement finished normally
            if (isPlayer && turnShouldEnd)
                TurnManager.Instance.ForcePlayerTurnEnd();

            yield break;
        }

        if (!isExtraMovement)
            lastTotalMovement += steps;

        /*
         * Checkpoint logic:
         * If the player would pass a checkpoint, movement is cut short
         * and the shop is entered.
         *
         * DEBUG: print distance and cut results
         */
        if (isPlayer && nextCheckpoint > 0 && movementIsPlayerControlled)
        {
            int total = spots.Length;

            // Calculate distance to checkpoint with wrap-around
            int distanceToCheckpoint = nextCheckpoint - actualPos;
            if (distanceToCheckpoint < 0)
                distanceToCheckpoint += total;

            Debug.Log("[MOVE DEBUG] distanceToCheckpoint = " + distanceToCheckpoint);

            // If the dice roll exceeds the distance, cut movement to checkpoint
            if (steps > distanceToCheckpoint)
            {
                Debug.Log("[MOVE DEBUG] Cutting steps because checkpoint is ahead");
                Debug.Log("[MOVE DEBUG] steps BEFORE cut = " + steps);
                steps = distanceToCheckpoint;
                Debug.Log("[MOVE DEBUG] steps AFTER cut = " + steps);
            }
            else
            {
                Debug.Log("[MOVE DEBUG] No cut. steps = " + steps + " distanceToCheckpoint = " + distanceToCheckpoint);
            }
        }

        if (!isPlayer)
            yield return new WaitForSeconds(1f);

        int direction = steps >= 0 ? 1 : -1;
        int totalSteps = Mathf.Abs(steps);

        Debug.Log("[MOVE DEBUG] totalSteps AFTER cut = " + totalSteps);

        /*
         * Step-by-step movement loop.
         */
        for (int i = 0; i < totalSteps; i++)
        {
            Debug.Log("[MOVE DEBUG] Step " + (i + 1) + " / " + totalSteps);

            int nextSpot = GetNextSpotIndex(actualPos, direction);
            Debug.Log("[MOVE DEBUG] Moving from " + actualPos + " to " + nextSpot);

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
                    Debug.Log("[MOVE DEBUG] Crossed spawn. New round = " + Round);

                    // Round 2: unlock one random locked item
                    if (Round == 2)
                    {
                        BaseItemSO[] allItems = Resources.LoadAll<BaseItemSO>("Items");

                        var locked = new System.Collections.Generic.List<BaseItemSO>();

                        foreach (var item in allItems)
                        {
                            if (item != null && !Unlocks.IsUnlocked(item.itemID))
                                locked.Add(item);
                        }

                        if (locked.Count > 0)
                        {
                            BaseItemSO reward = locked[UnityEngine.Random.Range(0, locked.Count)];
                            Unlocks.Unlock(reward.itemID);

                            Debug.Log("[Round 2 Unlock] Unlocked: " + reward.itemID);
                        }
                    }

                    // Round 4: unlock metal characters
                    if (Round == 4)
                    {
                        Unlocks.Unlock("character_verde_metalico");
                        Unlocks.Unlock("character_rojo_metalico");
                        Unlocks.Unlock("character_amarillo_metalico");
                        Unlocks.Unlock("character_azul_metalico");

                        Debug.Log("[Round 4 Unlock] Metal characters unlocked.");
                    }

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
                Debug.Log("[MOVE DEBUG] Bridge teleport to " + targetSpot);

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
                Debug.Log("[MOVE DEBUG] CHECKPOINT DETECTED at spot " + actualPos);
                Debug.Log("[MOVE DEBUG] Spot index = " + spots[actualPos - 1].index + ", checkpoint = " + spots[actualPos - 1].checkpoint);

                if (ignoreInitialCheckpoint)
                {
                    ignoreInitialCheckpoint = false;
                }
                else
                {
                    int remaining = totalSteps - (i + 1);

                    Debug.Log("[MOVE DEBUG] remaining steps = " + remaining);
                    Debug.Log("[MOVE DEBUG] pendingSteps BEFORE assignment = " + pendingSteps);

                    pendingSteps = remaining;

                    Debug.Log("[MOVE DEBUG] pendingSteps AFTER assignment = " + pendingSteps);

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

        Debug.Log("[MOVE DEBUG] Movement finished at spot " + actualPos);
        Debug.Log("[MOVE DEBUG] turnShouldEnd = " + turnShouldEnd);
        Debug.Log("[MOVE DEBUG] pendingSteps (final) = " + pendingSteps);

        OnMovementFinished?.Invoke();
        SendRealMovementToUI(lastSpotEffectText);

        if (isPlayer)
            CharacterEffectManager.Instance.NotifyMovementEnd(this);

        // End the player's turn if movement finished normally
        if (isPlayer && turnShouldEnd)
            TurnManager.Instance.ForcePlayerTurnEnd();
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
        int bestDistance = int.MaxValue;
        int bestCheckpoint = -1;

        Debug.Log("[MOVE DEBUG] --- GetNextCheckpoint() ---");
        Debug.Log("[MOVE DEBUG] actualPos = " + actualPos);

        for (int i = 0; i < total; i++)
        {
            if (!spots[i].checkpoint)
                continue;

            int cpIndex = spots[i].index;

            int distance = cpIndex - actualPos;
            if (distance < 0)
                distance += total;

            Debug.Log("[MOVE DEBUG] checkpoint candidate = " + cpIndex + " distance = " + distance);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCheckpoint = cpIndex;
            }
        }

        Debug.Log("[MOVE DEBUG] NEXT CHECKPOINT = " + bestCheckpoint + " (distance = " + bestDistance + ")");
        return bestCheckpoint;
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
