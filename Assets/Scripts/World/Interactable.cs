using System;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    public Action<Transform> interactAction;

    [SerializeField] private string context = "Interactable Object";

    public virtual void Awake()
    {
        interactAction += InteractFunction;
    }

    public virtual void InteractFunction(Transform sender)
    {
        Debug.Log($"{context} interaction started");
    }

    public virtual void Interact(Transform sender)
    {
        interactAction.Invoke(sender);
    }
}

public interface IInteractable 
{
    public void Interact(Transform sender);
}
