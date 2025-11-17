using UnityEngine;

public class CustomDirectionalGravity : MonoBehaviour
{
    public enum GravityDirection
    {
        Right,
        Down,
        Left,
        Up
    }

    public enum ObjectState
    {
        Default,
        ZeroGravity,
        MaxGravity
    }

    [Header("Configuración de gravedad personalizada")]
    public GravityDirection gravityDirection = GravityDirection.Down;
    public float gravityStrength = 9.81f;
    public float maxGravityMultiplier = 100f; // Fuerza extra en MaxGravity

    [Header("Estado del objeto")]
    public ObjectState currentState = ObjectState.Default;

    private Rigidbody rb;
    private Coroutine maxGravityCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        Vector3 direction = GetGravityDirection();

        switch (currentState)
        {
            case ObjectState.Default:
                rb.AddForce(direction * gravityStrength, ForceMode.Acceleration);
                break;

            case ObjectState.ZeroGravity:
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // No aplicamos gravedad
                break;

            case ObjectState.MaxGravity:
                rb.AddForce(direction * (gravityStrength * maxGravityMultiplier), ForceMode.Acceleration);
                break;
        }
    }

    private Vector3 GetGravityDirection()
    {
        return gravityDirection switch
        {
            GravityDirection.Up => Vector3.up,
            GravityDirection.Down => Vector3.down,
            GravityDirection.Left => Vector3.left,
            GravityDirection.Right => Vector3.right,
            _ => Vector3.zero
        };
    }

    // ----------------------------
    //        FUNCIONES DE ESTADO
    // ----------------------------

    /// <summary>
    /// Activa ZeroGravity: detiene el objeto y elimina la gravedad.
    /// </summary>
    public void ActivateZeroGravity()
    {
        currentState = ObjectState.ZeroGravity;
        Debug.Log("ZeroGravity Activada");
    }

    /// <summary>
    /// Activa MaxGravity por 4 segundos. Si ya está activa, se ignora.
    /// </summary>
    public void ActivateMaxGravity()
    {
        if (currentState == ObjectState.MaxGravity)
        {
            Debug.Log("MaxGravity ya está activa, ignorado.");
            return;
        }

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

    /// <summary>
    /// Invierte la dirección de la gravedad.
    /// </summary>
    public void AntiGravityChange()
    {
        switch (gravityDirection)
        {
            case GravityDirection.Up: gravityDirection = GravityDirection.Down; break;
            case GravityDirection.Down: gravityDirection = GravityDirection.Up; break;
            case GravityDirection.Left: gravityDirection = GravityDirection.Right; break;
            case GravityDirection.Right: gravityDirection = GravityDirection.Left; break;
        }

        Debug.Log($"Gravedad invertida: ahora apunta hacia {gravityDirection}");
    }
}
