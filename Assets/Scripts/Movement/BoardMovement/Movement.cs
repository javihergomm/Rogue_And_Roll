using System;
using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Spot[] spots;

    [SerializeField] private Transform[] positions;
    public Transform[] Positions => positions;

    private ShopExitManager shopExitManager;

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
    private int round = 1;

    public int startPos;
    public int lastPos;

    private bool effectAlreadyTriggered = false;

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

        if (isPlayer && StatManager.Instance.PreventMovementThisTurn)
            return;

        StartCoroutine(MoveWithVisibilityCheck());
    }

    public void StartMovingFixed(int steps)
    {
        nextCheckpoint = int.MaxValue;
        effectAlreadyTriggered = false;
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
        if (round >= 3)
            divisor = ((round - 3) / 2) + 2;

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

        // FIX: checkpoints solo afectan al jugador
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
            actualPos += direction;

            if (actualPos > positions.Length)
                actualPos = 1;
            if (actualPos < 1)
                actualPos = positions.Length;

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
            if (connections.Count > 0)
            {
                int targetSpot = connections[0];
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
            }

            yield return new WaitForSeconds(0.1f);
        }

        var type = spots[actualPos - 1].getType();

        if (isPlayer && !effectAlreadyTriggered)
        {
            if (spots[actualPos - 1].checkpoint)
            {
                shopExitManager.EnterShop();
                round++;
                yield break;
            }
            else if (type == Spot.SpotType.Good)
            {
                effectAlreadyTriggered = true;
                int extra = GoodSpotEffect();
                yield return StartCoroutine(ExtraMovementRoutine(extra));
                yield break;
            }
            else if (type == Spot.SpotType.Bad)
            {
                effectAlreadyTriggered = true;
                int extra = BadSpotEffect();
                yield return StartCoroutine(ExtraMovementRoutine(extra));
                yield break;
            }
        }

        SpotConnectionManager.Instance.OnRoundStepCompleted();

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

    private int GoodSpotEffect()
    {
        int effectType = SpotController.GoodSpot();

        if (effectType == 1)
            return UnityEngine.Random.Range(3, 6);

        return 0;
    }

    private int BadSpotEffect()
    {
        int effectType = SpotController.BadSpot();

        if (effectType == 1)
            return UnityEngine.Random.Range(-3, -6);

        return 0;
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
        {
            Debug.LogError("Movement: Positions array is not set.");
            return;
        }

        if (index < 1 || index > positions.Length)
        {
            Debug.LogError("Movement: TeleportToPosition index " + index + " is out of range.");
            return;
        }

        actualPos = index;
        transform.position = positions[index - 1].position;
    }
}
