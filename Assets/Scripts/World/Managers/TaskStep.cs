using UnityEngine;
using System;
using UnityEngine.Events;

public class TaskStep : MonoBehaviour
{
    public string taskDescription = "";
    public bool isComplete = false;
    public bool unordered = true;
    public Interactable Listener;
    public Action CompleteAction;
    public Action FailAction;
    public UnityAction OnBecomeCurrent;

    private bool flaggedAsCurrent = false;

    public enum TaskCompletionStatus
    {
        Completed,
        Failed,
    }

    public virtual void Awake()
    {
        Listener.OnInteractComplete += CompleteStep;

        if (!unordered && !flaggedAsCurrent)
        {
            Listener.SetInteractionEnabled(false);
        }
    }

    public virtual void CompleteStep()
    {
        Listener.OnInteractComplete -= CompleteStep;

        isComplete = true;
        CompleteAction?.Invoke();
        flaggedAsCurrent = false;
    }

    public virtual void FailStep()
    {
        isComplete = false;
        FailAction?.Invoke();
        flaggedAsCurrent = false;
    }

    public virtual void FlagAsCurrent()
    {
        OnBecomeCurrent?.Invoke();
        flaggedAsCurrent = true;
        Listener.SetInteractionEnabled(true);
    }

    public virtual string GetDescription()
    {
        return taskDescription;
    }
}