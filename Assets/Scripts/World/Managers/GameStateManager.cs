using System;
using System.Collections;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Singleton;

    [SerializeField] private float timer = 20f;
    [SerializeField] private bool startTimerOnAwake = false;

    private bool timerStarted = false;

    private float initalTimer = 0f;

    public Action OnTimerDepleted;

    private Coroutine TimerRoutineRef;

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            // for DontDestroyOnLoad
        }
    }

    private void Start()
    {
        initalTimer = timer;

        if(startTimerOnAwake)
            StartTimer();
    }

    public void StartTimer()
    {
        if (timerStarted)
            return;
        
        TimerRoutineRef = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        timerStarted = true;
        timer = initalTimer;

        while (true)
        {
            timer -= Time.deltaTime;

            GUIManager.Singleton.SetTimer(timer);

            if (timer <= 0)
                break;

            yield return null;
        }

        OnTimerDepleted?.Invoke();

        timer = 0f;
        GUIManager.Singleton.SetTimer(timer);

        // Lose Condition goes here
    }

    public void StopTimer()
    {
        if(!timerStarted)
            return;

        Debug.Log("Timer Stopped");

        timerStarted = false;
        GUIManager.Singleton.SetTimer(timer);

        StopCoroutine(TimerRoutineRef);
    }

    public float GetCurrentTimer()
    {
        return timer;
    }
}
