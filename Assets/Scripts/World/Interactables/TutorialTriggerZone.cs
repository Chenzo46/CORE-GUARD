using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using DG.Tweening;

public class TutorialTriggerZone : MonoBehaviour
{
    [SerializeField] private ScriptableTutorial tutorials;
    [SerializeField] private InputActionAsset _ipa;
    [SerializeField] private Vector3 tutorialBounds = new Vector3(1,1,1);
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private bool useOutsideTrigger = false;

    private bool tutorialShown = false;

    private List<CanvasGroup> tutorialGroups = new List<CanvasGroup>();

    private void Update()
    {
        if ((IsPlayerInBounds() && !useOutsideTrigger) && !tutorialShown)
        {
            SendTutorialSteps();
        }
    }

    public void SendTutorialSteps()
    {
        if (tutorialShown)
            return;

        tutorialShown = true;

        tutorialGroups = GUIManager.Singleton.GenerateTutorialVisuals(tutorials.tutorials);

        SetTutorialInteractionListener();
    }

    private void SetTutorialInteractionListener()
    {
        foreach(TutorialNode tn in tutorials.tutorials)
        {
            if (tn.isComplete)
                continue;

            InputActionMap iam = _ipa.FindActionMap(tn.tutorialInputMap);
            InputAction action = iam.FindAction(tn.tutorialKey);

            action.performed += ClearTutorialVisual;
        }
    }

    private void ClearTutorialVisual(InputAction.CallbackContext ctx)
    {
        string tKey = ctx.action.name;

        TutorialNode tn = tutorials.GetTutorialNode(tKey);
        CanvasGroup cg = tutorialGroups[tutorials.GetTutorialNodeIndex(tn)];

        DOTween.To(x => cg.alpha = x, 1f, 0f, 1f).OnComplete(() =>
        {
            Destroy(cg.gameObject);
        });

        tn.isComplete = true;

        ctx.action.performed -= ClearTutorialVisual;
    }

    public bool IsPlayerInBounds()
    {
        return Physics.OverlapBox(transform.position, tutorialBounds / 2f, Quaternion.identity, playerMask).Length > 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, tutorialBounds);
    }
}
