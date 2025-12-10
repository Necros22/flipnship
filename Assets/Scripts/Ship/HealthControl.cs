using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthControl : MonoBehaviour
{
    [Header("Vida del jugador")]
    [Range(1, 5)]
    public int currentHealth = 5;

    [Header("Imágenes de vida (5 variantes)")]
    public Image healthImageUI;
    public Sprite[] healthSprites;

    [Header("Escena a recargar cuando muere")]
    public string sceneToReload;

    [Header("Configuración de daño")]
    public string[] damagingTags = { "RingDoor", "Damage", "Saw" };
    public string[] damagingNames = { "GenDoor", "ShootDoor", "IsoDoor", "Block" };

    [Header("SFX")]
    public AudioSource damageSFX;

    private bool isDead = false;

    private void Start()
    {
        UpdateHealthUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        foreach (string t in damagingTags)
        {
            if (other.CompareTag(t))
            {
                TakeDamage(other.gameObject);
                return;
            }
        }

        foreach (string n in damagingNames)
        {
            if (other.name.Contains(n))
            {
                TakeDamage(other.gameObject);
                return;
            }
        }
    }

    private void TakeDamage(GameObject damagingObject)
    {
        if (damageSFX != null)
            damageSFX.Play();

        Destroy(damagingObject);

        currentHealth--;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthImageUI == null || healthSprites.Length < 5)
            return;

        int index = Mathf.Clamp(5 - currentHealth, 0, 4);
        healthImageUI.sprite = healthSprites[index];
    }

    private void Die()
    {
        isDead = true;

        if (string.IsNullOrEmpty(sceneToReload))
            return;

        SceneManager.LoadScene(sceneToReload);
    }
}
