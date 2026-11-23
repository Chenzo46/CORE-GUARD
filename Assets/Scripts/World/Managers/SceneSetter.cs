using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSetter : MonoBehaviour
{
    [SerializeField] private PlayerBase plr;
    [SerializeField] private Transform plrSpawn;

    [SerializeField] private Transform elevatorDoorLeft, elevatorDoorRight;

    public static SceneSetter Singleton;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        StartCoroutine(ElevatorScene());
    }

    private IEnumerator ElevatorScene()
    {
        plr.transform.position = plrSpawn.position;
        plr.transform.rotation = plrSpawn.rotation;
        plr.SetupCamera();
        GUIManager.Singleton.FadeSceneIn(3f);
        plr.ToggleInput(false);

        yield return null;
        
        plr.ShakeCamera(3f, 0.1f, false);

        AudioManager.Singleton.PlaySound("elevator-stop", 0.3f);

        yield return new WaitForSeconds(3f);

        AudioManager.Singleton.PlaySound("elevator-ding");

        plr.ToggleInput(true);

        elevatorDoorLeft.DOLocalMoveZ(0.8f, 1f).SetEase(Ease.InOutQuart);
        elevatorDoorRight.DOLocalMoveZ(-0.8f, 1f).SetEase(Ease.InOutQuart);

        // fade screen from white
        // Persistent Camera Shake
        // Elevator music + elevator moving sound
        // Wait 2 seconds
        // Elevator stop sound
        // Elevator door opens

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

    public void StartSceneTimer()
    {
        GameStateManager.Singleton.StartTimer();
    }
}
