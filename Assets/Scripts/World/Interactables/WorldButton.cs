using DG.Tweening;
using UnityEngine;

public class WorldButton : Interactable
{
    private Vector3 orgLocalPos;
    private bool isBeingPressed = false;

    private string orgCtx;

    public override void Awake()
    {
        base.Awake();
        orgLocalPos = transform.localPosition;
        orgCtx = GetContext();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void InteractFunction(PlayerBase sender)
    {
        if(isBeingPressed)
            return;

        isBeingPressed = true;

        SetContext($"{GetContext()} (In Use...)");

        base.InteractFunction(sender);
        
        transform.DOLocalMove(orgLocalPos + transform.forward * 0.035f, 0.1f).OnComplete(() =>
        {
            transform.DOLocalMove(orgLocalPos, 0.1f).OnComplete(() =>
            {
                isBeingPressed = false;
                SetContext(orgCtx);
            });
        });
    }
}
