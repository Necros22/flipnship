using UnityEngine;

public class SawController : MonoBehaviour
{
    [Header("Configuración")]
    public bool ignoreParalysis = false;

    [Header("Referencias")]
    public Animator sawAnimator;
    public GameObject activeCollider;
    public GameObject offCollider;

    [Header("Estado actual")]
    public bool isParalyzed = false;

    void Start()
    {
        SetActiveState(true);
    }

    public void Paralyze()
    {
        if (ignoreParalysis) return;
        isParalyzed = true;
        SetActiveState(false);
    }

    public void UnParalyze()
    {
        if (!isParalyzed) return;
        isParalyzed = false;
        SetActiveState(true);
    }

    private void SetActiveState(bool active)
    {
        activeCollider.SetActive(active);
        offCollider.SetActive(!active);

        if (sawAnimator != null)
        {
            sawAnimator.SetBool("activeSaw", active);
        }
    }
}
