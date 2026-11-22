using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TaskManager : MonoBehaviour
{
    public Task currentTask;
    private RectTransform objectiveCursor;
    private CanvasGroup cursorImage;

    [SerializeField] private float cursorAdjustMaxDistance = 10f;
    [SerializeField] private float minAlpha = 120f;
    [SerializeField] private float minScale = 0.2f;

    private void Awake()
    {
        currentTask.InitializeTask();
    }

    private void Start()
    {
        GameStateManager.Singleton.OnTimerDepleted += FailCurrentStep;
        objectiveCursor = GUIManager.Singleton.GetObjectiveCursor();
        cursorImage = objectiveCursor.GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        HandleObjectiveCursor();
    }

    private void HandleObjectiveCursor()
    {
        // Scale with distance between camera and objective (Size and Opacity)
        // Only show on first step of task

        if(currentTask.GetStepIndex() < 1)
        {

            // size + alpha
            float distFromObjective = Vector3.Distance(Camera.main.transform.position, currentTask.currentStep.transform.position);

            float prog = 1 - Mathf.Clamp(distFromObjective / cursorAdjustMaxDistance, 0, 1);

            float imageA = Mathf.Lerp(minAlpha, 1,1 - prog);
            float scale = Mathf.Lerp(minScale, 1, prog);

            cursorImage.alpha = imageA;
            objectiveCursor.localScale = new Vector3(scale, scale, 1);

            //position

            Camera cam = Camera.main;
            Transform target = currentTask.currentStep.transform;

            Vector3 screenPos = cam.WorldToScreenPoint(target.position);

            if (screenPos.z < 0f)
            {
                Vector3 camLocal = cam.transform.InverseTransformPoint(target.position);
                Vector2 dir = new Vector2(camLocal.x, camLocal.y);

                if (dir.sqrMagnitude < 0.0001f)
                    dir = Vector2.down;
                dir.Normalize();

                float far = Mathf.Max(Screen.width, Screen.height);
                screenPos = new Vector3(
                    Screen.width * 0.5f + dir.x * far,
                    Screen.height * 0.5f + dir.y * far,
                    0f);
            }

            RectTransform parentRect = (RectTransform)objectiveCursor.parent;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, null, out Vector2 localPos))
            {
                Rect bounds = parentRect.rect;
                Vector2 half = objectiveCursor.rect.size * 0.5f;

                localPos.x = Mathf.Clamp(localPos.x, bounds.xMin + half.x, bounds.xMax - half.x);
                localPos.y = Mathf.Clamp(localPos.y, bounds.yMin + half.y, bounds.yMax - half.y);

                objectiveCursor.anchoredPosition = localPos;
            }
        }
        else
        {
            cursorImage.alpha = 0;
            cursorImage.interactable = false;
            cursorImage.blocksRaycasts = false;
        }
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
    public string taskName;
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

        GUIManager.Singleton.UpdateTaskView(this);

        currentStep.FlagAsCurrent();

        if (currentStep.isComplete)
            HandleStepComplete();
    }

    private void HandleStepComplete()
    {
        currentStep.CompleteAction -= HandleStepComplete;
        currentStep.FailAction -= HandleStepFail;

        stepIndex++;

        GUIManager.Singleton.UpdateTaskView(this);

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



