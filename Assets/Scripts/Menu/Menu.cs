using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    public void ExitGame()
    {
        Application.Quit();
    }

    public void GoToIntro()
    {
        SceneManager.LoadScene("LEVEL1");
    }
}
