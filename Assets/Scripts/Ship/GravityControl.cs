using UnityEngine;
using System.Collections;

public class GravityControl : MonoBehaviour
{
    [Header("Referencia al objeto que contiene los props con CustomDirectionalGravity")]
    public GameObject gravityAnchor;

    [Header("Referencia a la cámara que debe rotar")]
    public Transform cameraHolder;

    [Header("Referencia a la nave (MovementShip)")]
    public MovementShip shipMovement; // drag your ship here just for passing rotationIndex

    private float rotationStep = 90f;
    private bool isRotating = false;

    // cuántas rotaciones lleva la cámara (0,1,2,3)
    [HideInInspector] public int currentCameraRotation = 0;

    void Update()
    {
        if (gravityAnchor == null || cameraHolder == null) return;

        if (Input.GetKeyDown(KeyCode.Q) && !isRotating)
        {
            RotateGravity(-1);
            RotateCamera(+rotationStep);
            UpdateRotationIndex(+1);
        }

        if (Input.GetKeyDown(KeyCode.E) && !isRotating)
        {
            RotateGravity(1);
            RotateCamera(-rotationStep);
            UpdateRotationIndex(-1);
        }
    }

    private void UpdateRotationIndex(int dir)
    {
        currentCameraRotation = (currentCameraRotation + dir) % 4;
        if (currentCameraRotation < 0) currentCameraRotation += 4;

        // pasamos la info a la nave
        if (shipMovement != null)
            shipMovement.SetRotationIndex(currentCameraRotation);
    }

    private void RotateGravity(int direction)
    {
        for (int i = 0; i < gravityAnchor.transform.childCount; i++)
        {
            Transform child = gravityAnchor.transform.GetChild(i);
            CustomDirectionalGravity gravity = child.GetComponent<CustomDirectionalGravity>();

            if (gravity != null)
            {
                // AHORA usamos la versión que respeta acceptsManualGravityChange
                int currentIndex = (int)gravity.gravityDirection;
                int total = System.Enum.GetValues(typeof(CustomDirectionalGravity.GravityDirection)).Length;

                int newIndex = (currentIndex + direction) % total;
                if (newIndex < 0) newIndex += total;

                gravity.SetGravityDirectionManual(
                    (CustomDirectionalGravity.GravityDirection)newIndex
                );
            }
        }
    }


    IEnumerator SmoothRotate(float angle)
    {
        isRotating = true;

        // ✅ Freeze everything
        Time.timeScale = 0f;

        float duration = 0.35f;
        float t = 0f;

        Quaternion startRot = cameraHolder.rotation;
        Quaternion endRot = cameraHolder.rotation * Quaternion.Euler(0, 0, angle);

        while (t < duration)
        {
            cameraHolder.rotation = Quaternion.Lerp(startRot, endRot, t / duration);
            t += Time.unscaledDeltaTime; // ✅ ignore timeScale
            yield return null;
        }

        cameraHolder.rotation = endRot;

        // ✅ Resume game
        Time.timeScale = 1f;

        isRotating = false;
    }

    private void RotateCamera(float angle)
    {
        StartCoroutine(SmoothRotate(angle));
    }
}
