using UnityEngine;

public class CameraShootRaycast : MonoBehaviour
{
    public enum ShotType
    {
        AntiGravity = 1,
        ZeroGravity = 2,
        MaxGravity = 3
    }

    [Header("Referencias")]
    public Transform pivotCamera;

    [Header("Distancias")]
    public float maxRayDistance = 3000f;
    public float maxDynamicDistance = 100f;

    [Header("Visualización")]
    public float clickRayDuration = 30f;
    public Color idleRayColor = Color.yellow;
    public Color clickRayColor = Color.cyan;

    private Camera cam;
    private float currentRayDistance;
    private int ignorePlayerMask;

    // ------------------------------------
    //      CONTROL DE DISPAROS NUEVO
    // ------------------------------------
    [Header("Control General de Disparos")]
    public bool shootingEnabled = true;   // ← Activar/desactivar todo el sistema

    [Header("Desbloqueo de Tipos de Disparo")]
    public bool unlockAntiGravity = true;
    public bool unlockZeroGravity = true;
    public bool unlockMaxGravity = true;

    [Header("Tipo de Disparo Actual")]
    public ShotType currentShotType = ShotType.AntiGravity; // ← Por defecto el 1

    void Start()
    {
        cam = Camera.main;

        if (cam == null)
            Debug.LogError("No se encontró una cámara con el tag 'MainCamera'.");

        if (pivotCamera == null)
            Debug.LogError("No se asignó el PivotCamera.");

        // Ignorar layer Player
        ignorePlayerMask = ~LayerMask.GetMask("Player");
    }

    void Update()
    {
        if (cam == null || pivotCamera == null)
            return;

        HandleShotSelection();
        UpdateAimRay();

        if (Input.GetMouseButtonDown(0))
            FireRay();
    }

    // ------------------------------------
    //        SELECCIÓN DE DISPARO
    // ------------------------------------
    void HandleShotSelection()
    {
        if (!shootingEnabled) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && unlockAntiGravity)
        {
            print("AntiGravity");
            currentShotType = ShotType.AntiGravity;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && unlockZeroGravity)
        {
            print("ZeroGravity");
            currentShotType = ShotType.ZeroGravity;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && unlockMaxGravity)
        {
            print("MaxGravity");
            currentShotType = ShotType.MaxGravity;
        }
    }

    // ------------------------------------
    //              RAY VISUAL
    // ------------------------------------
    void UpdateAimRay()
    {
        Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);

        Vector3 targetPoint;
        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxRayDistance, ignorePlayerMask))
            targetPoint = cameraHit.point;
        else
            targetPoint = cameraRay.GetPoint(maxRayDistance);

        Vector3 origin = pivotCamera.position;
        Vector3 direction = (targetPoint - origin).normalized;

        // 🌟 NUEVA FUNCIÓN: Raycast que ignora botones
        bool hitSomething = RaycastIgnoringButtons(origin, direction, out RaycastHit hit, maxDynamicDistance);

        if (hitSomething)
        {
            currentRayDistance = hit.distance;

            if (hit.collider.CompareTag("GravityAffected"))
                Debug.DrawRay(origin, direction * currentRayDistance, Color.green);
            else
                Debug.DrawRay(origin, direction * currentRayDistance, idleRayColor);
        }
        else
        {
            currentRayDistance = maxDynamicDistance;
            Debug.DrawRay(origin, direction * currentRayDistance, idleRayColor);
        }
    }

    // ------------------------------------
    //      RAYCAST QUE IGNORA BOTONES
    // ------------------------------------
    bool RaycastIgnoringButtons(Vector3 origin, Vector3 direction, out RaycastHit validHit, float maxDist)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDist, ignorePlayerMask);

        // Ordena por distancia
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit h in hits)
        {
            if (h.collider.CompareTag("Button"))
                continue; // ← LO IGNORA Y SIGUE BUSCANDO

            validHit = h;
            return true;
        }

        validHit = default;
        return false;
    }

    // ------------------------------------
    //              DISPARO
    // ------------------------------------
    void FireRay()
    {
        if (!shootingEnabled)
        {
            Debug.Log("Disparo desactivado.");
            return;
        }

        Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint;

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxRayDistance, ignorePlayerMask))
            targetPoint = cameraHit.point;
        else
            targetPoint = cameraRay.GetPoint(maxRayDistance);

        Vector3 origin = pivotCamera.position;
        Vector3 direction = (targetPoint - origin).normalized;

        // 🌟 Usa el raycast que ignora botones
        bool hitSomething = RaycastIgnoringButtons(origin, direction, out RaycastHit hit, maxDynamicDistance);

        float rayLength = maxDynamicDistance;

        if (hitSomething)
        {
            rayLength = hit.distance;

            Debug.DrawRay(origin, direction * rayLength, clickRayColor, clickRayDuration);

            if (!hit.collider.CompareTag("GravityAffected"))
            {
                Debug.Log($"Impacto sin efecto en {hit.collider.name}");
                return;
            }

            CustomDirectionalGravity gravityScript = hit.collider.GetComponent<CustomDirectionalGravity>();

            if (gravityScript == null)
            {
                Debug.LogWarning($"{hit.collider.name} no tiene CustomDirectionalGravity.");
                return;
            }

            // --------------------------------
            //       APLICAR EFECTO SEGÚN TIPO
            // --------------------------------
            switch (currentShotType)
            {
                case ShotType.AntiGravity:
                    gravityScript.AntiGravityChange();
                    Debug.Log("Disparo AntiGravity → AntiGravityChange()");
                    break;

                case ShotType.ZeroGravity:
                    gravityScript.ActivateZeroGravity();
                    Debug.Log("Disparo ZeroGravity → ActivateZeroGravity()");
                    break;

                case ShotType.MaxGravity:
                    gravityScript.ActivateMaxGravity();
                    Debug.Log("Disparo MaxGravity → ActivateMaxGravity()");
                    break;
            }
        }
        else
        {
            Debug.Log("Disparo al vacío.");
            Debug.DrawRay(origin, direction * rayLength, clickRayColor, clickRayDuration);
        }
    }
}
