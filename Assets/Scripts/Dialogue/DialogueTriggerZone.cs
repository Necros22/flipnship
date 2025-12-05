using UnityEngine;
using System.Collections;

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

    [Tooltip("Curva para controlar cómo se anima el FOV")]
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
    //               INTRO MOVEMENT THROUGH BEZIER POINTS
    // =========================================================
    [Header("Movimiento del modelo entre puntos (Bezier)")]
    public Transform[] waypoints;      // arrastra puntos en orden
    public float travelDuration = 3f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("Si activas esto verás la curva en el editor")]
    public bool drawCurve = true;

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
    //                    DISABLE GAMEPLAY
    // =========================================================
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



    // =========================================================
    //                    FOV + TIMESCALE FADE
    // =========================================================
    private IEnumerator FadeSequence()
    {
        float startTimeScale = Time.timeScale;
        float t = 0f;

        float startFOV = targetCamera.fieldOfView;
        float targetFOV = originalFOV - fovChangeAmount;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / fadeDuration;

            Time.timeScale = Mathf.Lerp(startTimeScale, 0f, lerp);

            float curved = fovCurve.Evaluate(lerp);
            targetCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, curved);

            yield return null;
        }

        Time.timeScale = 0f;
        targetCamera.fieldOfView = targetFOV;

        // activar modelo
        if (modelToActivate != null)
        {
            modelToActivate.SetActive(true);

            // hacer que ignore TimeScale
            modelAnimator = modelToActivate.GetComponentInChildren<Animator>();
            if (modelAnimator != null)
                modelAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        // iniciar movimiento
        if (waypoints.Length >= 2)
            StartCoroutine(MoveOnCurve());
    }



    // =========================================================
    //                     BEZIER MOVEMENT
    // =========================================================
    private IEnumerator MoveOnCurve()
    {
        float t = 0f;

        while (t < travelDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = movementCurve.Evaluate(t / travelDuration);

            // posición y rotación
            Vector3 pos = GetBezierPosition(lerp);
            Quaternion rot = GetBezierRotation(lerp);

            modelToActivate.transform.position = pos;
            modelToActivate.transform.rotation = rot;

            yield return null;
        }

        Debug.Log("✔ Movimiento Bezier finalizado");
    }



    // =========================================================
    //                BEZIER POSITION & ROTATION
    // =========================================================
    private Vector3 GetBezierPosition(float t)
    {
        if (waypoints.Length == 2)
            return Vector3.Lerp(waypoints[0].position, waypoints[1].position, t);

        int last = waypoints.Length - 1;
        float scaled = t * last;

        int idx = Mathf.FloorToInt(scaled);
        idx = Mathf.Clamp(idx, 0, last - 1);

        float lt = scaled - idx;

        // puntos para curva Bezier cúbica
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
        Vector3 pos = GetBezierPosition(t);
        Vector3 next = GetBezierPosition(Mathf.Min(t + 0.01f, 1f));

        Vector3 dir = (next - pos).normalized;

        return dir == Vector3.zero ? modelToActivate.transform.rotation : Quaternion.LookRotation(dir);
    }



    // =========================================================
    //               DRAW CURVE IN SCENE VIEW
    // =========================================================
    private void OnDrawGizmos()
    {
        if (!drawCurve || waypoints == null || waypoints.Length < 2)
            return;

        Gizmos.color = Color.cyan;

        Vector3 prev = waypoints[0].position;

        for (int i = 1; i <= 40; i++)
        {
            float t = i / 40f;
            Vector3 p = GetBezierPosition(t);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }
}
