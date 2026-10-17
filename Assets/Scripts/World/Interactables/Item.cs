using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Item : Interactable
{
    private float handFollowStrength = 0.01f;
    private float handRotateStrength = 20f;
    private bool isHeld = false;
    private Rigidbody _rb;
    private Collider _col;
    private PlayerBase player;
    private const int HAND_LAYER = 9;
    private const int WORLD_LAYER = 10;

    private Vector3 dampRef;

    private Action UseAction;
    [SerializeField] private InputAction UseActionKeybind;

    public override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        UseActionKeybind.performed += UseActionInvoke;
        UseAction += UseActionFunction;
    }

    private void OnDisable()
    {
        UseActionKeybind.performed -= UseActionInvoke;
        UseAction -= UseActionFunction;
    }

    private void LateUpdate()
    {
        FollowHand();
    }

    public override void InteractFunction(PlayerBase sender)
    {
        base.InteractFunction(sender);

        if (!isHeld)
        {
            // hold
            player = sender;
            _rb.isKinematic = true;
            _col.isTrigger = true;
            _rb.interpolation = RigidbodyInterpolation.None;
            transform.rotation = Quaternion.identity;
            isHeld = true;
            player.SetHeldItem(this);
            gameObject.layer = HAND_LAYER;
            //transform.SetParent(player.GetHandTransform(), true);
            //transform.localPosition = Vector3.zero;
        }
    }

    public void Drop()
    {
        if (!isHeld)
            return;

        _rb.isKinematic = false;
        _col.isTrigger = false;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        isHeld = false;
        gameObject.layer = WORLD_LAYER;
        _rb.linearVelocity = player.GetPlayerVelocity();

        player.SetHeldItem(null);
        player = null;
    }

    private void FollowHand()
    {
        if (!isHeld)
            return;

        transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, player.GetHandTransform().position, ref dampRef, handFollowStrength),
        Quaternion.Slerp(transform.rotation, player.GetHandTransform().rotation, handRotateStrength * Time.deltaTime));
    }

    public virtual void UseActionInvoke(InputAction.CallbackContext context)
    {
        if(context.performed && isHeld)
            UseAction?.Invoke();
    }
    
    public virtual void UseActionFunction()
    {
        Debug.Log($"{GetContext()} used");
        // to be overriden by child classes
    }
}
