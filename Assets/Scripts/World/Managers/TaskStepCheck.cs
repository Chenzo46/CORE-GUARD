using System;
using UnityEngine;

public abstract class TaskStepCheck : MonoBehaviour
{
    public Action<TaskStep.TaskCompletionStatus> OnTaskComplete;
    public bool isComplete { get; protected set; }
    public abstract void UpdateTaskCheck();
}
