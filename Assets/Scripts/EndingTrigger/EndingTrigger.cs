using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class EndingTrigger : MonoBehaviour
{
    [Header("Escena a cargar")]
    public string sceneToLoad;

    [Header("Fade a negro antes del cambio")]
    public bool useFade = false;
    public float fadeDuration = 1.5f;
    public Image blackPanel;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (useFade)
        {
            StartCoroutine(FadeAndLoad());
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private IEnumerator FadeAndLoad()
    {
        Color c = blackPanel.color;
        c.a = 0f;
        blackPanel.color = c;

        blackPanel.gameObject.SetActive(true);

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = t / fadeDuration;

            c.a = alpha;
            blackPanel.color = c;

            yield return null;
        }

        c.a = 1f;
        blackPanel.color = c;

        SceneManager.LoadScene(sceneToLoad);
    }
}
