using DG.Tweening;
using UnityEngine;

public class FuzeboxDoor : Interactable {

    public override void InteractFunction(PlayerBase sender)
    {
        base.InteractFunction(sender);

        transform.DOLocalRotate(new Vector3(0,155f,0), 1f).SetEase(Ease.OutBack);
    }
    
}
