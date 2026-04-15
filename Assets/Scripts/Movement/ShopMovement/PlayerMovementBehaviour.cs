using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class PlayerMovementBehaviour : MonoBehaviour
{
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float rotationSpeed = 720f;

    // Teclas configurables desde el inspector
    [SerializeField] Key forwardKey = Key.W;
    [SerializeField] Key backwardKey = Key.S;
    [SerializeField] Key leftKey = Key.A;
    [SerializeField] Key rightKey = Key.D;

    // Margen extra para evitar clipping
    [SerializeField] float extraMargin = 1f;

    Rigidbody rb;
    Transform board;
    BoxCollider boardCollider;
    BoxCollider pointerCollider;

    Vector3 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        pointerCollider = GetComponent<BoxCollider>();

        GameObject boardObj = GameObject.Find("Tablero");
        board = boardObj.transform;
        boardCollider = board.GetComponent<BoxCollider>();
    }

    void Update()
    {
        moveDirection = CalculateMoveDirection();
    }

    void FixedUpdate()
    {
        MoveWithPhysics();
        if (moveDirection.sqrMagnitude < 0.01f)
            rb.linearVelocity = Vector3.zero;
        ClampInsideBoard_Local_WithOffset();
        StickToBoardSurface();
    }


    void MoveWithPhysics()
    {
        if (moveDirection.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        rb.linearVelocity = moveDirection * movementSpeed;

        RotateTowards(moveDirection);
    }


    void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion newRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);
    }

    void StickToBoardSurface()
    {
        float boardTopY = board.position.y
                          + boardCollider.center.y
                          + (boardCollider.size.y * 0.5f);

        float pointerHalfHeight = pointerCollider.size.y * 0.5f;

        Vector3 pos = rb.position;
        pos.y = boardTopY + pointerHalfHeight;
        rb.position = pos;
    }

    void ClampInsideBoard_Local_WithOffset()
    {
        Vector3 localPos = board.InverseTransformPoint(rb.position);

        Vector3 boardMin = boardCollider.center - boardCollider.size * 0.5f;
        Vector3 boardMax = boardCollider.center + boardCollider.size * 0.5f;

        Vector3 ext = pointerCollider.size * 0.5f;
        Vector3 off = pointerCollider.center;

        float forwardMargin = Mathf.Abs(off.z + ext.z) + extraMargin;
        float backwardMargin = Mathf.Abs(ext.z - off.z) + extraMargin;
        float rightMargin = Mathf.Abs(off.x + ext.x) + extraMargin;
        float leftMargin = Mathf.Abs(ext.x - off.x) + extraMargin;

        localPos.x = Mathf.Clamp(localPos.x, boardMin.x + leftMargin, boardMax.x - rightMargin);
        localPos.z = Mathf.Clamp(localPos.z, boardMin.z + backwardMargin, boardMax.z - forwardMargin);

        rb.position = board.TransformPoint(localPos);
    }

    Vector3 CalculateMoveDirection()
    {
        Vector3 moveVector = Vector3.zero;

        if (Keyboard.current[forwardKey].isPressed) moveVector.z -= 1;
        if (Keyboard.current[backwardKey].isPressed) moveVector.z += 1;
        if (Keyboard.current[leftKey].isPressed) moveVector.x += 1;
        if (Keyboard.current[rightKey].isPressed) moveVector.x -= 1;

        return moveVector.normalized;
    }
}
