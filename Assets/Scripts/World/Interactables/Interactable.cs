using System;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    public Action<PlayerBase> interactAction;

    [SerializeField] private string context = "Interactable Object";

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
        interactAction.Invoke(sender);
    }

    public virtual string GetContext()
    {
        return context;
    }

    public virtual void SetContext(string newContext)
    {
        context = newContext;
    }
}

public interface IInteractable 
{
    public void Interact(PlayerBase sender);
    public string GetContext();
    public void SetContext(string newContext);
}
