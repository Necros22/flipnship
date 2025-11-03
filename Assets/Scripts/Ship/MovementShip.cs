using UnityEngine;

public class MovementShip : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;

    public GameObject ship;

    [Header("Rotation Settings")]
    [SerializeField] private float maxRotationAngle = 30f;
    [SerializeField] private float rotationSmooth = 5f;

    private float horizontalInput;
    private float verticalInput;

    private int cameraRotIndex = 0; // 0=normal,1=90°,2=180°,3=270°

    public void SetRotationIndex(int index)
    {
        cameraRotIndex = index;
    }

    void Update()
    {
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

        // Re-map WASD based on camera rotation
        switch (cameraRotIndex)
        {
            case 0: // normal
                horizontalInput = rawH;
                verticalInput = rawV;
                break;

            case 1: // 90° giro
                horizontalInput = -rawV;
                verticalInput = rawH;
                break;

            case 2: // 180°
                horizontalInput = -rawH;
                verticalInput = -rawV;
                break;

            case 3: // 270°
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
