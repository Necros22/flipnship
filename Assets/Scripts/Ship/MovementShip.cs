using UnityEngine;

public class MovementShip : MonoBehaviour
{
    // -----------------------------------------------------
    //                 MOVEMENT SETTINGS
    // -----------------------------------------------------
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 5f;        // velocidad normal
    [SerializeField] private float boostedForwardSpeed = 12f; // velocidad aumentada al presionar espacio

    [SerializeField] private float lateralSpeed = 5f;

    [Header("Optional Camera Forward Movement")]
    public Transform cameraToMoveForward;

    [Header("Ship Model (for tilting)")]
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
    public bool movementEnabled = true;

    private float horizontalInput;
    private float verticalInput;
    private int cameraRotIndex = 0;


    // ---------------------------------------------
    //         ROTACIÓN DE INPUT SEGÚN CÁMARA
    // ---------------------------------------------
    public void SetRotationIndex(int index)
    {
        cameraRotIndex = index;
    }

    void Update()
    {
        if (!movementEnabled) return;
        if (Time.timeScale == 0f) return;

        HandleInput();
        MoveForward();
        MoveLaterally();
        RotateShip();
    }

    // ---------------------------------------------
    //                INPUT
    // ---------------------------------------------
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

    // ---------------------------------------------
    //       MOVIMIENTO HACIA ADELANTE (Z)
    // ---------------------------------------------
    private void MoveForward()
    {
        float currentSpeed = Input.GetKey(KeyCode.Space) ? boostedForwardSpeed : forwardSpeed;

        // mover nave
        transform.position += new Vector3(0, 0, currentSpeed * Time.deltaTime);

        // mover cámara opcionalmente
        if (cameraToMoveForward != null)
        {
            cameraToMoveForward.position += new Vector3(0, 0, currentSpeed * Time.deltaTime);
        }
    }

    // ---------------------------------------------
    //  MOVIMIENTO LATERAL (IZQ-DER / ARRIBA-ABAJO)
    // ---------------------------------------------
    private void MoveLaterally()
    {
        Vector3 move = new Vector3(horizontalInput, verticalInput, 0)
                       * lateralSpeed * Time.deltaTime;

        transform.Translate(move, Space.World);

        if (useLimits)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }
    }

    // ---------------------------------------------
    //            ROTACIÓN VISUAL DE LA NAVE
    // ---------------------------------------------
    private void RotateShip()
    {
        if (ship == null) return;

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
