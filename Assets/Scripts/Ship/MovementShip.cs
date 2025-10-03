using UnityEngine;

public class MovementShip : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;

    public GameObject ship;

    [Header("Rotation Settings")]
    [SerializeField] private float maxRotationAngle = 30f;
    [SerializeField] private float rotationSmooth = 5f;

    private void Update()
    {
        MoveForward();
        Move();
        RotateShip();
    }

    private float horizontalInput;
    private float verticalInput;

    private void MoveForward()
    {
        transform.position += new Vector3(0, 0, speed * Time.deltaTime);
    }

    private void Move()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * speed * Time.deltaTime;

        transform.Translate(movement, Space.World);
    }

    private void RotateShip()
    {
        float targetY = horizontalInput * maxRotationAngle;
        float targetX = -verticalInput * maxRotationAngle;

        Quaternion targetRotation = Quaternion.Euler(targetX, targetY, 0);

        ship.transform.localRotation = Quaternion.Lerp(
            ship.transform.localRotation,
            targetRotation,
            Time.deltaTime * rotationSmooth
        );
    }
}
