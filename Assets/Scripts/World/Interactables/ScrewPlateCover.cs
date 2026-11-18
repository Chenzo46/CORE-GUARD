using DG.Tweening;
using UnityEngine;

public class ScrewPlateCover : Interactable
{
    [Header("Plate Cover Settings")]

    [SerializeField] private Rigidbody _plateRb;
    [SerializeField] private Rigidbody[] screws = new Rigidbody[2];

    private int amountRemoved = 0;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void InteractFunction(PlayerBase sender)
    {
        RemoveScrew();
    }

    public void RemoveScrew()
    {
        if (amountRemoved >= 2)
            return;

        amountRemoved++;
        Rigidbody currentScrew = screws[amountRemoved - 1];
        currentScrew.transform.DOLocalMoveZ(currentScrew.transform.localPosition.z + 0.1f, 0.3f);
        currentScrew.transform.DOLocalRotate(new Vector3(0, 0, 60), 0.3f).OnComplete(() => 
        {
            currentScrew.isKinematic = false;
            currentScrew.AddForce(transform.forward * 5f, ForceMode.Impulse);

            currentScrew.transform.DOScale(Vector3.zero, 0.5f).SetDelay(1f).OnComplete(() =>
            {
                currentScrew.gameObject.SetActive(false);
            });

            if (currentScrew == screws[1])
            {
                Debug.Log("All screws removed.");
                _plateRb.isKinematic = false;
                _plateRb.AddForce(transform.forward * 5f, ForceMode.Impulse);

                transform.DOScale(Vector3.zero, 0.5f).SetDelay(1f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            }
        });
    }
}
