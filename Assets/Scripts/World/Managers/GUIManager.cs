using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text interactCtxLabel, timerLabel;
    [SerializeField] private CanvasGroup baseHUDGroup, taskGUIGroup;
    [SerializeField] private CanvasGroup monitorHUDGroup;
    [SerializeField] private CanvasGroup transitionGroup;
    [SerializeField] private CanvasGroup dayHUDGroup;
    [Header("Task Manager Properties")]
    [SerializeField] private CanvasGroup taskViewGroup;
    [SerializeField] private TMP_Text taskTitleLabel;
    [SerializeField] private TMP_Text taskStepDetailLabel;
    [SerializeField] private RectTransform objectiveCursor;
    [SerializeField] private float CursorRotSpeed = 1f;

    public static GUIManager Singleton;
    private CanvasGroup currentGroup;

    private void Awake()
    {
        if (Singleton == null)
        {
            currentGroup = baseHUDGroup;
            Singleton = this;
        }
        else
        {
            // for DontDestroyOnLoad
        }
    }

    private void Update()
    {
        objectiveCursor.Rotate(0,0,CursorRotSpeed * Time.deltaTime);
    }

    public void SetInteractableContext(string context)
    {
        interactCtxLabel.text = context;
    }

    public void SwitchToMonitorHUD()
    {
        ShowCanvasGroup(baseHUDGroup, false);
        ShowCanvasGroup(monitorHUDGroup, true);
    }

    public void SwitchToBaseHUD()
    {
        ShowCanvasGroup(monitorHUDGroup, false);
        ShowCanvasGroup(baseHUDGroup, true);
    }

    public void ShowTaskGUI(bool show)
    {
        ShowCanvasGroupOverlap(taskGUIGroup, show);
    }

    public void ShowCanvasGroup(CanvasGroup cg, bool show)
    {
        cg.alpha = show ? 1 : 0;
        cg.blocksRaycasts = show;
        cg.interactable = show;

        if (show)
            currentGroup = cg;
    }

    public static void ShowCanvasGroupOverlap(CanvasGroup cg, bool show)
    {
        cg.alpha = show ? 1 : 0;
        cg.blocksRaycasts = show;
        cg.interactable = show;
    }


    public static void FadeCanvasGroup(CanvasGroup cg, float finalAlpha, float duration)
    {
        DOTween.To(x => cg.alpha = x, cg.alpha, finalAlpha, duration).OnComplete(() =>
        {
            cg.blocksRaycasts = finalAlpha != 0f;
            cg.interactable = finalAlpha != 0f;
        });
    }

    public void FadeDayText(float duration)
    {
        FadeCanvasGroup(dayHUDGroup, 0f, duration);
    }

    public void SetDayText(int day)
    {
        dayHUDGroup.transform.GetChild(0).GetComponent<TMP_Text>().SetText($"DAY {day}");
    }

    public void SetTimer(float time)
    {
        int ms = (int)((time * 100) - (Mathf.Floor(time) * 100));
        int s = (int)Mathf.Floor(time);

        timerLabel.text = $"<mspace=0.7em>{s:D2}:{ms:D2}</mspace>";
    }

    public void UpdateTaskView(Task tm)
    {
        taskTitleLabel.text = tm.taskName;

        string steps = ""; // only add current and old tasks

        for(int idx = 0; idx < tm.GetStepIndex()+1; idx++)
        {
            if (idx >= tm.steps.Count)
                break;

            steps += $"- {tm.steps[idx].GetDescription()}\n";
        }

        taskStepDetailLabel.text = steps;
    }

    public void FadeSceneIn(float duration)
    {
        ShowCanvasGroup(transitionGroup, true);
        FadeCanvasGroup(transitionGroup, 0f, duration);
    }

    public void FadeSceneOut(float duration)
    {
        FadeCanvasGroup(transitionGroup, 1f, duration);
    }

    public RectTransform GetObjectiveCursor()
    {
        return objectiveCursor;
    }

    public RectTransform GetCanvasRectTransform()
    {
        return GetComponent<RectTransform>();
    }

    public static bool rectOverlaps(RectTransform rectTrans1, RectTransform rectTrans2)
    {
        Rect rect1 = new Rect(rectTrans1.localPosition.x, rectTrans1.localPosition.y, rectTrans1.rect.width, rectTrans1.rect.height);
        Rect rect2 = new Rect(rectTrans2.localPosition.x, rectTrans2.localPosition.y, rectTrans2.rect.width, rectTrans2.rect.height);

        return rect1.Overlaps(rect2);
    }
}
