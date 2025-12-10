using UnityEngine;
using UnityEngine.UI;

public class CameraShootRaycast : MonoBehaviour
{
    public enum ShotType
    {
        AntiGravity = 1,
        ZeroGravity = 2,
        MaxGravity = 3
    }

    [Header("Referencias")]
    public Transform pivotCamera;

    [Header("Distancias")]
    public float maxRayDistance = 3000f;
    public float maxDynamicDistance = 100f;

    [Header("Visualización Raycast Debug")]
    public float clickRayDuration = 30f;
    public Color idleRayColor = Color.yellow;
    public Color clickRayColor = Color.cyan;

    [Header("Audio")]
    public AudioSource shootSFX;

    [Header("Control General de Disparos")]
    public bool shootingEnabled = true;

    [Header("Desbloqueo de Tipos de Disparo")]
    public bool unlockAntiGravity = true;
    public bool unlockZeroGravity = true;
    public bool unlockMaxGravity = true;

    [Header("Tipo de Disparo Actual")]
    public ShotType currentShotType = ShotType.AntiGravity;

    [Header("UI – Iconos del HUD de disparos")]
    public ShotUI antiGravityUI;
    public ShotUI zeroGravityUI;
    public ShotUI maxGravityUI;

    [System.Serializable]
    public class ShotUI
    {
        public Image imageIcon;
        public Sprite lockedSprite;
        public Sprite enabledSprite;
        public Sprite selectedSprite;
    }

    [Header("Láser Visual")]
    public GameObject laserPrefab;
    public Material antiGravityMat;
    public Material zeroGravityMat;
    public Material maxGravityMat;
    public float laserDuration = 1f;

    private GameObject activeLaser;
    private Camera cam;
    private float currentRayDistance;
    private int ignorePlayerMask;

    private CustomDirectionalGravity lastZeroGravityTarget;
    private SawController lastSawParalyzed;

    void Start()
    {
        cam = Camera.main;
        ignorePlayerMask = ~LayerMask.GetMask("Player");
    }

    void Update()
    {
        if (cam == null || pivotCamera == null)
            return;

        HandleShotSelection();
        UpdateAimRay();

        if (Input.GetMouseButtonDown(0))
        {
            if (IsCurrentShotUnlocked())
            {
                PlayShotSFX();
                FireRay();
            }
        }

        UpdateShotUI();
    }

    void PlayShotSFX()
    {
        if (shootSFX != null)
            shootSFX.Play();
    }

    bool IsCurrentShotUnlocked()
    {
        return (currentShotType == ShotType.AntiGravity && unlockAntiGravity) ||
               (currentShotType == ShotType.ZeroGravity && unlockZeroGravity) ||
               (currentShotType == ShotType.MaxGravity && unlockMaxGravity);
    }

    void UpdateShotUI()
    {
        UpdateSingleUI(antiGravityUI, unlockAntiGravity, currentShotType == ShotType.AntiGravity);
        UpdateSingleUI(zeroGravityUI, unlockZeroGravity, currentShotType == ShotType.ZeroGravity);
        UpdateSingleUI(maxGravityUI, unlockMaxGravity, currentShotType == ShotType.MaxGravity);
    }

    void UpdateSingleUI(ShotUI ui, bool unlocked, bool selected)
    {
        if (ui.imageIcon == null) return;
        if (!unlocked) { ui.imageIcon.sprite = ui.lockedSprite; return; }
        if (selected) { ui.imageIcon.sprite = ui.selectedSprite; return; }
        ui.imageIcon.sprite = ui.enabledSprite;
    }

    void HandleShotSelection()
    {
        if (!shootingEnabled) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && unlockAntiGravity)
            currentShotType = ShotType.AntiGravity;

        if (Input.GetKeyDown(KeyCode.Alpha2) && unlockZeroGravity)
            currentShotType = ShotType.ZeroGravity;

