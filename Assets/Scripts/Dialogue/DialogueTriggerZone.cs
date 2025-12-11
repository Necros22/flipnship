using UnityEngine;
using System.Collections;
using TMPro;

public class DialogueTriggerZone : MonoBehaviour
{
    [Header("Tiempo para fade")]
    public float fadeDuration = 0.5f;

    [Header("Efecto de Cámara")]
    public Camera targetCamera;
    public float fovChangeAmount = 10f;
    public AnimationCurve fovCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private float originalFOV;

    [Header("Referencias externas")]
    public GravityControl gravityControl;
    public CameraShootRaycast cameraShoot;

    [Header("Intro")]
    public GameObject modelToActivate;
    private Animator modelAnimator;

    [Header("Movimiento del modelo entre puntos")]
    public Transform[] waypoints;
    public float travelDuration = 3f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool drawCurve = true;

    [Header("Panel y diálogos")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueTMP;

    [TextArea]
    public string[] dialogueLines;

    [Header("Velocidad de escritura")]
    public float typewriterSpeed = 0.03f;
    public float postLineDelay = 1.0f;

    private bool alreadyTriggered = false;

    [Header("Modificar parámetros externos al finalizar")]
    public bool modifyExternalParameters = false;

    [Header("GravityControl Ajustes")]
    public bool set_allowRotateQ = true;
    public bool set_allowRotateE = true;

    [Header("CameraShootRaycast Ajustes")]
    public bool set_shootingEnabled = true;

    public bool set_unlockAntiGravity = true;
    public bool set_unlockZeroGravity = true;
    public bool set_unlockMaxGravity = true;

    private void ApplyExternalParameterChanges()
    {
        if (!modifyExternalParameters) return;

        if (gravityControl != null)
        {
            gravityControl.allowRotateQ = set_allowRotateQ;
            gravityControl.allowRotateE = set_allowRotateE;
        }

        if (cameraShoot != null)
        {
            cameraShoot.shootingEnabled = set_shootingEnabled;

            cameraShoot.unlockAntiGravity = set_unlockAntiGravity;
            cameraShoot.unlockZeroGravity = set_unlockZeroGravity;
            cameraShoot.unlockMaxGravity = set_unlockMaxGravity;
        }
    }

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
            cameraShoot.shootingEnabled = false;
    }

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

        if (modelToActivate != null)
        {
            modelToActivate.SetActive(true);

            modelAnimator = modelToActivate.GetComponentInChildren<Animator>();
            if (modelAnimator != null)
                modelAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        if (waypoints != null && waypoints.Length >= 2)
            StartCoroutine(MoveOnCurve());
    }

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

        StartCoroutine(StartDialogueSequence());
    }

    private IEnumerator StartDialogueSequence()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueTMP == null)
            yield break;

        for (int i = 0; i < dialogueLines.Length; i++)
        {
            yield return StartCoroutine(Typewriter(dialogueLines[i]));
            yield return new WaitForSecondsRealtime(postLineDelay);
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        StartCoroutine(ReturnMovement());
    }

    private IEnumerator Typewriter(string text)
    {
        dialogueTMP.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            dialogueTMP.text += text[i];
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }
    }

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

    private void RestoreEverything()
    {
        Time.timeScale = 1f;

        if (gravityControl != null)
        {
            gravityControl.allowRotateQ = true;
            gravityControl.allowRotateE = true;
        }

        if (cameraShoot != null)
            cameraShoot.shootingEnabled = true;

        if (targetCamera != null)
            StartCoroutine(SmoothReturnFOV());

        ApplyExternalParameterChanges();
    }

    private IEnumerator SmoothReturnFOV()
    {
        float duration = fadeDuration;
        float t = 0f;

        float startFOV = targetCamera.fieldOfView;
        float endFOV = originalFOV;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / duration;

            float curved = fovCurve.Evaluate(lerp);
            targetCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, curved);

            yield return null;
        }

        targetCamera.fieldOfView = endFOV;
    }

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
