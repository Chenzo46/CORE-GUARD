using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerBase : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CharacterController _cc;
    [SerializeField] private Transform checkPivot;
    [Header("Action Map")]
    [SerializeField] private InputActionAsset playerActionMap;
    [Header("Movement Variables")]
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float counterMovement = 1f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float maxFallSpeed = 30f;
    [SerializeField] private float airMoveFactor = 0.3f;
    [SerializeField] private float lookSensitivity;
    [SerializeField] private float jumpStrength;
    [SerializeField] private float jumpCancelStrength = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkRadius;
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

        rotateY = Camera.main.transform.eulerAngles.y;
        rotateX = Camera.main.transform.eulerAngles.x;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }



    private void OnEnable()
    {
        //setActionsEnabled(true);
    }
    private void OnDisable()
    {
        //setActionsEnabled(false);
    }

   
    private void Start()
    {

    }
    private void Update()
    {
        movementWrapper();
        LookingAtInteractable();
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

        //rb.AddForce(moveDir * moveSpeed * Time.fixedDeltaTime, ForceMode.Force); // Movement Force
        //rb.AddForce(new Vector3(-rb.linearVelocity.x, 0, -rb.linearVelocity.z) * counterMovement * airMoveSwitch * Time.fixedDeltaTime, ForceMode.Force); // Counter Force

        // Clamp Horizontal and vertical speeds
        //Vector3 horizontalClamp = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        //horizontalClamp = Vector3.ClampMagnitude(horizontalClamp, maxSpeed);
        //Vector3 verticalClamp = Vector3.ClampMagnitude(Vector3.up * rb.linearVelocity.y, maxFallSpeed);
        //rb.linearVelocity = horizontalClamp + verticalClamp;
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
            //rb.AddForce(transform.up * jumpStrength, ForceMode.Impulse);
            Debug.Log("Jump Triggered");
        }
        else if (JumpAction.WasReleasedThisFrame() && !IsGrounded() && yVelocity > 0)
        {
            yVelocity /= jumpCancelStrength;
            //rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * jumpCancelStrength, rb.linearVelocity.z);
        }

        yVelocity += gravity * Time.deltaTime;

        _cc.Move(Vector3.up * yVelocity * Time.deltaTime);
    }

    private void look()
    {
        lookDir = lookAction.ReadValue<Vector2>();

        Camera.main.transform.position = eyeLevelTransform.position;

        rotateX -= lookDir.y * lookSensitivity * Time.deltaTime;
        rotateY += lookDir.x * lookSensitivity * Time.deltaTime;

        rotateX = Mathf.Clamp(rotateX, -90f, 90f);

        Camera.main.transform.rotation = Quaternion.Euler(rotateX, rotateY, Camera.main.transform.eulerAngles.z);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotateY, transform.rotation.eulerAngles.z);
    }

    private void LookingAtInteractable()
    {
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
            //set GUI
            GUIManager.Singleton.SetInteractableContext(string.Empty);
        }
    }

    private bool IsGrounded()
    {
        RaycastHit hitInfo;
        return Physics.SphereCast(checkPivot.position, checkRadius, -Vector3.up, out hitInfo, checkRadius, groundLayer);
    }

    public Vector3 GetItemPosition()
    {
        return Vector3.zero;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = IsGrounded() ? Color.blue : Color.red;
        Gizmos.DrawSphere(checkPivot.position, checkRadius);
    }
}
