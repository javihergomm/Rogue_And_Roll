using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

/*
 * DiceRoller
 * ----------
 * Handles the physical behavior of a dice.
 * Applies force when clicked.
 * Reads the upward face after the dice settles.
 * Applies mid-air correction and final snap correction if needed.
 */
public class DiceRoller : MonoBehaviour
{
    private Rigidbody rb;
    private Camera cam;

    private DiceSO diceData;
    private ItemSlot linkedSlot;

    [Header("Dice Settings")]
    [SerializeField] private DiceType diceType = DiceType.D6;

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
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
    }

    /*
     * Stores the dice data and the slot that spawned it.
     */
    public void AssignDice(DiceSO data, ItemSlot slot)
    {
        diceData = data;
        linkedSlot = slot;
        diceType = data.diceType;
        InitFaceMap();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    RollDice();
                    StartCoroutine(HandleRoll());
                }
            }
        }
    }

    /*
     * Applies an upward impulse and random torque.
     */
    public void RollDice()
    {
        if (isRolling)
            return;

        isRolling = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 50f, ForceMode.Impulse);
    }

    /*
     * Roll sequence:
     * - Waits until the dice starts rotating
     * - Waits until it slows down again
     * - Reads the physical face
     * - Asks DiceRollManager for a target face
     * - Applies mid-air correction if needed
     * - Waits until the dice stops
     * - Applies final snap correction if needed
     * - Reports the final face
     */
    private IEnumerator HandleRoll()
    {
        yield return new WaitForFixedUpdate();

        // Wait until dice is spinning
        while (rb.angularVelocity.magnitude < 2f)
            yield return null;

        // Wait until dice slows down again
        while (rb.angularVelocity.magnitude > 0.5f)
            yield return null;

        int physicalRoll = GetFaceUp(false);

        DiceContext ctx = new DiceContext
        {
            turnNumber = StatManager.Instance.CurrentTurn,
            previousRoll = StatManager.Instance.PreviousRoll,
            slot = linkedSlot
        };

        int? targetFace = DiceRollManager.Instance.GetTargetFaceForRoll(linkedSlot, physicalRoll, ctx);

        // Mid-air correction
        if (targetFace.HasValue && targetFace.Value != physicalRoll)
            StartCoroutine(ApplyMidAirCorrection(targetFace.Value));

        // Wait until dice fully stops
        while (!rb.IsSleeping())
            yield return null;

        int finalFace = GetFaceUp(false);

        // Final snap correction if needed
        if (!DiceRollManager.Instance.IsFaceAllowed(linkedSlot, finalFace))
        {
            int? snapTarget = DiceRollManager.Instance.GetTargetFaceForRoll(linkedSlot, finalFace, ctx);
            if (snapTarget.HasValue)
                yield return StartCoroutine(SnapToFace(snapTarget.Value));

            finalFace = GetFaceUp(false);
        }

        isRolling = false;

        DiceRollManager.Instance.OnDiceResult(linkedSlot, finalFace);
        InventoryManager.Instance.RefreshActiveDiceUI();

    }

    /*
     * Applies torque to rotate the dice toward a target face.
     * The torque is soft and does not override physics.
     */
    private IEnumerator ApplyMidAirCorrection(int targetValue)
    {
        Vector3 targetLocalDir = Vector3.zero;

        foreach (var kvp in FaceMap)
        {
            if (kvp.Value == targetValue)
            {
                targetLocalDir = kvp.Key;
                break;
            }
        }

        float timer = 0f;
        float maxTime = 1.2f;

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
     * Smoothly rotates the dice to the target face after it lands.
     */
    private IEnumerator SnapToFace(int targetValue)
    {
        // Find the local direction of the target face
        Vector3 targetLocalDir = Vector3.zero;

        foreach (var kvp in FaceMap)
        {
            if (kvp.Value == targetValue)
            {
                targetLocalDir = kvp.Key;
                break;
            }
        }

        // Compute the world direction that should point up
        Vector3 targetWorldUp = transform.TransformDirection(targetLocalDir);

        // Compute the final rotation that aligns that direction with Vector3.up
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.FromToRotation(targetWorldUp, Vector3.up) * transform.rotation;

        float t = 0f;
        float duration = 0.18f; // shorter, more natural

        while (t < duration)
        {
            // Smooth easing (ease in-out)
            float smooth = t / duration;
            smooth = smooth * smooth * (3f - 2f * smooth);

            transform.rotation = Quaternion.Slerp(startRot, endRot, smooth);

            t += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRot;
    }


    /*
     * Returns the face whose direction is closest to world up.
     */
    private int GetFaceUp(bool verbose)
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
     * Maps local directions to face values.
     */
    public void InitFaceMap()
    {
        FaceMap = new Dictionary<Vector3, int>();

        switch (diceType)
        {
            case DiceType.D4:
                FaceMap[Vector3.up] = 1;
                FaceMap[Vector3.forward] = 2;
                FaceMap[Vector3.right] = 3;
                FaceMap[Vector3.left] = 4;
                break;

            case DiceType.D6:
                FaceMap[Vector3.up] = 6;
                FaceMap[Vector3.down] = 1;
                FaceMap[Vector3.forward] = 2;
                FaceMap[Vector3.back] = 5;
                FaceMap[Vector3.right] = 4;
                FaceMap[Vector3.left] = 3;
                break;

            case DiceType.D8:
                FaceMap[Vector3.up] = 1;
                FaceMap[Vector3.down] = 8;
                FaceMap[Vector3.forward] = 2;
                FaceMap[Vector3.back] = 7;
                FaceMap[Vector3.right] = 3;
                FaceMap[Vector3.left] = 6;
                break;

            case DiceType.D10:
                FaceMap[Vector3.up] = 1;
                FaceMap[Vector3.down] = 10;
                FaceMap[Vector3.forward] = 2;
                FaceMap[Vector3.back] = 9;
                FaceMap[Vector3.right] = 3;
                FaceMap[Vector3.left] = 8;
                break;

            case DiceType.D12:
                FaceMap[Vector3.up] = 1;
                FaceMap[Vector3.down] = 12;
                FaceMap[Vector3.forward] = 2;
                FaceMap[Vector3.back] = 11;
                FaceMap[Vector3.right] = 3;
                FaceMap[Vector3.left] = 10;
                break;

            case DiceType.D20:
                FaceMap[Vector3.up] = 1;
                FaceMap[Vector3.down] = 20;
                FaceMap[Vector3.forward] = 2;
                FaceMap[Vector3.back] = 19;
                FaceMap[Vector3.right] = 3;
                FaceMap[Vector3.left] = 18;
                break;
        }
    }
}
