using UnityEngine;
using System.Collections;

public class DialogueTriggerZone : MonoBehaviour
{
    [Header("Tiempo para hacer fade a TimeScale=0")]
    public float fadeDuration = 0.5f;

    [Header("Efecto de Cámara")]
    public Camera targetCamera;
    public float fovChangeAmount = 10f; // cuánto se reduce para hacer zoom-in

    [Tooltip("Curva para controlar cómo se anima el FOV")]
    public AnimationCurve fovCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float originalFOV;

    [Header("Referencias externas")]
    public GravityControl gravityControl;
    public CameraShootRaycast cameraShoot;

    private bool alreadyTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered) return;
        if (!other.CompareTag("Player")) return;

        alreadyTriggered = true;

        if (targetCamera != null)
            originalFOV = targetCamera.fieldOfView;

        DisableGameplay();

        StartCoroutine(FadeSequence());
    }

    private void DisableGameplay()
    {
        if (gravityControl != null)
        {
            gravityControl.allowRotateQ = false;
            gravityControl.allowRotateE = false;
        }

        if (cameraShoot != null)
        {
            cameraShoot.shootingEnabled = false;
        }
    }

    private IEnumerator FadeSequence()
    {
        float startTimeScale = Time.timeScale;
        float t = 0f;

        float startFOV = (targetCamera != null) ? targetCamera.fieldOfView : 0f;
        float targetFOV = originalFOV - fovChangeAmount; // zoom-in

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / fadeDuration;

            // ------- Fade timescale (lineal) -------
            Time.timeScale = Mathf.Lerp(startTimeScale, 0f, lerp);

            // ------- Fade FOV usando curva -------
            if (targetCamera != null)
            {
                float curved = fovCurve.Evaluate(lerp); // curva controla el "ritmo"
                targetCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, curved);
            }

            yield return null;
        }

        Time.timeScale = 0f;

        if (targetCamera != null)
            targetCamera.fieldOfView = targetFOV;
    }
}
