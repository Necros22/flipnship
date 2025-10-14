using UnityEngine;

public class RingTrigger : MonoBehaviour
{
    [Header("Puerta asignada a este anillo")]
    public GameObject targetWall; // Asigna aquí el muro que debe destruirse

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetWall != null)
            {
                Destroy(targetWall);
                Debug.Log($"El muro '{targetWall.name}' se ha destruido por el anillo '{name}'.");
            }
            else
            {
                Debug.LogWarning($"No se asignó un muro para el anillo '{name}'.");
            }

            // (Opcional) destruir el anillo después de activarse
            Destroy(gameObject);
        }
    }
}

