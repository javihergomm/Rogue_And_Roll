using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class DiceRoller : MonoBehaviour
{
    private Rigidbody rb;
    private Camera cam;

    [Header("Dice Settings")]
    [SerializeField] private DiceType diceType = DiceType.D6;

    // Axis -> face value mapping
    private Dictionary<Vector3, int> faceMap;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        InitFaceMap();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    RollDice();
                    StartCoroutine(PrintFaceUpWhenStopped());
                }
            }
        }
    }

    private void RollDice()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 50f, ForceMode.Impulse);
    }

    private IEnumerator PrintFaceUpWhenStopped()
    {
        while (!rb.IsSleeping())
        {
            yield return null;
        }

        int faceUp = GetFaceUp(true); // pass true to enable detailed debug
        Debug.Log("[" + diceType + "] Final face up: " + faceUp);
    }

    private int GetFaceUp(bool verbose = false)
    {
        float bestDot = -1f;
        int bestValue = 0;
        Vector3 bestAxis = Vector3.zero;

        foreach (var kvp in faceMap)
        {
            Vector3 worldAxis = transform.TransformDirection(kvp.Key);
            float dot = Vector3.Dot(worldAxis, Vector3.up);

            if (verbose)
            {
                Debug.Log("Axis " + kvp.Key + " (world " + worldAxis + ") -> Face " + kvp.Value + " | Dot: " + dot);
            }

            if (dot > bestDot)
            {
                bestDot = dot;
                bestValue = kvp.Value;
                bestAxis = kvp.Key;
            }
        }

        if (verbose)
        {
            Debug.Log("Winning axis: " + bestAxis + " -> Face " + bestValue + " (Dot " + bestDot + ")");
        }

        return bestValue;
    }

    public void InitFaceMap()
    {
        faceMap = new Dictionary<Vector3, int>();

        switch (diceType)
        {
            case DiceType.D4:
                faceMap[Vector3.up] = 1;
                faceMap[Vector3.forward] = 2;
                faceMap[Vector3.right] = 3;
                faceMap[Vector3.left] = 4;
                break;

            case DiceType.D6:
                faceMap[Vector3.up] = 6;
                faceMap[Vector3.down] = 1;
                faceMap[Vector3.forward] = 2;
                faceMap[Vector3.back] = 5;
                faceMap[Vector3.right] = 4;
                faceMap[Vector3.left] = 3;
                break;

            case DiceType.D8:
                faceMap[Vector3.up] = 1;
                faceMap[Vector3.down] = 8;
                faceMap[Vector3.forward] = 2;
                faceMap[Vector3.back] = 7;
                faceMap[Vector3.right] = 3;
                faceMap[Vector3.left] = 6;
                break;

            case DiceType.D10:
                faceMap[Vector3.up] = 1;
                faceMap[Vector3.down] = 10;
                faceMap[Vector3.forward] = 2;
                faceMap[Vector3.back] = 9;
                faceMap[Vector3.right] = 3;
                faceMap[Vector3.left] = 8;
                break;

            case DiceType.D12:
                faceMap[Vector3.up] = 1;
                faceMap[Vector3.down] = 12;
                faceMap[Vector3.forward] = 2;
                faceMap[Vector3.back] = 11;
                faceMap[Vector3.right] = 3;
                faceMap[Vector3.left] = 10;
                break;

            case DiceType.D20:
                faceMap[Vector3.up] = 1;
                faceMap[Vector3.down] = 20;
                faceMap[Vector3.forward] = 2;
                faceMap[Vector3.back] = 19;
                faceMap[Vector3.right] = 3;
                faceMap[Vector3.left] = 18;
                break;
        }
    }

    // Expose faceMap for editor helper
    public Dictionary<Vector3, int> FaceMap => faceMap;

    // Editor-only helper to test face up without rolling
    public int EditorTestFaceUp()
    {
        return GetFaceUp(true); // always verbose in editor
    }
}

public enum DiceType { D4, D6, D8, D10, D12, D20 }
