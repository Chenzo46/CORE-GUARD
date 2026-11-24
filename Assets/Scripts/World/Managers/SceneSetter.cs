using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneSetter : MonoBehaviour
{
    [SerializeField] private PlayerBase plr;
    [SerializeField] private Transform plrSpawn;

    [SerializeField] private Transform elevatorDoorLeft, elevatorDoorRight;
    [SerializeField] private string nextLevel = "COOLANT_LVL_1";
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip endKey1, endKey2;
    [SerializeField] private UnityEvent OnPlayerReady;

    //13.33f

    private int day = 0;

    public static SceneSetter Singleton;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        StartCoroutine(ElevatorScene());
    }

    private void SetDay()
    {
        try
        {
            day = int.Parse(SceneManager.GetActiveScene().name.Split('_')[2]);
        }
        catch
        {
            day = 0;
        }


        GUIManager.Singleton.SetDayText(day);
    }

    private IEnumerator ElevatorScene()
    {
        plr.transform.position = plrSpawn.position;
        plr.transform.rotation = plrSpawn.rotation;
        plr.SetupCamera();
        GUIManager.Singleton.FadeSceneIn(3f);
        GUIManager.Singleton.ShowTaskGUI(false);
        SetDay();
        plr.ToggleInput(false);

        yield return null;
        
        plr.ShakeCamera(3f, 0.1f, false);

        AudioManager.Singleton.PlaySoundWorld("elevator-stop", plrSpawn.position, 0.3f);

        yield return new WaitForSeconds(3f);

        AudioManager.Singleton.PlaySoundWorld("elevator-ding", plrSpawn.position);

        GUIManager.Singleton.FadeDayText(1f);

        plr.ToggleInput(true);
        OnPlayerReady?.Invoke();

        elevatorDoorLeft.DOLocalMoveZ(0.8f, 1f).SetEase(Ease.InOutQuart);
        elevatorDoorRight.DOLocalMoveZ(-0.8f, 1f).SetEase(Ease.InOutQuart);

        yield return null;
    }

    public void LoseGame()
    {
        StartCoroutine(LoseScene());
    }

    private IEnumerator LoseScene()
    {

        plr.ToggleInput(false);
        
        // stop music
        // Fade screen to white
        GUIManager.Singleton.FadeSceneOut(3f);
        // Shake camera violently
        plr.ShakeCamera(3f, 0.5f, false);
        // play explosion sound
        AudioManager.Singleton.PlaySound("core-explode");
        yield return new WaitForSeconds(4.1f);

        yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void WinGame()
    {
        StartCoroutine(WinScene());
    }

    private IEnumerator WinScene()
    {
        GUIManager.Singleton.FadeSceneOut(3f);

        WinJingle();

        plr.ToggleInput(false);

        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(nextLevel);
    }

    private void WinJingle()
    {
        if(musicSource.time < 13.33f)
        {
            musicSource.Pause();
            musicSource.clip = endKey1;
        }
        else
        {
            musicSource.Pause();
            musicSource.clip = endKey2;
        }

        musicSource.Play();
    }

    public void StartSceneTimer()
    {
        GameStateManager.Singleton.StartTimer();
        if(musicSource != null)
            musicSource.Play();
        GUIManager.Singleton.ShowTaskGUI(true);
    }

    public void ForceFadeOutMusic()
    {
        musicSource.DOFade(0f, 2.9f);
    }
}
