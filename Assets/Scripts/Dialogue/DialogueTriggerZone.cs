using UnityEngine;
using System.Collections;
using TMPro;

public class DialogueTriggerZone : MonoBehaviour
{
    // =========================================================
    //                 TIME SCALE + CAMERA FOV FADE
    // =========================================================
    [Header("Tiempo para hacer fade a TimeScale=0")]
    public float fadeDuration = 0.5f;

    [Header("Efecto de Cámara")]
    public Camera targetCamera;
    public float fovChangeAmount = 10f;
    public AnimationCurve fovCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private float originalFOV;

    // =========================================================
    //                 GAMEPLAY SYSTEMS TO DISABLE
    // =========================================================
    [Header("Referencias externas")]
    public GravityControl gravityControl;
    public CameraShootRaycast cameraShoot;

    // =========================================================
    //                 INTRO MODEL APPEAR
    // =========================================================
    [Header("Intro – Modelo que aparecerá")]
    public GameObject modelToActivate;
    private Animator modelAnimator;

    // =========================================================
    //               MOVIMIENTO BEZIER
    // =========================================================
    [Header("Movimiento del modelo entre puntos (Bezier)")]
    public Transform[] waypoints;
    public float travelDuration = 3f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool drawCurve = true;

    // =========================================================
    //                    DIALOG SYSTEM
    // =========================================================
    [Header("Panel y diálogos")]
    public GameObject dialoguePanel;       // Panel a activar (arrastrar)
    public TMP_Text dialogueTMP;           // TextMeshPro dentro del panel (arrastrar)

    [TextArea]
    public string[] dialogueLines;         // Array de diálogos (arrastrar/llenar)

    [Header("Typewriter / Timings")]
    public float typewriterSpeed = 0.03f;  // tiempo entre caracteres (segundos, real time)
    public float postLineDelay = 1.0f;     // <-- TIEMPO que se espera después de terminar cada línea (inspector)

    private bool alreadyTriggered = false;

    // =========================================================
    //                    TRIGGER ENTER
    // =========================================================
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

    // =========================================================
    //                  DISABLE GAMEPLAY
    // =========================================================
    private void DisableGameplay()
    {
        if (gravityControl != null)
        {
            gravityControl.allowRotateQ = false;
            gravityControl.allowRotateE = false;
        }

        if (cameraShoot != null)
            cameraShoot.shootingEnabled = false;
    }

    // =========================================================
    //                  FOV + TIMESCALE FADE
    // =========================================================
    private IEnumerator FadeSequence()
    {
        float startTS = Time.timeScale;
        float t = 0f;

        float startFOV = targetCamera != null ? targetCamera.fieldOfView : 60f;
        float targetFOV = originalFOV - fovChangeAmount;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / fadeDuration;

            Time.timeScale = Mathf.Lerp(startTS, 0f, lerp);

            float curved = fovCurve.Evaluate(lerp);
            if (targetCamera != null)
                targetCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, curved);

