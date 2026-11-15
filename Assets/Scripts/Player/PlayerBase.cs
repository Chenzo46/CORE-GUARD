using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Linq;

public class PlayerBase : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _cc;
    [SerializeField] private Transform checkPivot;
    [SerializeField] private Transform heldItemTransform;
    [Header("Action Map")]
    [SerializeField] private InputActionAsset playerActionMap;
    [Header("Movement Variables")]
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float counterMovement = 1f;
    //[SerializeField] private float maxSpeed = 10f;
    //[SerializeField] private float maxFallSpeed = 30f;
    [SerializeField] private float airMoveFactor = 0.3f;
    [SerializeField] private float lookSensitivity;
    [SerializeField] private float gamepadSensitivityScale = 100f;
    [SerializeField] private float jumpStrength;
    [SerializeField] private float jumpCancelStrength = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkRadius;
    //[SerializeField] private float handFollowStrength = 20f;
    [SerializeField] private Vector3 handOffset = Vector3.zero;
    [SerializeField] private Transform eyeLevelTransform;
    [Header("Object Interaction")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask interactableMask;
    //Input Actions
    private InputActionMap moveActionMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction JumpAction;
    private InputAction interactAction;
    private InputAction dropAction;
    private InputAction useAction;

    //Movement Variables
    private Vector2 moveInput;
    private Vector3 moveDir = new Vector3();
    private Vector2 lookDir;

    private Vector3 moveVelocity;
    private float yVelocity;
    private float rotateY;
    private float rotateX;

    //Singleton
    public static PlayerBase Singleton;

    //Item interaction

    private Item heldItem;
    

    private void Awake()
    {
        Singleton = this;

        //Movement Action map
        moveActionMap = playerActionMap.FindActionMap("Movement");
        moveAction = moveActionMap.FindAction("Move");
        lookAction = moveActionMap.FindAction("Look");
        JumpAction = moveActionMap.FindAction("Jump");
        interactAction = moveActionMap.FindAction("Interact");
        dropAction = moveActionMap.FindAction("Drop");
        useAction = moveActionMap.FindAction("Use");

        rotateY = Camera.main.transform.eulerAngles.y;
        rotateX = Camera.main.transform.eulerAngles.x;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }
   
    private void Start()
    {
        heldItemTransform.parent = Camera.main.transform;
    }
    private void Update()
    {
        movementWrapper();
        LookingAtInteractable();
        HeldItemBehavior();
    }

    private void LateUpdate()
    {
        look();
    }

    private void movementWrapper()
    {
        movementHorizontal();
        movementVertical();
    }


    private void movementHorizontal()
    {
        float airMoveSwitch = 1 - airMoveFactor * (IsGrounded() ? 0 : 1);

        moveInput = moveAction.ReadValue<Vector2>();
        moveDir = (transform.forward * moveInput.y + transform.right * moveInput.x) * airMoveSwitch;

        moveVelocity += moveDir * moveSpeed * Time.deltaTime;

        moveVelocity += -moveVelocity * counterMovement * Time.deltaTime;

        _cc.Move(moveVelocity * Time.deltaTime);
    }

    private void movementVertical()
    {
        if (IsGrounded() && yVelocity <= 0)
        {
            yVelocity = -2f;
        }

        if (JumpAction.WasPerformedThisFrame() && IsGrounded())
        {
            yVelocity = Mathf.Sqrt(jumpStrength * -2f * gravity);
            
        }
        else if (JumpAction.WasReleasedThisFrame() && !IsGrounded() && yVelocity > 0)
        {
            yVelocity /= jumpCancelStrength;
        }

        yVelocity += gravity * Time.deltaTime;

        _cc.Move(Vector3.up * yVelocity * Time.deltaTime);
    }

    private void look()
    {
        if (!moveActionMap.enabled)
        {
            return;
        }

        lookDir = lookAction.ReadValue<Vector2>();

        Camera.main.transform.position = eyeLevelTransform.position;

        Vector2 delta;
        InputDevice device = lookAction.activeControl?.device;

        if (device is Gamepad)
            delta = lookDir * lookSensitivity * gamepadSensitivityScale * Time.deltaTime;
        else
            delta = lookDir * lookSensitivity;

        rotateX -= delta.y;
        rotateY += delta.x;

        rotateX = Mathf.Clamp(rotateX, -90f, 90f);

        Camera.main.transform.rotation = Quaternion.Euler(rotateX, rotateY, Camera.main.transform.eulerAngles.z);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotateY, transform.rotation.eulerAngles.z);
    }

    private void LookingAtInteractable()
    {
        if (!moveActionMap.enabled)
        {
            GUIManager.Singleton.SetInteractableContext(string.Empty);
            return;
        }
        RaycastHit hit;

        bool inView = Physics.Linecast(eyeLevelTransform.position, 
            eyeLevelTransform.position + Camera.main.transform.forward * interactDistance,
            out hit,
            interactableMask);

        if(inView)
        {
            IInteractable interactable = hit.collider.transform.GetComponent<IInteractable>();

            GUIManager.Singleton.SetInteractableContext(interactable.GetContext());

            if (interactAction.WasPressedThisFrame())
            {
                interactable.Interact(this);
            }
        }
        else
        {
            GUIManager.Singleton.SetInteractableContext(string.Empty);
        }
    }

    private bool IsGrounded()
    {
        RaycastHit hitInfo;
        return Physics.SphereCast(checkPivot.position, checkRadius, -Vector3.up, out hitInfo, checkRadius, groundLayer);
    }

    public Transform GetHandTransform()
    {
        return heldItemTransform;
    }

    public Transform GetEyeLevelTransform()
    {
        return eyeLevelTransform;
    }

    public void SetHeldItem(Item it)
    {
        heldItem = it;
    }

    public bool IsHoldingItem()
    {
        return heldItem != null;
    }   

    public Item GetHeldItem()
    {
        return heldItem;
    }

    private void HeldItemBehavior()
    {
        heldItemTransform.localPosition = handOffset;
        if (heldItem == null)
            return;

        if(heldItem.GetItemType() == ItemType.Plug)
        {
            Transform cableRoot = heldItem.transform.parent.GetChild(0);
           
            if(Vector3.Distance(cableRoot.position, transform.position) > 5f)
            {
                heldItem.Drop();
            }
        }

        if(dropAction.WasPerformedThisFrame())
        {
            heldItem.Drop();
        }
        else if (useAction.WasPerformedThisFrame())
        {
            heldItem.UseActionFunction();
        }
    }

    public Vector3 GetPlayerVelocity()
    {
        return moveVelocity + Vector3.up * yVelocity;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = IsGrounded() ? Color.blue : Color.red;
        Gizmos.DrawSphere(checkPivot.position, checkRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyeLevelTransform.position + handOffset, 0.1f);
    }
}
