using UnityEngine;
using DG.Tweening;


public class TestInteractable : Interactable
{
    private Tween currentMove;

    public override void Awake()
    {
        base.Awake();
    }

    public override void InteractFunction(PlayerBase sender)
    {
        if (currentMove != null)
            return;

        base.InteractFunction(sender);

        currentMove = transform.DOLocalMove(transform.position + transform.right * 2f, 1.3f).SetEase(Ease.OutBounce);

        currentMove.OnComplete(() =>
        {
            currentMove = null;
        });
    }
}
