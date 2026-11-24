using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{

public void PlayGame()
    {
        SceneManager.LoadScene("PlayerControlSample");
    }
public void ExitGame()
    {
        Application.Quit();
    }
}
