using UnityEngine;

public class CustomDirectionalGravity : MonoBehaviour
{
    // ---------------------------------------
    //                ENUMS
    // ---------------------------------------
    public enum GravityDirection { Right, Down, Left, Up }
    public enum ObjectState { Default, ZeroGravity, MaxGravity }

    // ---------------------------------------
    //          PERMISOS DE FUNCIONES
    // ---------------------------------------
    [Header("Permisos de efectos / funciones")]
    public bool canUseCustomGravity = true;
    public bool canUseAntiGravity = true;
    public bool canActivateZeroGravity = true;
    public bool canActivateMaxGravity = true;

    [Tooltip("Si está en false, este objeto NO permitirá cambios manuales globales de gravedad.")]
    public bool acceptsManualGravityChange = true;

    // ---------------------------------------
    //         CONFIGURACIÓN DE GRAVEDAD
    // ---------------------------------------
    [Header("Configuración de gravedad personalizada")]
    public GravityDirection gravityDirection = GravityDirection.Down;
    public float gravityStrength = 9.81f;
    public float maxGravityMultiplier = 100f;

    // ---------------------------------------
    //        ESTADO ACTUAL DEL OBJETO
    // ---------------------------------------
    [Header("Estado del objeto")]
    public ObjectState currentState = ObjectState.Default;

    // ---------------------------------------
    //           VARIABLES PRIVADAS
    // ---------------------------------------
    private Rigidbody rb;
    private Coroutine maxGravityCoroutine;

    // ---------------------------------------
    //               UNITY METHODS
    // ---------------------------------------
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.useGravity = false;
    }

    void FixedUpdate()
    {
        Vector3 direction = GetGravityDirection();

        switch (currentState)
        {
            case ObjectState.Default:
                rb?.AddForce(direction * gravityStrength, ForceMode.Acceleration);
                break;

            case ObjectState.ZeroGravity:
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                break;

            case ObjectState.MaxGravity:
                rb?.AddForce(direction * (gravityStrength * maxGravityMultiplier), ForceMode.Acceleration);
                break;
        }
    }

    // ---------------------------------------
    //       OBTENER DIRECCIÓN DE GRAVEDAD
    // ---------------------------------------
    private Vector3 GetGravityDirection()
    {
        if (!canUseCustomGravity)
            return Vector3.zero;

        return gravityDirection switch
        {
            GravityDirection.Up => Vector3.up,
            GravityDirection.Down => Vector3.down,
            GravityDirection.Left => Vector3.left,
            GravityDirection.Right => Vector3.right,
            _ => Vector3.zero
        };
    }

    // ---------------------------------------
    //         ANTI GRAVEDAD DESDE RAYOS
    // ---------------------------------------
    public void AntiGravityChange()
    {
        if (!canUseAntiGravity)
            return;

        // NUEVO: si está en ZeroGravity, se apaga
        if (currentState == ObjectState.ZeroGravity)
        {
            Debug.Log("ZeroGravity cancelada por AntiGravity.");
            currentState = ObjectState.Default;
        }

        // Ahora sí aplicar AntiGravity normal
        switch (gravityDirection)
        {
            case GravityDirection.Up: gravityDirection = GravityDirection.Down; break;
            case GravityDirection.Down: gravityDirection = GravityDirection.Up; break;
            case GravityDirection.Left: gravityDirection = GravityDirection.Right; break;
            case GravityDirection.Right: gravityDirection = GravityDirection.Left; break;
        }

        Debug.Log($"Gravedad invertida por rayo: ahora apunta hacia {gravityDirection}");
    }

    // ---------------------------------------
    //     CAMBIO DE DIRECCIÓN LIBRE (RAYOS)
    // ---------------------------------------
    public void SetGravityDirection(GravityDirection newDirection)
    {
        gravityDirection = newDirection;
        Debug.Log($"Dirección de gravedad cambiada a {newDirection}");
    }

    // ---------------------------------------
    //     CAMBIO DE DIRECCIÓN MANUAL GLOBAL
    // ---------------------------------------
    public void SetGravityDirectionManual(GravityDirection newDirection)
    {
        if (!acceptsManualGravityChange)
        {
            Debug.Log("Este objeto NO acepta cambios manuales de gravedad.");
            return;
        }

        gravityDirection = newDirection;
        Debug.Log($"Dirección de gravedad cambiada manualmente a {newDirection}");
    }

    // ---------------------------------------
    //           ACTIVAR ZERO GRAVITY
    // ---------------------------------------
    public void ActivateZeroGravity()
    {
        if (!canActivateZeroGravity)
            return;

        currentState = ObjectState.ZeroGravity;
        Debug.Log("ZeroGravity activada");
    }

    // ---------------------------------------
    //           DESACTIVAR ZERO GRAVITY
    // ---------------------------------------
    public void DisableZeroGravity()
    {
        if (currentState == ObjectState.ZeroGravity)
        {
            currentState = ObjectState.Default;
            Debug.Log("ZeroGravity desactivada manualmente.");
        }
    }

    // ---------------------------------------
    //           ACTIVAR MAX GRAVITY
    // ---------------------------------------
    public void ActivateMaxGravity()
    {
        if (!canActivateMaxGravity)
            return;

        // NUEVO: si está en ZeroGravity, se apaga
        if (currentState == ObjectState.ZeroGravity)
        {
            Debug.Log("ZeroGravity cancelada por MaxGravity.");
            currentState = ObjectState.Default;
        }

        if (currentState == ObjectState.MaxGravity)
            return;

        if (maxGravityCoroutine != null)
            StopCoroutine(maxGravityCoroutine);

        maxGravityCoroutine = StartCoroutine(MaxGravityTimer());
    }

    private System.Collections.IEnumerator MaxGravityTimer()
    {
        Debug.Log("MaxGravity Activada");
        currentState = ObjectState.MaxGravity;

        yield return new WaitForSeconds(8f);

        currentState = ObjectState.Default;
        Debug.Log("MaxGravity finalizada, regresando a Default.");
    }
}
