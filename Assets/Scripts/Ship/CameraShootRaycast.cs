using UnityEngine;

public class CursorRayVisualizer : MonoBehaviour
{
    [Header("Referencias")]
    public Transform pivotCamera;              // Origen del disparo (ej: cañón o pivot)

    [Header("Distancias")]
    public float maxRayDistance = 3000f;       // Distancia máxima absoluta (para rayos de cámara)
    public float maxDynamicDistance = 100f;    // Límite de detección y longitud del rayo (editable en Inspector)

    [Header("Visualización")]
    public float clickRayDuration = 30f;       // Tiempo de vida del rayo de clic
    public Color idleRayColor = Color.yellow;  // Color del rayo que se actualiza siempre
    public Color clickRayColor = Color.cyan;   // Color del rayo cuando se hace clic

    private Camera cam;
    private float currentRayDistance;          // Distancia dinámica del rayo (según impacto)

    void Start()
    {
        cam = Camera.main;

        if (cam == null)
            Debug.LogError("❌ No se encontró una cámara con el tag 'MainCamera'.");
        if (pivotCamera == null)
            Debug.LogError("❌ No se asignó el PivotCamera.");
    }

    void Update()
    {
        if (cam == null || pivotCamera == null)
            return;

        UpdateAimRay();

        // 🔹 Siempre se puede disparar, sin importar si apunta a algo o no
        if (Input.GetMouseButtonDown(0))
            FireRay();
    }

    /// <summary>
    /// Dibuja un rayo continuo desde el pivot apuntando hacia el cursor.
    /// </summary>
    void UpdateAimRay()
    {
        Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);

        // Calculamos el punto objetivo (donde apunta la cámara)
        Vector3 targetPoint;
        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxRayDistance))
            targetPoint = cameraHit.point;
        else
            targetPoint = cameraRay.GetPoint(maxRayDistance);

        // Rayo desde el pivot hacia ese punto
        Vector3 origin = pivotCamera.position;
        Vector3 direction = (targetPoint - origin).normalized;

        // Si el rayo del pivot impacta algo, ajustamos su longitud dinámica
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDynamicDistance))
        {
            currentRayDistance = hit.distance;
            Debug.DrawRay(origin, direction * currentRayDistance, idleRayColor);

            if (hit.collider.CompareTag("GravityAffected"))
            {
                Debug.DrawRay(origin, direction * currentRayDistance, Color.green); // Verde si apunta a un objeto válido
            }
        }
        else
        {
            // Si no hay impacto, usa el máximo dinámico
            currentRayDistance = maxDynamicDistance;
            Debug.DrawRay(origin, direction * currentRayDistance, idleRayColor);
        }
    }

    /// <summary>
    /// Dispara un rayo visual y llama a la función AntiGravityChange() en el objeto afectado.
    /// </summary>
    void FireRay()
    {
        Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint;
        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxRayDistance))
            targetPoint = cameraHit.point;
        else
            targetPoint = cameraRay.GetPoint(maxRayDistance);

        Vector3 origin = pivotCamera.position;
        Vector3 direction = (targetPoint - origin).normalized;

        float rayLength = maxDynamicDistance;
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDynamicDistance))
        {
            rayLength = hit.distance;

            // ✅ Solo afecta si el objeto tiene la tag "GravityAffected"
            if (hit.collider.CompareTag("GravityAffected"))
            {
                Debug.Log($"💥 Disparo ejecutado hacia {hit.collider.name} a {rayLength:F1} unidades.");

                // Llamar a la función AntiGravityChange del script CustomDirectionalGravity
                CustomDirectionalGravity gravityScript = hit.collider.GetComponent<CustomDirectionalGravity>();
                if (gravityScript != null)
                {
                    gravityScript.AntiGravityChange();
                    Debug.Log($"🪐 Se llamó a AntiGravityChange() en {hit.collider.name}.");
                }
                else
                {
                    Debug.LogWarning($"⚠️ {hit.collider.name} no tiene un componente CustomDirectionalGravity.");
                }
            }
            else
            {
                Debug.Log($"🔸 Disparo sin efecto: impactó en {hit.collider.name} (sin tag válida).");
            }
        }
        else
        {
            // No golpea nada dentro del rango dinámico
            Debug.Log("🚀 Disparo al vacío (no impactó ningún objeto).");
        }

        // Dibujar el rayo con la longitud hasta el impacto o el máximo dinámico
        Debug.DrawRay(origin, direction * rayLength, clickRayColor, clickRayDuration);
    }
}
