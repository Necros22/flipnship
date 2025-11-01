using UnityEngine;

public class CursorRayVisualizer : MonoBehaviour
{
    [Header("Configuración del rayo")]
    public Transform pivotCamera;       // Punto de origen del rayo
    public float rayDistance = 1000f;   // Alcance máximo del raycast

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CastRayToCursor();
        }
    }

    void CastRayToCursor()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("❌ No se encontró una cámara con el tag 'MainCamera'.");
            return;
        }

        if (pivotCamera == null)
        {
            Debug.LogError("❌ No se asignó el PivotCamera. Asigna un objeto en el Inspector.");
            return;
        }

        // Rayo desde la cámara hacia el cursor
        Ray screenRay = cam.ScreenPointToRay(Input.mousePosition);

        // Origen desde el PivotCamera, dirección hacia donde apunta el cursor
        Vector3 rayOrigin = pivotCamera.position;
        Vector3 rayDirection = (screenRay.GetPoint(rayDistance) - rayOrigin).normalized;

        // Visualiza el rayo en la Scene View
        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red, 2f);

        // Lanza el raycast sin filtrar por layer
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance))
        {
            // Solo reacciona si el objeto tiene el tag correcto
            if (hit.collider.CompareTag("GravityAffected"))
            {
                Debug.Log($"✅ Impacto con objeto con tag 'GravityAffected': {hit.collider.name}");

                // Ejemplo visual: marcar el punto de impacto
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.position = hit.point;
                marker.transform.localScale = Vector3.one * 0.2f;
                Destroy(marker.GetComponent<Collider>());
                Destroy(marker, 1.5f);
            }
            else
            {
                Debug.Log($"⚪ Impacto con objeto sin tag 'GravityAffected': {hit.collider.name}");
            }
        }
        else
        {
            Debug.Log("⛔ No se impactó ningún objeto.");
        }
    }
}
