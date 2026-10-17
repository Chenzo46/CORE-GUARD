using UnityEngine;
using System;

public class TaskStep : MonoBehaviour
{
    public bool isComplete = false;
    public bool unordered = true;
    public Interactable Listener;
    public Action CompleteAction;
    public Action FailAction;

    private void Awake()
    {
        Listener.OnInteractComplete += CompleteStep;

        if (!unordered)
        {
            Listener.SetInteractionEnabled(false);
        }
    }

    public void CompleteStep()
    {
        Listener.OnInteractComplete -= CompleteStep;

        isComplete = true;
        CompleteAction?.Invoke();
    }

    public void FailStep()
    {
        isComplete = false;
        FailAction?.Invoke();
    }

    public void FlagAsCurrent()
    {
        Listener.SetInteractionEnabled(true);
    }
}
