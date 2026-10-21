using UnityEngine;
using UnityEngine.InputSystem;

public class MonitorScreen : Interactable
{
    [Header("Monitor Properties")]
    [SerializeField] private InputActionAsset _ipa;
    [SerializeField] private Transform Cursor;
    [SerializeField] private Transform cameraPivotPoint;

    private InputActionMap monitorActionMap, movementActionMap;
    private InputAction dragCursorAction;
    private InputAction leftClickAction;
    private InputAction leaveAction;

    private bool isFoucsed = false;

    public override void Awake()
    {
        base.Awake();

        InitializeInputActions();
    }

    private void OnEnable()
    {
        leaveAction.performed += LeaveMonitor;
    }

    private void OnDisable()
    {
        leaveAction.performed -= LeaveMonitor;
    }

    private void InitializeInputActions()
    {
        monitorActionMap = _ipa.FindActionMap("Monitor");
        movementActionMap = _ipa.FindActionMap("Movement");

        dragCursorAction = monitorActionMap.FindAction("Drag");
        leftClickAction = monitorActionMap.FindAction("Left Click");
        leaveAction = monitorActionMap.FindAction("Leave");

        monitorActionMap.Disable();
    }

    public override void InteractFunction(PlayerBase sender)
    {
        base.InteractFunction(sender);

        EnterMonitor();
    }

    private void EnterMonitor()
    {
        movementActionMap.Disable();
        monitorActionMap.Enable();

        isFoucsed = true;

        Camera.main.transform.position = cameraPivotPoint.position;
        Camera.main.transform.rotation = cameraPivotPoint.rotation;
    }

    private void LeaveMonitor(InputAction.CallbackContext ctx)
    {
        if(ctx.performed && isFoucsed)
        {
            movementActionMap.Enable();
            monitorActionMap.Disable();
            // set cursor position back to center
            // tween main camera back to player
            isFoucsed = false;
        }
    }

}
