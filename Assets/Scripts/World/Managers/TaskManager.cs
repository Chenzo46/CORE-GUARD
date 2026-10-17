using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public Task currentTask;

    private void Awake()
    {
        currentTask.InitializeTask();
    }

    public void CompleteCurrentStep()
    {
        currentTask.currentStep.CompleteStep();
    }

    public void FailCurrentStep()
    {
        currentTask.currentStep.FailStep();
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
        }
        else
        {
            Debug.Log($"Step {stepIndex} Complete! Moving to next step.");
            InitializeTaskStep();
        }
    }

    private void HandleStepFail()
    {
        
    }
}



