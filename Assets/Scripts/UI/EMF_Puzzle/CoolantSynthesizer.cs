using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class CoolantSynthesizer : MonoBehaviour
{
    [SerializeField] private MonitorScreen _monitorScreen;
    [Header("Synthesizer Variables/Settings")]
    // Water, Fire, Ice
    [SerializeField] private RectTransform coolantResultant; // Maybe make color average of material input color
    [SerializeField] private float resultSizeMultipler = 1.2f;
    [SerializeField] private Button[] materialButtons = new Button[3];
    [SerializeField] private GameObject[] materialIconPrefabs = new GameObject[3];
    [SerializeField] private TMP_Text[] recipeLabels = new TMP_Text[3];
    [SerializeField] private List<CoolantMaterialType> recipeAsList = new List<CoolantMaterialType>();

    [Header("Screen Canvas Groups")]

    [SerializeField] private CanvasGroup BootScreen;
    [SerializeField] private CanvasGroup SynthScreen;
    [SerializeField] private CanvasGroup CompleteScreen;
    [SerializeField] private CanvasGroup PopupScreen;
    [SerializeField] private CanvasGroup PopupWindow;
    [SerializeField] private Button closePopButton;


    // Boot screen variables
    [Header("Boot Screen Variables")]
    [SerializeField] private Image loadingSlider;
    [SerializeField] private CanvasGroup QuikBewtGroup;
    [SerializeField] private TMP_Text bootCtx;
    [SerializeField] private InputActionAsset _ipa;
    [SerializeField] private float bootTime = 5f;
    [SerializeField] private float quikBootPopTime = 1f;
    [SerializeField] private float quikBootTimeWindow = 1f;

    private Dictionary<CoolantMaterialType, int> materialInputs = new Dictionary<CoolantMaterialType, int>();
    private Dictionary<CoolantMaterialType, int> coolantRecipe = new Dictionary<CoolantMaterialType, int>();

    private bool loading = false;
    private bool synthOpen = false;

    private InputActionMap monitorActionMap;
    private InputAction quickBootAction;

    private CoolantStatus coolantStatus = CoolantStatus.Incomplete;

    private enum CoolantMaterialType
    {
        Water,
        Fire,
        Ice
    }

    public enum CoolantStatus
    {
        Incomplete,
        Incorrect,
        Complete
    }

    private void Awake()
    {
        InitButtons();
        ConvertListToRecipe();

        monitorActionMap = _ipa.FindActionMap("Monitor");
        quickBootAction = monitorActionMap.FindAction("Quick Boot");
    }

    private void OnEnable()
    {
        _monitorScreen.OnMonitorEnter += BeginLoading;
    }

    private void OnDisable()
    {
        _monitorScreen.OnMonitorEnter -= BeginLoading;
    }

    private void InitButtons()
    {
        int idx = 0;
        foreach (Button b in materialButtons)
        {
            int tIdx = idx;
            b.onClick.AddListener(() => AddMaterialToCoolant((CoolantMaterialType)tIdx));
            idx++;
        }
    }

    private void AddMaterialToCoolant(CoolantMaterialType type)
    {   
        Debug.Log("Adding Material: " + type.ToString());

        if (materialInputs.ContainsKey(type))
        {
            materialInputs[type]++;
        }
        else
        {
            materialInputs[type] = 1;
        }

        // Send Visual
        SendCoolantVisual();

    }
    /*
     * Checks if materialInputs matches the coolantRecipe, if so, finalize the coolant and proceed to next task
     * Otherwise, Tigger the lose condition
     */
    public void FinalizeCoolant()
    {
        foreach (CoolantMaterialType type in coolantRecipe.Keys)
        {
            if (!materialInputs.ContainsKey(type) || materialInputs[type] != coolantRecipe[type])
            {
                Debug.Log("Lose Condition Triggered");
                coolantStatus = CoolantStatus.Incorrect;
                return;
            }
        }

        GUIManager.FadeCanvasGroup(CompleteScreen, 1f, 0.1f);
        coolantStatus = CoolantStatus.Complete;
        Debug.Log("Coolant Finalized");
    }

    public CoolantStatus GetCoolantStatus()
    {
        return coolantStatus;
    }

    private void ConvertListToRecipe()
    {
        foreach(CoolantMaterialType type in recipeAsList)
        {
            if (coolantRecipe.ContainsKey(type))
            {
                coolantRecipe[type]++;
            }
            else
            {
                coolantRecipe[type] = 1;
            }

            recipeLabels[(int)type].text = coolantRecipe[type].ToString();
        }
    }

    private void SendCoolantVisual()
    {
        Sequence addToResult = DOTween.Sequence();

        addToResult.Append(coolantResultant.DOScale(coolantResultant.localScale * resultSizeMultipler, 0.33f).SetEase(Ease.OutBounce));
    }

    private void BeginLoading()
    {
        if (loading || synthOpen)
            return;

        loading = true;
        bootCtx.text = "Loading Synthesizer...";

        // Show/Start loading progress bar
        // Use Coroutine to progress bar and prompt quik bewt
        StartCoroutine(LoadSystemRoutine());
    }

    private IEnumerator LoadSystemRoutine()
    {
        float prog = 0;

        float timeElapsed = 0;

        bool quickBootShowing = false;

        while (true)
        {
            prog = timeElapsed / bootTime;

            loadingSlider.fillAmount = prog;

            bootCtx.text = $"Loading Synthesizer... {(int)(prog*100f)}%";

            timeElapsed += Time.deltaTime;

            if (timeElapsed >= bootTime)
                break;

            quickBootShowing = timeElapsed >= quikBootPopTime && timeElapsed <= quikBootPopTime + quikBootTimeWindow;
            GUIManager.ShowCanvasGroupOverlap(QuikBewtGroup, quickBootShowing);

            
            if (quickBootShowing && quickBootAction.WasPerformedThisFrame())
            {
                prog = 1f;
                break;
            }

            yield return null;
        }

        GUIManager.ShowCanvasGroupOverlap(QuikBewtGroup, false);
        loadingSlider.fillAmount = prog;
        bootCtx.text = "Loading Synthesizer... Done!";

        // finalize boot
        yield return new WaitForSeconds(0.1f);
        GUIManager.FadeCanvasGroup(BootScreen, 0f, 0.3f);
        synthOpen = true;

        yield return new WaitForSeconds(0.3f);

        // Open Popup Window
        GUIManager.FadeCanvasGroup(PopupScreen, 1f, 0.3f);

        RectTransform popupRt = PopupWindow.GetComponent<RectTransform>();

        popupRt.localScale = Vector3.zero;
        popupRt.DOScale(1f, 0.3f);

        closePopButton.onClick.AddListener(() =>
        {
            popupRt.DOScale(0f, 0.3f).OnComplete(() =>
            {
                GUIManager.FadeCanvasGroup(PopupScreen, 0f, 0.3f);
            });

        });
    }
}
