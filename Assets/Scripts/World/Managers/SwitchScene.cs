using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    [SerializeField] private CanvasGroup mainCG, fadeCG;
    [SerializeField] private AudioSource music;

    private void Awake()
    {
        fadeCG.alpha = 1;
        fadeCG.blocksRaycasts = true;
        GUIManager.FadeCanvasGroup(fadeCG, 0f, 2f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


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
        music.DOFade(0f, 3f);

        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene("COOLANT_LVL_1");
    }

    public void BackToMain()
    {
        StartCoroutine(ToMain());
    }

    private IEnumerator ToMain()
    {
        DOTween.To(x => fadeCG.alpha = x, 0f, 1f, 3f);
        mainCG.interactable = false;
        music.DOFade(0f, 3f);

        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene("Main Menu");
    }
}
