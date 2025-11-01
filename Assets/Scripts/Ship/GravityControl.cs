using UnityEngine;

public class GravityControl : MonoBehaviour
{
    [Header("Referencia al objeto que contiene los props con CustomDirectionalGravity")]
    public GameObject gravityAnchor; // Se asigna el objeto "GravityAnchor" desde el Inspector

    void Update()
    {
        if (gravityAnchor == null) return;

        // Girar a la izquierda (Q)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateGravity(-1);
        }

        // Girar a la derecha (E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            RotateGravity(1);
        }
    }

    private void RotateGravity(int direction)
    {
        // Recorremos todos los hijos del objeto "GravityAnchor"
        for (int i = 0; i < gravityAnchor.transform.childCount; i++)
        {
            Transform child = gravityAnchor.transform.GetChild(i);
            CustomDirectionalGravity customGravity = child.GetComponent<CustomDirectionalGravity>();

            if (customGravity != null)
            {
                int currentIndex = (int)customGravity.gravityDirection;
                int total = System.Enum.GetValues(typeof(CustomDirectionalGravity.GravityDirection)).Length;

                // Calculamos el nuevo índice con rotación circular
                int newIndex = (currentIndex + direction) % total;
                if (newIndex < 0) newIndex += total;

                // Asignamos la nueva dirección
                customGravity.gravityDirection = (CustomDirectionalGravity.GravityDirection)newIndex;

                // Mostrar en consola el cambio
                Debug.Log($"[{child.name}] nueva dirección de gravedad: {customGravity.gravityDirection}");
            }
        }
    }
}

