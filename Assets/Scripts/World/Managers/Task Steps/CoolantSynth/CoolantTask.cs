using UnityEngine;
using UnityEngine.UI;

public class CoolantTask : TaskStepCheck
{
    [SerializeField] private CoolantSynthesizer _cs;

    public override void UpdateTaskCheck()
    {
        if(_cs.GetCoolantStatus() == CoolantSynthesizer.CoolantStatus.Complete)
        {
            isComplete = true;
            OnTaskComplete?.Invoke(TaskStep.TaskCompletionStatus.Completed);
        }
        else if (_cs.GetCoolantStatus() == CoolantSynthesizer.CoolantStatus.Incorrect)
        {
            isComplete = true;
            OnTaskComplete?.Invoke(TaskStep.TaskCompletionStatus.Failed);
        }
    }
}
