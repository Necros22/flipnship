using UnityEngine;
using System.Collections;

public class GravityControl : MonoBehaviour
{
    [Header("Referencia al objeto que contiene los props con CustomDirectionalGravity")]
    public GameObject gravityAnchor;

    [Header("Referencia a la cámara que debe rotar")]
    public Transform cameraHolder;
    public Transform dialogueMovementPoints;
    public Transform flipModel; // extraObjectToMoveForward

    [Header("Referencia a la nave (MovementShip)")]
    public MovementShip shipMovement;

    [Header("Permisos de cambio de gravedad")]
    public bool allowRotateQ = true;   // habilita/deshabilita la rotación con Q
    public bool allowRotateE = true;   // habilita/deshabilita la rotación con E

    private float rotationStep = 90f;
    private bool isRotating = false;


    [HideInInspector] public int currentCameraRotation = 0;

    void Update()
    {
        if (gravityAnchor == null || cameraHolder == null) return;

        // ------------ ROTACIÓN CON Q ------------
        if (Input.GetKeyDown(KeyCode.Q) && !isRotating && allowRotateQ)
        {
            RotateGravity(-1);
            RotateCamera(+rotationStep);
            UpdateRotationIndex(+1);
        }

        // ------------ ROTACIÓN CON E ------------
        if (Input.GetKeyDown(KeyCode.E) && !isRotating && allowRotateE)
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

        Time.timeScale = 0f;

        float duration = 0.35f;
        float t = 0f;

        Quaternion startCam = cameraHolder.rotation;
        Quaternion endCam = cameraHolder.rotation * Quaternion.Euler(0, 0, angle);

        Quaternion startDialogue = dialogueMovementPoints != null ? dialogueMovementPoints.rotation : Quaternion.identity;
        Quaternion endDialogue = dialogueMovementPoints != null ? startDialogue * Quaternion.Euler(0, 0, angle) : Quaternion.identity;

        Quaternion startFlip = flipModel != null ? flipModel.rotation : Quaternion.identity;
        Quaternion endFlip = flipModel != null ? startFlip * Quaternion.Euler(0, 0, angle) : Quaternion.identity;

        while (t < duration)
        {
            float lerp = t / duration;

            cameraHolder.rotation = Quaternion.Lerp(startCam, endCam, lerp);

            if (dialogueMovementPoints != null)
                dialogueMovementPoints.rotation = Quaternion.Lerp(startDialogue, endDialogue, lerp);

            if (flipModel != null)
                flipModel.rotation = Quaternion.Lerp(startFlip, endFlip, lerp);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        cameraHolder.rotation = endCam;

        if (dialogueMovementPoints != null)
            dialogueMovementPoints.rotation = endDialogue;

        if (flipModel != null)
            flipModel.rotation = endFlip;

        Time.timeScale = 1f;

        isRotating = false;
    }


    private void RotateCamera(float angle)
    {
        StartCoroutine(SmoothRotate(angle));
    }
}
