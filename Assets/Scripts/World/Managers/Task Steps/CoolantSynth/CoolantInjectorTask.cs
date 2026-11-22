using UnityEngine;

public class CoolantInjectorTask : TaskStepCheck
{
    [SerializeField] CoolantInjector _ci;

    private void Awake()
    {
        _ci.OnCoolantInjected += HandleCoolantInjection;
    }

    public override void UpdateTaskCheck()
    {
        return;
    }

    private void HandleCoolantInjection()
    {
        OnTaskComplete?.Invoke(TaskStep.TaskCompletionStatus.Completed);
    }
}