            yield return null;
        }

        Time.timeScale = 0f;
        if (targetCamera != null)
            targetCamera.fieldOfView = targetFOV;

        // activar modelo
        if (modelToActivate != null)
        {
            modelToActivate.SetActive(true);

            modelAnimator = modelToActivate.GetComponentInChildren<Animator>();
            if (modelAnimator != null)
                modelAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        // iniciar movimiento
        if (waypoints != null && waypoints.Length >= 2)
            StartCoroutine(MoveOnCurve());
    }

    // =========================================================
    //                 MOVIMIENTO BEZIER (IDEM)
    // =========================================================
    private IEnumerator MoveOnCurve()
    {
        float t = 0f;

        while (t < travelDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = movementCurve.Evaluate(t / travelDuration);

            Vector3 pos = GetBezierPosition(lerp);
            Quaternion rot = GetBezierRotation(lerp);

            if (modelToActivate != null)
            {
                modelToActivate.transform.position = pos;
                modelToActivate.transform.rotation = rot;
            }

            yield return null;
        }

        // Cuando termina el movimiento → INICIA DIÁLOGOS
        StartCoroutine(StartDialogueSequence());
    }

    // =========================================================
    //                 DIALOG SEQUENCE
    // =========================================================
    private IEnumerator StartDialogueSequence()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueTMP == null)
        {
            Debug.LogWarning("Dialogue TMP not assigned.");
            yield break;
        }

        for (int i = 0; i < dialogueLines.Length; i++)
        {
            // Typewriter: escribe texto
            yield return StartCoroutine(Typewriter(dialogueLines[i]));

            // Espera el tiempo configurado en el inspector después de terminar la línea
            yield return new WaitForSecondsRealtime(postLineDelay);
        }

        // Terminar diálogos
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Iniciar recorrido invertido
        StartCoroutine(ReturnMovement());
    }

    // =========================================================
    //                 TYPEWRITER EFFECT
    // =========================================================
    private IEnumerator Typewriter(string text)
    {
        dialogueTMP.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            dialogueTMP.text += text[i];

            // espera entre caracteres (tiempo real, no afectado por timescale)
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }
        // Aquí justo termina la escritura de la última tecla -> empieza el postLineDelay en StartDialogueSequence
    }

    // =========================================================
    //                 MOVIMIENTO REVERSE
    // =========================================================
    private IEnumerator ReturnMovement()
    {
        float t = 0f;

        while (t < travelDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = movementCurve.Evaluate(t / travelDuration);

            float back = 1f - lerp;

            Vector3 pos = GetBezierPosition(back);
            Quaternion rot = GetBezierRotation(back);

            if (modelToActivate != null)
            {
                modelToActivate.transform.position = pos;
                modelToActivate.transform.rotation = rot;
            }

            yield return null;
        }

        if (modelToActivate != null)
            modelToActivate.SetActive(false);

        RestoreEverything();
    }

    // =========================================================
    //                     RESTORE SYSTEM
    // =========================================================
    private void RestoreEverything()
    {
        // restaurar tiempo
        Time.timeScale = 1f;

        // restaurar gameplay
        if (gravityControl != null)
        {
            gravityControl.allowRotateQ = true;
            gravityControl.allowRotateE = true;
        }

        if (cameraShoot != null)
            cameraShoot.shootingEnabled = true;

        // Ahora hacer el FOV smooth, no instantáneo:
        if (targetCamera != null)
            StartCoroutine(SmoothReturnFOV());
    }

    private IEnumerator SmoothReturnFOV()
    {
        float duration = fadeDuration;        // usa el mismo tiempo o puedes hacer otro si quieres
        float t = 0f;

        float startFOV = targetCamera.fieldOfView;
        float endFOV = originalFOV;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;          // usa tiempo real
            float lerp = t / duration;

            float curved = fovCurve.Evaluate(lerp);   // usa la misma curva de entrada
            targetCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, curved);

            yield return null;
        }

        targetCamera.fieldOfView = endFOV; // asegurar valor final exacto
    }


    // =========================================================
    //                BEZIER CALCULATION
    // =========================================================
    private Vector3 GetBezierPosition(float t)
    {
        if (waypoints == null || waypoints.Length == 0) return Vector3.zero;

        if (waypoints.Length == 2)
            return Vector3.Lerp(waypoints[0].position, waypoints[1].position, t);

        int last = waypoints.Length - 1;
        float scaled = t * last;

        int idx = Mathf.FloorToInt(scaled);
        idx = Mathf.Clamp(idx, 0, last - 1);

        float lt = scaled - idx;

        Vector3 p0 = waypoints[idx].position;
        Vector3 p1 = waypoints[idx].position + waypoints[idx].forward * 2f;
        Vector3 p2 = waypoints[idx + 1].position - waypoints[idx + 1].forward * 2f;
        Vector3 p3 = waypoints[idx + 1].position;

        return Mathf.Pow(1 - lt, 3) * p0 +
               3 * Mathf.Pow(1 - lt, 2) * lt * p1 +
               3 * (1 - lt) * Mathf.Pow(lt, 2) * p2 +
               Mathf.Pow(lt, 3) * p3;
    }

    private Quaternion GetBezierRotation(float t)
    {
        // Si estamos al final exacto del recorrido → usar forward del último waypoint
        if (t >= 0.999f)
        {
            Transform last = waypoints[waypoints.Length - 1];
            return last.rotation;
        }

        Vector3 pos = GetBezierPosition(t);
        Vector3 next = GetBezierPosition(t + 0.01f);

        Vector3 dir = (next - pos).normalized;

        return dir == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(dir);
    }

    // =========================================================
    //                 DRAW CURVE IN EDITOR
    // =========================================================
    private void OnDrawGizmos()
    {
        if (!drawCurve || waypoints == null || waypoints.Length < 2)
            return;

        Gizmos.color = Color.cyan;

        Vector3 prev = GetBezierPosition(0f);

        int segments = 40;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 p = GetBezierPosition(t);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }
}
