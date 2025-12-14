using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Android;
using UnityEngine.InputSystem.Controls;

public class PlayerMovementBehaviour : MonoBehaviour
{

    [SerializeField] float movementSpeed;
    [SerializeField] float rotationSpeed;

    //Control keys
    [SerializeField] Key fowardKey;
    [SerializeField] Key backwardKey;
    [SerializeField] Key leftKey;
    [SerializeField] Key rightKey;

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {
        Vector3 moveDirection = CalculateMoveDirection();

        transform.position = transform.position + moveDirection * movementSpeed * Time.deltaTime;

        if (moveDirection.magnitude != 0)
        {
            LookAt(moveDirection);
        }
    }

    void LookAt(Vector3 lookDirection)
    {
        Quaternion targetRotation;
        targetRotation = Quaternion.LookRotation(lookDirection); //Esto lo que hace es calcular la rotación que debería tener si estuviese girado para donde va realmente

        Quaternion newRotation;
        newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = newRotation;
    }

    Vector3 CalculateMoveDirection()
    {
        Vector3 moveVector;
        Vector3 moveNormalized;

        moveVector = new Vector3(0, 0, 0);

        if (Keyboard.current[fowardKey].isPressed)
        {
            moveVector.z = moveVector.z + 1;
        }

        if (Keyboard.current[backwardKey].isPressed)
        {
            moveVector.z = moveVector.z - 1;
        }

        if (Keyboard.current[leftKey].isPressed)
        {
            moveVector.x = moveVector.x - 1;
        }

        if (Keyboard.current[rightKey].isPressed)
        {
            moveVector.x = moveVector.x + 1;
        }

        moveNormalized = moveVector.normalized;

        return moveNormalized;
    }
}
