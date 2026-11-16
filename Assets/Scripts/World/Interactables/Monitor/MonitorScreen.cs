using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MonitorScreen : Interactable
{
    [Header("Monitor Properties")]
    [SerializeField] private InputActionAsset _ipa;
    [SerializeField] private MeshRenderer _monitorMr;
    [SerializeField] private RectTransform cursor;
    [SerializeField] private Transform cameraPivotPoint;
    [SerializeField] private Material offMat;
    [SerializeField] private Material onMat;
    [SerializeField] private bool IsMonitorAwake = true;

    [Header("Virtual Cursor")]
    [SerializeField] private Canvas monitorCanvas;
    [SerializeField] private float sensitivity = 0.5f;
    [SerializeField] private float gamepadSensitivityScale = 2000;
    private InputActionMap monitorActionMap, movementActionMap;
    private InputAction dragCursorAction;
    private InputAction leftClickAction;
    private InputAction leaveAction;

    private RectTransform canvasRect;
    private GraphicRaycaster raycaster;
    private PointerEventData pointer;
    private readonly List<RaycastResult> results = new();

    private Camera renderCamera;                    
    private InputSystemUIInputModule uiModule;
    private GameObject hitObject;
    private GameObject hovered; 
    private GameObject pressed;

    private bool isFoucsed = false;

    private Sequence MonitorCameraTween;

    private PlayerBase currentViewer;
    private Vector3 orgCamRot;

    public Action OnMonitorEnter;
    public Action OnMonitorExit;

    private bool isPluggedIn = true;

    public override void Awake()
    {
        base.Awake();

        InitializeInputActions();

        canvasRect = monitorCanvas.GetComponent<RectTransform>();
        raycaster = monitorCanvas.GetComponent<GraphicRaycaster>();
        renderCamera = monitorCanvas.worldCamera;
        uiModule = FindFirstObjectByType<InputSystemUIInputModule>();

        if (renderCamera == null)
            Debug.LogError("MonitorScreen: set the monitor canvas's Render Camera to the RenderTexture camera.", this);

        foreach (Graphic g in cursor.GetComponentsInChildren<Graphic>())
            g.raycastTarget = false;

        if (!IsMonitorAwake)
            _monitorMr.material = offMat;
    }

    private void OnEnable()
    {
        leaveAction.performed += LeaveMonitor;
        leftClickAction.started += OnClickDown;
        leftClickAction.canceled += OnClickUp;
    }

    private void OnDisable()
    {
        leaveAction.performed -= LeaveMonitor;
        leftClickAction.started -= OnClickDown;
        leftClickAction.canceled -= OnClickUp;
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
        if (!IsMonitorAwake)
            return;

        base.InteractFunction(sender);

        currentViewer = sender;
        orgCamRot = Camera.main.transform.rotation.eulerAngles;
        EnterMonitor();
    }

    private void EnterMonitor()
    {
        movementActionMap.Disable();
        monitorActionMap.Enable();
        GUIManager.Singleton.SwitchToMonitorHUD();
        cursor.gameObject.SetActive(true);

        isFoucsed = true;

        if (uiModule) uiModule.enabled = false;

        //Camera.main.transform.position = cameraPivotPoint.position;
        //Camera.main.transform.rotation = cameraPivotPoint.rotation;
        MoveCameraToPivot();

        pointer = new PointerEventData(EventSystem.current);
        cursor.anchoredPosition = Vector2.zero;

        OnMonitorEnter?.Invoke();
    }

    private void LeaveMonitor(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && isFoucsed && !MonitorCameraTween.IsPlaying())
        {
            MoveCameraToPlayer();

            ReleaseAll();
            if (uiModule) uiModule.enabled = true;
            GUIManager.Singleton.SwitchToBaseHUD();
            cursor.gameObject.SetActive(false);

            OnMonitorEnter?.Invoke();

            MonitorCameraTween.OnComplete(() =>
            {
                movementActionMap.Enable();
                monitorActionMap.Disable();
                cursor.anchoredPosition = Vector2.zero;
                // tween main camera back to player
                isFoucsed = false;
            });
        }
    }

    private void MoveCameraToPivot()
    {
        MonitorCameraTween?.Kill();

        MonitorCameraTween = DOTween.Sequence();

        MonitorCameraTween.Append(Camera.main.transform.DOMove(cameraPivotPoint.position, 0.3f).SetAutoKill(false));
        MonitorCameraTween.Join(Camera.main.transform.DORotate(cameraPivotPoint.rotation.eulerAngles, 0.3f).SetAutoKill(false));

        MonitorCameraTween.SetAutoKill(false);
        MonitorCameraTween.Play();
    }

    private void MoveCameraToPlayer()
    {
        MonitorCameraTween?.Kill();

        MonitorCameraTween = DOTween.Sequence();

        Vector3 camEyeLevelPos = currentViewer.GetEyeLevelTransform().position;

        MonitorCameraTween.Append(Camera.main.transform.DOMove(camEyeLevelPos, 0.3f).SetAutoKill(false));
        MonitorCameraTween.Join(Camera.main.transform.DORotate(orgCamRot, 0.3f).SetAutoKill(false));

        MonitorCameraTween.SetAutoKill(false);
        MonitorCameraTween.Play();
    }

    private void Update()
    {
        if (!isFoucsed) return;

        Vector2 delta;
        InputDevice device = dragCursorAction.activeControl?.device;

        if (device is Gamepad)
            delta = dragCursorAction.ReadValue<Vector2>() * sensitivity * gamepadSensitivityScale * Time.deltaTime;
        else
            delta = dragCursorAction.ReadValue<Vector2>() * sensitivity;

        cursor.anchoredPosition += delta;
        ClampCursor();

        UpdateHover(delta);
    }

    private void ClampCursor()
    {
        Rect r = canvasRect.rect;
        Vector2 p = cursor.anchoredPosition;
        p.x = Mathf.Clamp(p.x, r.xMin, r.xMax);
        p.y = Mathf.Clamp(p.y, r.yMin, r.yMax);
        cursor.anchoredPosition = p;
    }


    private void UpdateHover(Vector2 delta)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(renderCamera, cursor.position);

        pointer.position = screenPoint;
        pointer.delta = delta;

        results.Clear();
        raycaster.Raycast(pointer, results);

        if (results.Count > 0)
        {
            pointer.pointerCurrentRaycast = results[0];
            hitObject = results[0].gameObject;
        }
        else
        {
            pointer.pointerCurrentRaycast = new RaycastResult();
            hitObject = null;
        }

        GameObject target = hitObject ? ExecuteEvents.GetEventHandler<IPointerEnterHandler>(hitObject) : null;

        if (target != hovered)
        {
            ExitHover();
            hovered = target;
            if (hovered) ExecuteEvents.Execute(hovered, pointer, ExecuteEvents.pointerEnterHandler);
        }
    }

    private void ExitHover()
    {
        if (hovered) ExecuteEvents.Execute(hovered, pointer, ExecuteEvents.pointerExitHandler);
        hovered = null;
    }

    private void OnClickDown(InputAction.CallbackContext ctx)
    {
        if (!isFoucsed || hitObject == null) return;

        pointer.pressPosition = pointer.position;
        pointer.pointerPressRaycast = pointer.pointerCurrentRaycast;
        pointer.eligibleForClick = true;

        pressed = ExecuteEvents.ExecuteHierarchy(hitObject, pointer, ExecuteEvents.pointerDownHandler);
        if (pressed == null)
            pressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hitObject);

        pointer.pointerPress = pressed;
        pointer.rawPointerPress = hitObject;
    }

    private void OnClickUp(InputAction.CallbackContext ctx)
    {
        if (!isFoucsed || pressed == null) return;

        ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerUpHandler);

        // Only a click if released over the same thing we pressed
        GameObject clickTarget = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hitObject);
        if (clickTarget == pressed)
            ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerClickHandler);

        pressed = null;
        pointer.pointerPress = null;
        pointer.rawPointerPress = null;
        pointer.eligibleForClick = false;
    }

    private void ReleaseAll()
    {
        if (pressed != null)
        {
            ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerUpHandler);
            pressed = null;
        }
        ExitHover();
        hitObject = null;
    }

    public void TurnMonitorOn()
    {
        if(IsMonitorAwake || !isPluggedIn)
            return;

        IsMonitorAwake = true;
        _monitorMr.material = onMat;
    }

    public void SetPluggedIn()
    {
        isPluggedIn = true;
    }
}