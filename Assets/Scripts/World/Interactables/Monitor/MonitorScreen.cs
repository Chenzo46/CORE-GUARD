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
    [SerializeField] private bool isPluggedIn = true;

    [Header("Virtual Cursor")]
    [SerializeField] private Canvas monitorCanvas;
    [SerializeField] private float sensitivity = 0.5f;
    [SerializeField] private float gamepadSensitivityScale = 2000;
    [SerializeField, Range(0.1f, 1f)] private float hitboxScale = 0.5f;
    [SerializeField, Range(1, 5)] private int samplesPerAxis = 3;

    private GraphicRaycaster raycaster;
    private readonly List<RaycastResult> raycastResults = new();
    private InputActionMap monitorActionMap, movementActionMap;
    private InputAction dragCursorAction;
    private InputAction leftClickAction;
    private InputAction leaveAction;

    private RectTransform canvasRect;

    private PointerEventData pointer;

    private Camera renderCamera;                    
    private InputSystemUIInputModule uiModule;
    private GameObject pressed;

    private List<Selectable> selectables = new();
    private HashSet<GameObject> hoveredSet = new();
    private HashSet<GameObject> nextHovered = new();
    private List<GameObject> exitBuffer = new();
    private Vector3[] corners = new Vector3[4];
    private GameObject clickTarget;

    private bool isFoucsed = false;

    private Sequence MonitorCameraTween;

    private PlayerBase currentViewer;
    private Vector3 orgCamRot;

    public Action OnMonitorEnter;
    public Action OnMonitorExit;
    public Action OnMonitorAwake;

    

    public override void Awake()
    {
        base.Awake();

        InitializeInputActions();

        canvasRect = monitorCanvas.GetComponent<RectTransform>();
        renderCamera = monitorCanvas.worldCamera;
        uiModule = FindFirstObjectByType<InputSystemUIInputModule>();

        raycaster = monitorCanvas.GetComponent<GraphicRaycaster>();

        RefreshSelectables();

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

    public void RefreshSelectables()
    {
        selectables.Clear();
        monitorCanvas.GetComponentsInChildren(true, selectables);
    }

    private Rect GetCanvasSpaceRect(RectTransform rt)
    {
        rt.GetWorldCorners(corners);
        Vector3 min = canvasRect.InverseTransformPoint(corners[0]);
        Vector3 max = canvasRect.InverseTransformPoint(corners[2]);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private Rect GetCursorHitbox()
    {
        Rect r = GetCanvasSpaceRect(cursor);
        Vector2 center = r.center;
        r.size *= hitboxScale;
        r.center = center;
        return r;
    }

    private static Rect Intersection(Rect a, Rect b)
    {
        return Rect.MinMaxRect(
            Mathf.Max(a.xMin, b.xMin), Mathf.Max(a.yMin, b.yMin),
            Mathf.Min(a.xMax, b.xMax), Mathf.Min(a.yMax, b.yMax));
    }
    
    private bool IsTopAtAnySample(Selectable s, Rect area)
    {
        for (int ix = 0; ix < samplesPerAxis; ix++)
        {
            for (int iy = 0; iy < samplesPerAxis; iy++)
            {
                Vector2 local = new Vector2(
                    Mathf.Lerp(area.xMin, area.xMax, (ix + 0.5f) / samplesPerAxis),
                    Mathf.Lerp(area.yMin, area.yMax, (iy + 0.5f) / samplesPerAxis));

                Vector3 world = canvasRect.TransformPoint(local);
                pointer.position = RectTransformUtility.WorldToScreenPoint(renderCamera, world);

                raycastResults.Clear();
                raycaster.Raycast(pointer, raycastResults);
                if (raycastResults.Count == 0) continue;

                Selectable owner = raycastResults[0].gameObject.GetComponentInParent<Selectable>();
                if (owner == s) return true;
            }
        }
        return false;
    }

    private void UpdateHover(Vector2 delta)
    {
        Vector2 cursorScreenPoint = RectTransformUtility.WorldToScreenPoint(renderCamera, cursor.position);
        pointer.delta = delta;

        Rect hitbox = GetCursorHitbox();

        nextHovered.Clear();
        clickTarget = null;
        float closest = float.MaxValue;

        foreach (Selectable s in selectables)
        {
            if (!s || !s.gameObject.activeInHierarchy || !s.IsInteractable()) continue;

            Rect r = GetCanvasSpaceRect((RectTransform)s.transform);
            if (!r.Overlaps(hitbox)) continue;

            if (!IsTopAtAnySample(s, Intersection(r, hitbox))) continue;

            nextHovered.Add(s.gameObject);

            float dist = (r.center - hitbox.center).sqrMagnitude;
            if (dist < closest) { closest = dist; clickTarget = s.gameObject; }
        }

        pointer.position = cursorScreenPoint;

        exitBuffer.Clear();
        foreach (GameObject go in hoveredSet)
            if (!go || !nextHovered.Contains(go)) exitBuffer.Add(go);

        foreach (GameObject go in exitBuffer)
        {
            hoveredSet.Remove(go);
            if (go) ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerExitHandler);
        }

        foreach (GameObject go in nextHovered)
            if (hoveredSet.Add(go))
                ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerEnterHandler);
    }
    private void OnClickDown(InputAction.CallbackContext ctx)
    {
        if (!isFoucsed || clickTarget == null) return;

        pointer.pressPosition = pointer.position;
        pointer.eligibleForClick = true;

        pressed = clickTarget;
        pointer.pointerPress = pressed;
        pointer.rawPointerPress = pressed;

        ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerDownHandler);
    }

    private void OnClickUp(InputAction.CallbackContext ctx)
    {
        if (!isFoucsed || pressed == null) return;

        ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerUpHandler);

        if (hoveredSet.Contains(pressed))
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

        foreach (GameObject go in hoveredSet)
            if (go) ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerExitHandler);

        hoveredSet.Clear();
        clickTarget = null;
    }

    public void TurnMonitorOn()
    {
        if(IsMonitorAwake || !isPluggedIn)
            return;

        IsMonitorAwake = true;
        _monitorMr.material = onMat;
        OnMonitorAwake?.Invoke();
    }

    public void SetPluggedIn()
    {
        isPluggedIn = true;
    }
}