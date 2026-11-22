using System;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IInteractable
{
    public Action<PlayerBase> interactAction;
    public Action OnInteractComplete;
    [Header("Interactable Settings")]

    [SerializeField] private string context = "Interactable Object";
    [SerializeField] private string interactSound = string.Empty;
    [SerializeField] private bool singleInteraction = false;
    [SerializeField] private bool interactionEnabled = true;
    [SerializeField] private ItemConstraint itemConstraint;
    public UnityEvent OnInteractCompleteUnity;

    private bool singleComplete = false;
    private bool isLooking = false;

    public virtual void Awake()
    {
        interactAction += InteractFunction;
    }

    public virtual void OnDestroy()
    {
        interactAction -= InteractFunction;
    }

    public virtual void InteractFunction(PlayerBase sender)
    {
        Debug.Log($"{context} interaction started");
    }

    public virtual void Interact(PlayerBase sender)
    {
        if (IsConstraintMet(sender))
        {
            singleComplete = singleInteraction ? true : false;

            interactAction.Invoke(sender);
            OnInteractComplete?.Invoke();
            OnInteractCompleteUnity?.Invoke();
            
            if(!interactSound.Equals(string.Empty))
                AudioManager.Singleton.PlaySoundWorld(interactSound, transform.position);
        }

    }

    public virtual string GetContext()
    {
        return context;
    }

    public virtual void SetContext(string newContext)
    {
        context = newContext;
    }

    private bool IsConstraintMet(PlayerBase sender)
    {
        if (singleComplete || !interactionEnabled)
            return false;

        Item heldItem = sender.GetHeldItem();
        return (itemConstraint.constraintActive && heldItem != null && heldItem.GetItemType() == itemConstraint.itemType)
            || (itemConstraint.constraintActive && itemConstraint.requireFreeHands && heldItem == null)
            || (!itemConstraint.constraintActive);
    }

    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;
    }

    public void UpdateIsLooking(bool isLooking)
    {
        this.isLooking = isLooking;
    }
}

[System.Serializable]
public struct ItemConstraint 
{
    public bool constraintActive;
    public bool requireFreeHands;
    public ItemType itemType;
}


public interface IInteractable 
{
    public void Interact(PlayerBase sender);
    public string GetContext();
    public void SetContext(string newContext);

    public void UpdateIsLooking(bool looking);
}
