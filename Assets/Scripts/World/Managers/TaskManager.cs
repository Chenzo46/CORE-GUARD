using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private string taskName;
    public Task currentTask;

    private void Awake()
    {
        currentTask.InitializeTask();
    }

    private void Start()
    {
        GameStateManager.Singleton.OnTimerDepleted += FailCurrentStep;
    }

    public void CompleteCurrentStep()
    {
        currentTask.currentStep.CompleteStep();
    }

    public void FailCurrentStep()
    {
        currentTask.currentStep.FailStep();
    }

    public string GetTaskName()
    {
        return taskName;
    }
}

[System.Serializable]
public class Task
{
    public List<TaskStep> steps;
    public TaskStep currentStep => steps[stepIndex];
    private int stepIndex = 0;

    public void InitializeTask()
    {
        InitializeTaskStep();
    }

    private void InitializeTaskStep()
    {
        currentStep.CompleteAction += HandleStepComplete;
        currentStep.FailAction += HandleStepFail;

        currentStep.FlagAsCurrent();

        if (currentStep.isComplete)
            HandleStepComplete();
    }

    private void HandleStepComplete()
    {
        currentStep.CompleteAction -= HandleStepComplete;
        currentStep.FailAction -= HandleStepFail;

        stepIndex++;
        if (stepIndex >= steps.Count)
        {
            Debug.Log("Task Complete!");
            GameStateManager.Singleton.StopTimer();
        }
        else
        {
            Debug.Log($"Step {stepIndex} Complete! Moving to next step.");
            InitializeTaskStep();
        }
    }

    private void HandleStepFail()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public int GetStepIndex()
    {
        return stepIndex;
    }
}



