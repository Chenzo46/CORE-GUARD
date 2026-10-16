using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Item : Interactable
{
    private bool isHeld = false;
    private PlayerBase player;

    private Action UseAction;
    [SerializeField] private InputAction UseActionKeybind;

    public override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        //UseActionKeybind.performed += 
    }

    private void OnDisable()
    {
        
    }

    public override void InteractFunction(PlayerBase sender)
    {
        base.InteractFunction(sender);

        if (!isHeld)
        {
            // hold
            player = sender;
        }
    }

    public void Drop()
    {
        if (!isHeld)
            return;

        player = null;
    }

    public virtual void UseActionInvoke()
    {

    }
}
