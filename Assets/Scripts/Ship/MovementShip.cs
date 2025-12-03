using UnityEngine;

public class MovementShip : MonoBehaviour
{
    // -----------------------------------------------------
    //                 MOVEMENT SETTINGS
    // -----------------------------------------------------
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float boostedForwardSpeed = 12f;

    [SerializeField] private float lateralSpeed = 5f;

    // -----------------------------------------------------
    //        OPTIONAL CAMERA FORWARD MOVEMENT
    // -----------------------------------------------------
    [Header("Optional Camera Forward Movement")]
    public Transform cameraToMoveForward;
    [SerializeField] private bool cameraMovesForward = true;

    // -----------------------------------------------------
    //         CAMERA INTRO ANIMATION
    // -----------------------------------------------------
    [Header("Camera Intro Animation")]
    public Transform cameraPivotTarget;
    [SerializeField] private float cameraDelay = 2f;
    [SerializeField] private float cameraMoveDuration = 3f;

    [SerializeField] private GameObject objectToDisableAfterIntro;

    private bool cameraAnimationStarted = false;
    private bool cameraAnimationFinished = false;
    private float cameraAnimationTimer = 0f;

    private Vector3 camStartPos;
    private Quaternion camStartRot;

    // -----------------------------------------------------
    //   REFERENCES TO EXTERNAL SYSTEMS (NEW)
    // -----------------------------------------------------
    [Header("GravityControl Settings To Apply After Intro")]
    public GravityControl gravityControl;
    public bool enableRotateQ = true;
    public bool enableRotateE = true;

    [Header("CameraShootRaycast Settings To Apply After Intro")]
    public CameraShootRaycast raycastShoot;
    public bool enableShooting = true;
    public bool enableAntiGravityShot = true;
    public bool enableZeroGravityShot = true;
    public bool enableMaxGravityShot = true;

    // -----------------------------------------------------
    //                     SHIP MODEL
    // -----------------------------------------------------
    [Header("Ship Model (for tilting)")]
    public GameObject ship;

    // -----------------------------------------------------
    //                    LIMITS
    // -----------------------------------------------------
    [Header("Movement Limits")]
    [SerializeField] private bool useLimits = true;
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = 3f;

    // -----------------------------------------------------
    //                    ROTATION
    // -----------------------------------------------------
    [Header("Rotation Settings")]
    [SerializeField] private float maxRotationAngle = 30f;
    [SerializeField] private float rotationSmooth = 5f;

    // -----------------------------------------------------
    //                    CONTROL
    // -----------------------------------------------------
    [Header("Control")]
    public bool movementEnabled = true;
    public bool allowDirectionalMovement = false;

    private float horizontalInput;
    private float verticalInput;
    private int cameraRotIndex = 0;



    public void SetRotationIndex(int index)
    {
        cameraRotIndex = index;
    }



    void Update()
    {
        if (!movementEnabled) return;
        if (Time.timeScale == 0f) return;

        HandleCameraIntroAnimation();

        HandleInput();
        MoveForward();

        if (allowDirectionalMovement)
            MoveLaterally();

        RotateShip();
    }



    // ========================================================================
    //            CAMERA INTRO ANIMATION
    // ========================================================================
    private void HandleCameraIntroAnimation()
    {
        if (cameraToMoveForward == null || cameraPivotTarget == null) return;
        if (cameraAnimationFinished) return;

        if (!cameraAnimationStarted)
        {
            cameraDelay -= Time.deltaTime;

            if (cameraDelay <= 0f)
            {
                cameraAnimationStarted = true;

                camStartPos = cameraToMoveForward.position;
                camStartRot = cameraToMoveForward.rotation;

                cameraMovesForward = false;
            }
            return;
        }

        cameraAnimationTimer += Time.deltaTime;
        float t = cameraAnimationTimer / cameraMoveDuration;

        if (t > 1f) t = 1f;
        t = Mathf.SmoothStep(0f, 1f, t);

        cameraToMoveForward.position = Vector3.Lerp(
            camStartPos,
            cameraPivotTarget.position,
            t
        );

        cameraToMoveForward.rotation = Quaternion.Lerp(
            camStartRot,
            cameraPivotTarget.rotation,
            t
        );

        if (t >= 1f)
        {
            cameraAnimationFinished = true;

            cameraMovesForward = true;

            if (objectToDisableAfterIntro != null)
                objectToDisableAfterIntro.SetActive(false);

            allowDirectionalMovement = true;

            ApplyExternalSettings();   // ← NUEVO: aplicar permisos externos
        }
    }



    // ========================================================================
    //           APPLY SETTINGS TO OTHER SCRIPTS (NEW)
    // ========================================================================
    private void ApplyExternalSettings()
    {
        // -------------------- GRAVITY CONTROL --------------------
        if (gravityControl != null)
        {
            gravityControl.allowRotateQ = enableRotateQ;
            gravityControl.allowRotateE = enableRotateE;
        }

        // -------------------- CAMERA SHOOT ------------------------
        if (raycastShoot != null)
        {
            raycastShoot.shootingEnabled = enableShooting;
            raycastShoot.unlockAntiGravity = enableAntiGravityShot;
            raycastShoot.unlockZeroGravity = enableZeroGravityShot;
            raycastShoot.unlockMaxGravity = enableMaxGravityShot;
        }
    }



    // ========================================================================
    //                                INPUT
    // ========================================================================
    void HandleInput()
    {
        if (!allowDirectionalMovement)
        {
            horizontalInput = 0;
            verticalInput = 0;
            return;
        }

        float rawH = Input.GetAxisRaw("Horizontal");
        float rawV = Input.GetAxisRaw("Vertical");

        switch (cameraRotIndex)
        {
            case 0: horizontalInput = rawH; verticalInput = rawV; break;
            case 1: horizontalInput = -rawV; verticalInput = rawH; break;
            case 2: horizontalInput = -rawH; verticalInput = -rawV; break;
            case 3: horizontalInput = rawV; verticalInput = -rawH; break;
        }
    }




    // ========================================================================
    //                      MOVE FORWARD
    // ========================================================================
    private void MoveForward()
    {
        float currentSpeed = Input.GetKey(KeyCode.Space) ? boostedForwardSpeed : forwardSpeed;

        transform.position += new Vector3(0, 0, currentSpeed * Time.deltaTime);

        if (cameraMovesForward && cameraAnimationFinished && cameraToMoveForward != null)
            cameraToMoveForward.position += new Vector3(0, 0, currentSpeed * Time.deltaTime);
    }



    // ========================================================================
    //                        MOVE LATERALLY
    // ========================================================================
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



    // ========================================================================
    //                          ROTATE SHIP
    // ========================================================================
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
