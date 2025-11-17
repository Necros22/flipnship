using UnityEngine;

public class MovementShip : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;

    public GameObject ship;

    [Header("Movement Limits")]
    [SerializeField] private bool useLimits = true;
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = 3f;

    [Header("Rotation Settings")]
    [SerializeField] private float maxRotationAngle = 30f;
    [SerializeField] private float rotationSmooth = 5f;

    [Header("Control")]
    public bool movementEnabled = true; // <<--- DESACTIVAR MOVIMIENTO COMPLETO

    private float horizontalInput;
    private float verticalInput;

    private int cameraRotIndex = 0; // 0=normal,1=90°,2=180°,3=270°

    public void SetRotationIndex(int index)
    {
        cameraRotIndex = index;
    }

    void Update()
    {
        if (!movementEnabled) return;      // <<--- SI ESTA EN FALSE NO PUEDE MOVERSE
        if (Time.timeScale == 0f) return;

        HandleInput();
        MoveForward();
        Move();
        RotateShip();
    }

    void HandleInput()
    {
        float rawH = Input.GetAxisRaw("Horizontal");
        float rawV = Input.GetAxisRaw("Vertical");

        switch (cameraRotIndex)
        {
            case 0:
                horizontalInput = rawH;
                verticalInput = rawV;
                break;

            case 1:
                horizontalInput = -rawV;
                verticalInput = rawH;
                break;

            case 2:
                horizontalInput = -rawH;
                verticalInput = -rawV;
                break;

            case 3:
                horizontalInput = rawV;
                verticalInput = -rawH;
                break;
        }
    }

    private void MoveForward()
    {
        transform.position += new Vector3(0, 0, speed * Time.deltaTime);
    }

    private void Move()
    {
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        if (useLimits)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }
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
