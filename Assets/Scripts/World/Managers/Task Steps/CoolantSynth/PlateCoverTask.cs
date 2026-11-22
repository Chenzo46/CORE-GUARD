using UnityEngine;

public class PlateCoverTask : TaskStepCheck
{
    [SerializeField] ScrewPlateCover _spc;

    int screwsRemovedCount = 0;

    private void Awake()
    {
        _spc.OnInteractComplete += UpdateScrewsRemoved;
    }

    public override void UpdateTaskCheck()
    {
        return;
    }

    private void UpdateScrewsRemoved()
    {
        screwsRemovedCount++;

        if (screwsRemovedCount > 1)
            OnTaskComplete?.Invoke(TaskStep.TaskCompletionStatus.Completed);
    }
}
