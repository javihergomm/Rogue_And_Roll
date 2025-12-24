using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

/*
 * DiceRoller
 * ----------
 * Handles physics-based rolling and face detection for a dice prefab.
 * Compatible with DiceRollerEditorHelper.
 */
public class DiceRoller : MonoBehaviour
{
    private Rigidbody rb;
    private Camera cam;

    private DiceSO diceData;
    private ItemSlot linkedSlot;

    [Header("Dice Settings")]
    [SerializeField] private DiceType diceType = DiceType.D6;

    // PUBLIC so the custom editor can access it
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
                    StartCoroutine(WaitForStop());
                }
            }
        }
    }

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

    private IEnumerator WaitForStop()
    {
        yield return new WaitForSeconds(0.5f);

        while (!rb.IsSleeping())
            yield return null;

        int result = GetFaceUp(false);

        Debug.Log("Dice rolled: " + result);

        isRolling = false;
    }

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

    // REQUIRED by the custom editor
    public int EditorTestFaceUp()
    {
        return GetFaceUp(false);
    }

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
