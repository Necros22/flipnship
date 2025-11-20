using UnityEngine;

public class Z1PushDoors : MonoBehaviour
{
    public enum PuertaLado { Izquierda, Derecha }

    [Header("Tipo de puerta")]
    public PuertaLado puertaLado = PuertaLado.Izquierda;

    [Header("Movimiento")]
    public float distanciaPorUso = 0.2f;   // cuánto se mueve por llamada
    public float velocidad = 3f;          // qué tan rápido se mueve

    [Header("Límite de usos")]
    public int usosMaximos = 5;
    private int usosActuales = 0;

    private bool moviendo = false;
    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.localPosition;
    }

    /// <summary>
    /// Llama esta función desde otro script para mover la puerta.
    /// </summary>
    public void ActivarPuerta()
    {
        if (usosActuales >= usosMaximos)
        {
            Debug.Log("La puerta ya no se puede activar más.");
            return;
        }

        if (!moviendo)
        {
            usosActuales++;
            StartCoroutine(MoverPuerta());
        }
    }

    private System.Collections.IEnumerator MoverPuerta()
    {
        moviendo = true;

        Vector3 inicio = transform.localPosition;

        float direccion = (puertaLado == PuertaLado.Izquierda) ? -1f : 1f;
        Vector3 destino = inicio + new Vector3(distanciaPorUso * direccion, 0, 0);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidad;
            transform.localPosition = Vector3.Lerp(inicio, destino, t);
            yield return null;
        }

        transform.localPosition = destino;
        moviendo = false;
    }
}
