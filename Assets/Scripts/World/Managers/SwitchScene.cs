using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    [SerializeField] private CanvasGroup mainCG, fadeCG;
    

    public void PlayGame()
    {
        StartCoroutine(GoToFirstLevel());
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private IEnumerator GoToFirstLevel()
    {
        DOTween.To(x => fadeCG.alpha = x, 0f, 1f, 3f);
        mainCG.interactable = false;

        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene("COOLANT_LVL_1");
    }
}
