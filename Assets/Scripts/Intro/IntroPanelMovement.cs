using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroPanelMovement : MonoBehaviour
{
    public RectTransform[] panels;
    public float moveSpeed = 3f;
    public float waitBetween = 3f;
    public string sceneToLoad = "LEVEL1";

    int index = 0;
    bool moving = false;

    void Start()
    {
        StartCoroutine(RunSequence());
    }

    System.Collections.IEnumerator RunSequence()
    {
        while (index < panels.Length)
        {
            RectTransform p = panels[index];
            moving = true;

            Vector3 target = Vector3.zero;
            while (Vector3.Distance(p.anchoredPosition, target) > 0.1f)
            {
                p.anchoredPosition = Vector3.Lerp(p.anchoredPosition, target, Time.deltaTime * moveSpeed);
                yield return null;
            }

            p.anchoredPosition = target;
            moving = false;

            index++;
            yield return new WaitForSeconds(waitBetween);
        }

        yield return new WaitForSeconds(waitBetween);
        SceneManager.LoadScene(sceneToLoad);
    }
}
