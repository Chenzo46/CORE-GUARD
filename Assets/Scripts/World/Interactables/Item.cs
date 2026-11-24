using System;
using UnityEngine;

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
    [Header("Item Settings")]
    [SerializeField] private ItemType itemType = ItemType.None;
    [SerializeField] private float outOfBoundsOffset = 0.5f;
    [SerializeField] private LayerMask outOfBoundsExclude;


    public override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }

    private void LateUpdate()
    {
        FollowHand();
    }

    public override void InteractFunction(PlayerBase sender)
    {
        if (sender.IsHoldingItem())
            return;

        base.InteractFunction(sender);

        if (!isHeld)
        {
            // hold
            AudioManager.Singleton.PlaySound("grab-item", 0.3f);
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

        HandleOutOfBounds();

        player.SetHeldItem(null);
        player = null;

        AudioManager.Singleton.PlaySound("drop-item", 0.2f, 0.2f);
    }

    private void FollowHand()
    {
        if (!isHeld)
            return;

        transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, player.GetHandTransform().position, ref dampRef, handFollowStrength),
        Quaternion.Slerp(transform.rotation, player.GetHandTransform().rotation, handRotateStrength * Time.deltaTime));
    }

    private void HandleOutOfBounds()
    {
        RaycastHit rh;

        if (Physics.Linecast(player.transform.position, transform.position, out rh))
        {
            if (IsLayerInMask(rh.transform.gameObject.layer, outOfBoundsExclude))
                return;

            Vector3 playerDir = (transform.position - player.transform.position).normalized;

            transform.position = rh.point - playerDir * outOfBoundsOffset;
        }
    }

    public virtual void UseActionFunction()
    {
        Debug.Log($"{GetContext()} used");
        // to be overriden by child classes
    }

    public ItemType GetItemType()
    {
        return itemType;
    }

    private bool IsLayerInMask(int layer, LayerMask lm)
    {
        return (lm.value & (1 << layer)) != 0;

    }
}

    public enum ItemType
    {       
        None,
        Screwdriver,
        Broom,

        Plug,
    }
