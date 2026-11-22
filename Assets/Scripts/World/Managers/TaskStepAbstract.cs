using UnityEngine;

public class TaskStepAbstract : TaskStep
{
    [SerializeField] private TaskStepCheck checkFunctionality;

    public override void Awake()
    {
        checkFunctionality.OnTaskComplete += FinalizeStep;
    }

    void Update()
    {
        if (checkFunctionality.isComplete)
            return;

        checkFunctionality.UpdateTaskCheck();
    }

    private void FinalizeStep(TaskCompletionStatus tcs)
    {
        if(tcs == TaskCompletionStatus.Completed)
        {
            CompleteStep();
        }
        else
        {
            FailStep();
        }
    }

    public override void CompleteStep()
    {
        isComplete = true;
        CompleteAction?.Invoke();
    }

    public override void FailStep()
    {
        base.FailStep();
    }

    public override void FlagAsCurrent()
    {
        OnBecomeCurrent?.Invoke();
    }
}
