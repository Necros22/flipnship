using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FinalManager : MonoBehaviour
{
    public TextMeshProUGUI firstText;
    public string firstReplacement;
    public TextMeshProUGUI secondText;

    void Start()
    {
        StartCoroutine(Flow());
    }

    System.Collections.IEnumerator Flow()
    {
        yield return new WaitForSeconds(3f);

        firstText.gameObject.SetActive(true);
        firstText.text = firstReplacement;

        yield return new WaitForSeconds(2f);

        secondText.gameObject.SetActive(true);

        yield return new WaitForSeconds(15f);

        SceneManager.LoadScene("MENU");
    }
}
