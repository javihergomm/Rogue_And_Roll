using UnityEngine;
using UnityEngine.InputSystem; // New Input System
using System.Collections;

public class DiceRoller : MonoBehaviour
{
    private Rigidbody rb;
    private Camera cam;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
    }

    void Update()
    {
        // Detect left mouse click (or tap)
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

        int faceUp = GetFaceUp();
        Debug.Log(faceUp);
    }

    private int GetFaceUp()
    {
        if (Vector3.Dot(transform.up, Vector3.up) > 0.9f) return 6;
        if (Vector3.Dot(-transform.up, Vector3.up) > 0.9f) return 1;
        if (Vector3.Dot(transform.forward, Vector3.up) > 0.9f) return 2;
        if (Vector3.Dot(-transform.forward, Vector3.up) > 0.9f) return 5;
        if (Vector3.Dot(transform.right, Vector3.up) > 0.9f) return 4;   
        if (Vector3.Dot(-transform.right, Vector3.up) > 0.9f) return 3;  
        return 0;
    }
}
