using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementBehaviour : MonoBehaviour
{
    [SerializeField] float movementSpeed = 5f;   // Speed of movement
    [SerializeField] float rotationSpeed = 720f; // Speed of rotation in degrees per second

    /* Control keys with default values set to WASD.
       These can still be customized in the Unity Inspector. */
    [SerializeField] Key forwardKey = Key.W;   // Default forward key
    [SerializeField] Key backwardKey = Key.S;  // Default backward key
    [SerializeField] Key leftKey = Key.A;      // Default left key
    [SerializeField] Key rightKey = Key.D;     // Default right key

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    // Handles movement and rotation
    void Move()
    {
        Vector3 moveDirection = CalculateMoveDirection();

        // Move the player in the chosen direction
        transform.position += moveDirection * movementSpeed * Time.deltaTime;

        // Rotate towards movement direction if moving
        if (moveDirection.magnitude != 0)
        {
            LookAt(moveDirection);
        }
    }

    // Smoothly rotates the player to face the movement direction
    void LookAt(Vector3 lookDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection); // Desired rotation
        Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime); // Smooth rotation
        transform.rotation = newRotation;
    }

    // Calculates the movement direction based on pressed keys
    Vector3 CalculateMoveDirection()
    {
        Vector3 moveVector = Vector3.zero;

        // Forward movement (W by default)
        if (Keyboard.current[forwardKey].isPressed)
            moveVector.z -= 1;

        // Backward movement (S by default)
        if (Keyboard.current[backwardKey].isPressed)
            moveVector.z += 1;

        // Left movement (A by default)
        if (Keyboard.current[leftKey].isPressed)
            moveVector.x += 1;

        // Right movement (D by default)
        if (Keyboard.current[rightKey].isPressed)
            moveVector.x -= 1;

        // Normalize to prevent faster diagonal movement
        return moveVector.normalized;
    }
}
