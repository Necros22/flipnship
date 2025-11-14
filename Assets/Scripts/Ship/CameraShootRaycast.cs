using UnityEngine;

public class CursorRayVisualizer : MonoBehaviour
{
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

    // ✅ LayerMask para ignorar el Player
    private int ignorePlayerMask;

    void Start()
    {
        cam = Camera.main;

        if (cam == null)
            Debug.LogError("No se encontró una cámara con el tag 'MainCamera'.");
        if (pivotCamera == null)
            Debug.LogError("No se asignó el PivotCamera.");

        // ✅ Excluir solo el layer Player
        ignorePlayerMask = ~LayerMask.GetMask("Player");
    }

    void Update()
    {
        if (cam == null || pivotCamera == null)
            return;

        UpdateAimRay();

        if (Input.GetMouseButtonDown(0))
            FireRay();
    }

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

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDynamicDistance, ignorePlayerMask))
        {
            currentRayDistance = hit.distance;
            Debug.DrawRay(origin, direction * currentRayDistance, idleRayColor);

            if (hit.collider.CompareTag("GravityAffected"))
            {
                Debug.DrawRay(origin, direction * currentRayDistance, Color.green);
            }
        }
        else
        {
            currentRayDistance = maxDynamicDistance;
            Debug.DrawRay(origin, direction * currentRayDistance, idleRayColor);
        }
    }

    void FireRay()
    {
        Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint;
        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxRayDistance, ignorePlayerMask))
            targetPoint = cameraHit.point;
        else
            targetPoint = cameraRay.GetPoint(maxRayDistance);

        Vector3 origin = pivotCamera.position;
        Vector3 direction = (targetPoint - origin).normalized;

        float rayLength = maxDynamicDistance;
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDynamicDistance, ignorePlayerMask))
        {
            rayLength = hit.distance;

            if (hit.collider.CompareTag("GravityAffected"))
            {
                Debug.Log($"Disparo ejecutado hacia {hit.collider.name} a {rayLength:F1} unidades.");

                CustomDirectionalGravity gravityScript = hit.collider.GetComponent<CustomDirectionalGravity>();
                if (gravityScript != null)
                {
                    gravityScript.AntiGravityChange();
                    Debug.Log($"Se llamó a AntiGravityChange() en {hit.collider.name}.");
                }
                else
                {
                    Debug.LogWarning($"{hit.collider.name} no tiene un componente CustomDirectionalGravity.");
                }
            }
            else
            {
                Debug.Log($"Disparo sin efecto: impactó en {hit.collider.name} (sin tag válida).");
            }
        }
        else
        {
            Debug.Log("Disparo al vacío (no impactó ningún objeto).");
        }

        Debug.DrawRay(origin, direction * rayLength, clickRayColor, clickRayDuration);
    }
}
