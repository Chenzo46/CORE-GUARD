using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Rotatable : MonoBehaviour, IInteractable
{
    [SerializeField] private InputActionAsset _ipa;
    [SerializeField] private Transform _referenceVisual;
    [SerializeField] private Transform grabPointRef;
    [SerializeField] private string context = "Rotatable";
    [SerializeField] private float maxRotateStrength = 50, minRotateStrength = 25;
    [SerializeField] private float maxGrabDistance = 3f;

    private InputActionMap moveActionMap;
    private InputAction grabAction;

    private bool isLooking = false;
    private bool isGrabbing = false;

    private Vector3 previousGrabPos;

    private float currentThetaDisplace = 0f;

    [SerializeField] private float MIN_THETA_DISPLACE = 0f;
    [SerializeField] private float MAX_THETA_DISPLACE = 2000f;

    [SerializeField] private bool infiniteRotate = false;
    [SerializeField] private bool isInteractable = true;

    [Header("Rotate Audio")]
    [SerializeField] private float audioMaxVolume = 1f;
    [SerializeField] private float minAudioSpeed = 5f;
    [SerializeField] private float maxAudioSpeed = 180f;
    [SerializeField] private float audioResponse = 12f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.85f, 1.15f);

    private float lastVisualZ;
    private float smoothedSpeed;
    private AudioSource rotateAudioSource = null;

    private void Awake()
    {
        moveActionMap = _ipa.FindActionMap("Movement");
        grabAction = moveActionMap.FindAction("Use");
    }

    private void Update()
    {
        //transform.Rotate(new Vector3(0, 0, -5) * Time.deltaTime);

        HandleGrab();
    }

    private void HandleGrab()
    {
        if (!isInteractable)
            return;

        if (!isLooking && !isGrabbing)
            return;

        if (grabAction.IsPressed())
        {
            // Get mouse point projection on object's xy plane
            Plane objectPlane = new Plane(transform.forward, transform.position);

            Vector3 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);


            if (objectPlane.Raycast(ray, out float enterDistance))
            {
                // Convert mouse raycast to world position
                Vector3 mouseWorldPos = ray.GetPoint(enterDistance);

                Vector3 localMousePos = transform.InverseTransformPoint(mouseWorldPos);

                grabPointRef.localPosition = localMousePos;

                if (!isGrabbing)
                {
                    previousGrabPos = grabPointRef.localPosition;
                    isGrabbing = true;
                    return;
                }
                    

                // Calculate point's polar coordinates
                Vector3 currentGrabPointPos = grabPointRef.localPosition;

                float t1 = Mathf.Atan2(currentGrabPointPos.y, currentGrabPointPos.x) * Mathf.Rad2Deg;
                float t2 = Mathf.Atan2(previousGrabPos.y, previousGrabPos.x) * Mathf.Rad2Deg;

                // Find difference in theta and apply rotation on the object's z-axis

                float dt = Mathf.DeltaAngle(t2, t1);

                currentThetaDisplace -= dt;

                //Debug.Log(currentThetaDisplace);
                float currentRotateStrength = GetCurrentRotateStrength();


                _referenceVisual.localRotation = Quaternion.Slerp(
                    _referenceVisual.localRotation,
                    Quaternion.Euler(0, 0, _referenceVisual.localRotation.eulerAngles.z + dt),
                    Time.deltaTime * currentRotateStrength
                );
                    
                

                previousGrabPos = grabPointRef.localPosition;

                // Audio Loop

                if (rotateAudioSource == null)
                {
                    rotateAudioSource = AudioManager.Singleton.GetFreeAudioSource("valve-rotate-loop");
                    rotateAudioSource.volume = 0f;
                    rotateAudioSource.Play();
                    lastVisualZ = _referenceVisual.localEulerAngles.z;
                    smoothedSpeed = 0f;
                }

                UpdateRotateAudio();

                // Distance Check
                if (Vector3.Distance(grabPointRef.position, transform.position) > maxGrabDistance)
                {
                    StopRotateAudio();
                    isGrabbing = false;
                    return;
                }
            }
        }
        else
        {
            StopRotateAudio();
            isGrabbing = false;
        }
    }

    private void UpdateRotateAudio()
    {
        float z = _referenceVisual.localEulerAngles.z;
        float visualSpeed = Mathf.Abs(Mathf.DeltaAngle(lastVisualZ, z)) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastVisualZ = z;

        // Frame-rate independent smoothing so tiny hitches don't make the sound flutter
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, visualSpeed, 1f - Mathf.Exp(-audioResponse * Time.deltaTime));

        float t = Mathf.InverseLerp(minAudioSpeed, maxAudioSpeed, smoothedSpeed);

        rotateAudioSource.volume = t * audioMaxVolume;
        rotateAudioSource.pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, t);
    }

    private void StopRotateAudio()
    {
        if (rotateAudioSource == null)
            return;

        AudioSource src = rotateAudioSource;
        rotateAudioSource = null;

        src.DOKill();
        src.DOFade(0f, 0.3f).OnComplete(() =>
        {
            if (src != null)
                Destroy(src.gameObject);
        });
    }

    private void OnDisable()
    {
        StopRotateAudio();
    }

    private float GetCurrentRotateStrength()
    {
        if (infiniteRotate)
            return maxRotateStrength;

        if (currentThetaDisplace < MIN_THETA_DISPLACE) // clamp Rotation negative
        {
            currentThetaDisplace = 0;
            _referenceVisual.localRotation = Quaternion.Euler(Vector3.zero);
            return maxRotateStrength;
        }
        else if(currentThetaDisplace > MAX_THETA_DISPLACE)
        {
            currentThetaDisplace = MAX_THETA_DISPLACE;
            return 0;
        }

        return Mathf.Lerp(maxRotateStrength, minRotateStrength, GetCurrentThetaDispProg());
    }

    public float GetCurrentThetaDispProg()
    {
        return Mathf.Clamp(currentThetaDisplace / MAX_THETA_DISPLACE, 0, 1);
    }

    public void Interact(PlayerBase plr)
    {
        Debug.Log("Click and drag to rotate this object");
    }

    public string GetContext()
    {
        return context;
    }

    public void SetContext(string ctx)
    {
        context = ctx;
    }

    public void UpdateIsLooking(bool isLooking)
    {
        this.isLooking = isLooking;
    }

    public void UpdateIsInteractable(bool inter)
    {
        isInteractable = inter;
    }

    public void OnDrawGizmos()
    {
        /*
        Gizmos.color = isGrabbing ? Color.green : Color.red;

        
        Gizmos.DrawSphere(grabPointRef.position, 0.1f);
        */
    }
}
