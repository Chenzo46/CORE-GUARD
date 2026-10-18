using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Lever : Interactable
{
    [Header("Lever Settings")]
    [SerializeField] private Transform leverHand;
    [SerializeField] private bool isOn = false;

    public UnityEvent OnSwitchOn;
    public UnityEvent OnSwitchOff;

    private Tween switchOnTween;
    private Tween switchOffTween;

    public override void Awake()
    {
        base.Awake();
        InitTweens();
    }

    private void InitTweens()
    {
        leverHand.localRotation = Quaternion.Euler(isOn ? 60f : -60f, 0, 0);
        switchOnTween = leverHand.DOLocalRotate(new Vector3(60f, 0, 0), 0.5f).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
        switchOffTween = leverHand.DOLocalRotate(new Vector3(-60f, 0, 0), 0.5f).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
    }

    public override void InteractFunction(PlayerBase sender)
    {
        base.InteractFunction(sender);

        if (switchOnTween.IsPlaying() || switchOffTween.IsPlaying())
            return;

        if (isOn)
        {
            isOn = false;
            //switchOffTween.Play();
            switchOffTween.Restart();
            OnSwitchOff?.Invoke();
        }
        else
        {
            isOn = true;
            switchOnTween.Restart();
            OnSwitchOn?.Invoke();
        }
    }
}
