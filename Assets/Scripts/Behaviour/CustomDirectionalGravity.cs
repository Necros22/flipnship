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

    [Header("Configuración de gravedad personalizada")]
    public GravityDirection gravityDirection = GravityDirection.Down; // Dirección elegida desde el inspector
    public float gravityStrength = 9.81f; // Intensidad de la gravedad

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Desactiva la gravedad global
    }

    void FixedUpdate()
    {
        Vector3 direction = Vector3.zero;

        // Según la dirección elegida, aplicamos la fuerza correspondiente
        switch (gravityDirection)
        {
            case GravityDirection.Up:
                direction = Vector3.up;
                break;
            case GravityDirection.Down:
                direction = Vector3.down;
                break;
            case GravityDirection.Left:
                direction = Vector3.left;
                break;
            case GravityDirection.Right:
                direction = Vector3.right;
                break;
        }

        rb.AddForce(direction * gravityStrength, ForceMode.Acceleration);
    }

    /// <summary>
    /// Invierte la dirección actual de la gravedad.
    /// </summary>
    public void AntiGravityChange()
    {
        switch (gravityDirection)
        {
            case GravityDirection.Up:
                gravityDirection = GravityDirection.Down;
                break;
            case GravityDirection.Down:
                gravityDirection = GravityDirection.Up;
                break;
            case GravityDirection.Left:
                gravityDirection = GravityDirection.Right;
                break;
            case GravityDirection.Right:
                gravityDirection = GravityDirection.Left;
                break;
        }

        Debug.Log($"🌀 Gravedad invertida: ahora apunta hacia {gravityDirection}");
    }
}
