using TMPro;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text interactCtxLabel;
    [SerializeField] private CanvasGroup baseHUDGroup;
    [SerializeField] private CanvasGroup monitorHUDGroup;
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

    public void SetInteractableContext(string context)
    {
        interactCtxLabel.text = context;
    }

    public void SwitchToMonitorHUD()
    {
        ShowCanvasGroup(currentGroup, false);
        ShowCanvasGroup(monitorHUDGroup, true);
    }

    public void SwitchToBaseHUD()
    {
        ShowCanvasGroup(currentGroup, false);
        ShowCanvasGroup(baseHUDGroup, true);
    }

    public void ShowCanvasGroup(CanvasGroup cg, bool show)
    {
        cg.alpha = show ? 1 : 0;
        cg.blocksRaycasts = show;
        cg.interactable = show;

        if (show)
            currentGroup = cg;
    }

    public static bool rectOverlaps(RectTransform rectTrans1, RectTransform rectTrans2)
    {
        Rect rect1 = new Rect(rectTrans1.localPosition.x, rectTrans1.localPosition.y, rectTrans1.rect.width, rectTrans1.rect.height);
        Rect rect2 = new Rect(rectTrans2.localPosition.x, rectTrans2.localPosition.y, rectTrans2.rect.width, rectTrans2.rect.height);

        return rect1.Overlaps(rect2);
    }
}
