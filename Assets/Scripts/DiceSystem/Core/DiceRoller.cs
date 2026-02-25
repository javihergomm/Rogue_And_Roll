using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * DiceRoller
 * ----------
 * Handles the physical rolling of a dice:
 *  - Applies force and torque to start a roll
 *  - Waits until the dice stabilizes
 *  - Detects the upward face
 *  - Applies mid-air correction and snapping if needed
 *  - Reports the final result to DiceRollManager
 *
 * IMPORTANT:
 * This version does NOT react to mouse clicks anymore.
 * Dice rolls must be triggered externally (for example, from a UI button).
 */
public class DiceRoller : MonoBehaviour
{
    /*
     * FaceEntry
     * ---------
     * Represents a single face of a dice.
     * Stores:
     *  - The local-space normal vector of the face
     *  - The numeric value associated with that face
     */
    [System.Serializable]
    public struct FaceEntry
    {
        public Vector3 normal;
        public int value;
    }

    private Rigidbody rb;
    private Camera cam;

    private DiceSO diceData;
    private ItemSlot linkedSlot;

    [Header("Dice Settings")]
    [SerializeField] private DiceType diceType = DiceType.D6;

    [Header("Face Map (Prefab Only)")]
    [SerializeField] private List<FaceEntry> serializedFaceMap = new List<FaceEntry>();

    public Dictionary<Vector3, int> FaceMap { get; private set; }

    private bool isRolling = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        InitFaceMap();
    }

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            return;

        ResetPhysics();
    }

    /*
     * Assigns the dice data and inventory slot to this roller.
     * Called by DiceRollManager when spawning dice in the world.
     */
    public void AssignDice(DiceSO data, ItemSlot slot)
    {
        diceData = data;
        linkedSlot = slot;
        diceType = data.DiceType;
        InitFaceMap();
    }

    /*
     * Converts the serialized face list into a dictionary
     * mapping local-space normals to face values.
     */
    public void InitFaceMap()
    {
        FaceMap = new Dictionary<Vector3, int>();

        foreach (var entry in serializedFaceMap)
            FaceMap[entry.normal] = entry.value;

        if (FaceMap.Count == 0)
            Debug.LogWarning(name + ": Prefab has no FaceMap assigned.");
    }

    /*
     * PUBLIC: Called externally to begin the roll routine.
     */
    public void StartRollRoutine()
    {
        StartCoroutine(HandleRoll());
    }

    /*
     * Applies force and torque to start the roll.
     * Does NOT start the roll routine automatically.
     */
    public void RollDice()
    {
        if (rb == null)
            return;

        if (DiceRollManager.Instance.HasSlotRolledThisTurn(linkedSlot))
            return;

        if (isRolling)
            return;

        isRolling = true;

        ResetPhysics();

        rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 50f, ForceMode.Impulse);
    }

    /*
     * Main roll routine:
     *  - Waits for the dice to spin
     *  - Waits for it to slow down
     *  - Detects the physical face
     *  - Applies mid-air correction if needed
     *  - Snaps to allowed face if needed
     *  - Sends final result to DiceRollManager
     */
    private IEnumerator HandleRoll()
    {
        if (rb == null)
            yield break;

        yield return new WaitForFixedUpdate();

        while (rb.angularVelocity.magnitude < 2f)
            yield return null;

        while (rb.angularVelocity.magnitude > 0.5f)
            yield return null;

        int physicalRoll = GetFaceUp();

        DiceContext ctx = new DiceContext
        {
            turnNumber = StatManager.Instance.CurrentTurn,
            previousRoll = StatManager.Instance.PreviousRoll,
            slot = linkedSlot
        };

        int? targetFace = DiceRollManager.Instance.GetTargetFaceForRoll(linkedSlot, physicalRoll, ctx);

        if (targetFace.HasValue && targetFace.Value != physicalRoll)
            StartCoroutine(ApplyMidAirCorrection(targetFace.Value));

        while (!rb.IsSleeping())
            yield return null;

        int finalFace = GetFaceUp();

        if (!DiceRollManager.Instance.IsFaceAllowed(linkedSlot, finalFace))
        {
            int? snapTarget = DiceRollManager.Instance.GetTargetFaceForRoll(linkedSlot, finalFace, ctx);
            if (snapTarget.HasValue)
                yield return StartCoroutine(SnapToFace(snapTarget.Value));

            finalFace = GetFaceUp();
        }

        isRolling = false;

        DiceRollManager.Instance.OnDiceResult(linkedSlot, finalFace);
        InventoryManager.Instance.RefreshActiveDiceUI();
    }

    /*
     * Applies torque to rotate the dice toward a target face while airborne.
     */
    private IEnumerator ApplyMidAirCorrection(int targetValue)
    {
        if (rb == null)
            yield break;

        Vector3 targetLocalDir = GetLocalDirectionForFace(targetValue);
        float timer = 0f;
        const float maxTime = 1.2f;

        while (timer < maxTime && !rb.IsSleeping())
        {
            Vector3 targetWorldDir = transform.TransformDirection(targetLocalDir);
            Vector3 currentUp = transform.up;

            float alignment = Vector3.Dot(currentUp, targetWorldDir);
            float strength = Mathf.Clamp01(1f - alignment);

            Vector3 torqueDir = Vector3.Cross(currentUp, targetWorldDir);
            rb.AddTorque(torqueDir * (strength * 6f), ForceMode.Acceleration);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    /*
     * Smoothly rotates the dice to align a target face upward.
     */
    private IEnumerator SnapToFace(int targetValue)
    {
        if (rb == null)
            yield break;

        Vector3 targetLocalDir = GetLocalDirectionForFace(targetValue);
        Vector3 targetWorldUp = transform.TransformDirection(targetLocalDir);

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.FromToRotation(targetWorldUp, Vector3.up) * transform.rotation;

        float t = 0f;
        const float duration = 0.18f;

        while (t < duration)
        {
            float smooth = t / duration;
            smooth = smooth * smooth * (3f - 2f * smooth);

            transform.rotation = Quaternion.Slerp(startRot, endRot, smooth);

            t += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRot;
    }

    /*
     * Determines which face normal is closest to world-up.
     */
    private int GetFaceUp()
    {
        float bestDot = -1f;
        int bestValue = 0;

        foreach (var kvp in FaceMap)
        {
            Vector3 worldAxis = transform.TransformDirection(kvp.Key);
            float dot = Vector3.Dot(worldAxis, Vector3.up);

            if (dot > bestDot)
            {
                bestDot = dot;
                bestValue = kvp.Value;
            }
        }

        return bestValue;
    }

    /*
     * Returns the local-space normal for a given face value.
     */
    private Vector3 GetLocalDirectionForFace(int value)
    {
        foreach (var kvp in FaceMap)
            if (kvp.Value == value)
                return kvp.Key;

        return Vector3.up;
    }

    /*
     * Stops all motion and resets the rigidbody.
     */
    private void ResetPhysics()
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
    }
}