        if (Input.GetKeyDown(KeyCode.Alpha3) && unlockMaxGravity)
            currentShotType = ShotType.MaxGravity;
    }

    void UpdateAimRay()
    {
        Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);

        Vector3 targetPoint;
        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxRayDistance, ignorePlayerMask))
            targetPoint = cameraHit.point;
        else
            targetPoint = cameraRay.GetPoint(maxRayDistance);

        Vector3 origin = pivotCamera.position;
        Vector3 direction = (targetPoint - origin).normalized;

        bool hitSomething = RaycastIgnoringButtons(origin, direction, out RaycastHit hit, maxDynamicDistance);

        if (hitSomething)
        {
            currentRayDistance = hit.distance;

            if (hit.collider.CompareTag("GravityAffected"))
                Debug.DrawRay(origin, direction * currentRayDistance, Color.green);
            else
                Debug.DrawRay(origin, direction * currentRayDistance, idleRayColor);
        }
        else
        {
            currentRayDistance = maxDynamicDistance;
            Debug.DrawRay(origin, direction * currentRayDistance, idleRayColor);
        }
    }

    bool RaycastIgnoringButtons(Vector3 origin, Vector3 direction, out RaycastHit validHit, float maxDist)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDist, ignorePlayerMask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit h in hits)
        {
            if (h.collider.CompareTag("Button"))
                continue;

            validHit = h;
            return true;
        }

        validHit = default;
        return false;
    }

    void FireRay()
    {
        Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);

        Vector3 targetPoint;
        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxRayDistance, ignorePlayerMask))
            targetPoint = cameraHit.point;
        else
            targetPoint = cameraRay.GetPoint(maxRayDistance);

        Vector3 origin = pivotCamera.position;
        Vector3 direction = (targetPoint - origin).normalized;

        bool hitSomething = RaycastIgnoringButtons(origin, direction, out RaycastHit hit, maxDynamicDistance);

        float rayLength = maxDynamicDistance;

        if (!hitSomething)
        {
            Debug.DrawRay(origin, direction * rayLength, clickRayColor, clickRayDuration);
            CreateLaser(origin, direction, rayLength);
            return;
        }

        rayLength = hit.distance;
        Debug.DrawRay(origin, direction * rayLength, clickRayColor, clickRayDuration);
        CreateLaser(origin, direction, rayLength);

        SawController saw = hit.collider.GetComponentInParent<SawController>();
        if (saw != null)
        {
            if (currentShotType == ShotType.ZeroGravity)
            {
                if (lastZeroGravityTarget != null)
                {
                    lastZeroGravityTarget.DisableZeroGravity();
                    lastZeroGravityTarget = null;
                }

                if (lastSawParalyzed != null && lastSawParalyzed != saw)
                    lastSawParalyzed.UnParalyze();

                saw.Paralyze();
                lastSawParalyzed = saw;
            }
            else
            {
                saw.UnParalyze();
            }
            return;
        }

        if (!hit.collider.CompareTag("GravityAffected"))
            return;

        CustomDirectionalGravity gravityScript = hit.collider.GetComponent<CustomDirectionalGravity>();
        if (gravityScript == null)
            return;

        switch (currentShotType)
        {
            case ShotType.AntiGravity:
                gravityScript.AntiGravityChange();
                break;

            case ShotType.ZeroGravity:
                if (lastSawParalyzed != null)
                {
                    lastSawParalyzed.UnParalyze();
                    lastSawParalyzed = null;
                }

                if (lastZeroGravityTarget != null && lastZeroGravityTarget != gravityScript)
                    lastZeroGravityTarget.DisableZeroGravity();

                gravityScript.ActivateZeroGravity();
                lastZeroGravityTarget = gravityScript;
                break;

            case ShotType.MaxGravity:
                gravityScript.ActivateMaxGravity();
                break;
        }
    }

    void CreateLaser(Vector3 origin, Vector3 direction, float length)
    {
        if (activeLaser != null)
            Destroy(activeLaser);

        // Crear láser
        activeLaser = Instantiate(laserPrefab, origin, Quaternion.LookRotation(direction));

        // Estirar en Z
        Vector3 scale = activeLaser.transform.localScale;
        scale.z = length;
        activeLaser.transform.localScale = scale;

        // 🔥 MOVERLO HACIA ADELANTE LA MITAD DEL LARGO
        activeLaser.transform.position += direction * (length * 0.5f);

        MeshRenderer r = activeLaser.GetComponent<MeshRenderer>();

        switch (currentShotType)
        {
            case ShotType.AntiGravity:
                r.material = antiGravityMat;
                break;

            case ShotType.ZeroGravity:
                r.material = zeroGravityMat;
                break;

            case ShotType.MaxGravity:
                r.material = maxGravityMat;
                break;
        }

        Destroy(activeLaser, laserDuration);
    }

}
