using UnityEngine;
using DG.Tweening;


public class TestInteractable : Interactable
{
    private Tween currentMove;

    private string orgCtx;

    public override void Awake()
    {
        base.Awake();
        orgCtx = GetContext();
    }

    public override void InteractFunction(PlayerBase sender)
    {
        if (currentMove != null)
            return;

        base.InteractFunction(sender);

        SetContext($"{orgCtx} (In Use...)");

        currentMove = transform.DOLocalMove(transform.position + transform.right * 2f, 1.3f).SetEase(Ease.OutBounce);

        currentMove.OnComplete(() =>
        {
            currentMove = null;
            SetContext(orgCtx);
        });
    }
}
