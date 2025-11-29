using UnityEngine;

public class SawController : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Si está activado, esta sierra NO puede ser paralizada por ZeroGravity.")]
    public bool ignoreParalysis = false;   // <<--- NUEVA VARIABLE

    [Header("Referencias")]
    public Animator sawAnimator;
    public GameObject activeCollider;   // Collider grande (encendida)
    public GameObject offCollider;      // Collider chico (apagada)

    [Header("Estado actual")]
    public bool isParalyzed = false;

    void Start()
    {
        SetActiveState(true); // Comienza encendida por defecto
    }

    // Llamado cuando el rayo ZeroGravity la paraliza
    public void Paralyze()
    {
        // <<--- NUEVA PROTECCIÓN
        if (ignoreParalysis)
        {
            Debug.Log("Sierra con ignoreParalysis activado → no se paraliza.");
            return;
        }

        isParalyzed = true;
        SetActiveState(false);
        Debug.Log("Sierra paralizada");
    }

    // Llamado cuando otro objeto recibe el disparo → esta sierra deja de estar paralizada
    public void UnParalyze()
    {
        if (!isParalyzed) return;

        isParalyzed = false;
        SetActiveState(true);
        Debug.Log("Sierra reactivada");
    }

    private void SetActiveState(bool active)
    {
        activeCollider.SetActive(active);
        offCollider.SetActive(!active);

        if (sawAnimator != null)
        {
            sawAnimator.SetBool("activeSaw", active);
        }
    }
}
