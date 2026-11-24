using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private InputActionAsset _ipa;
    [SerializeField] private CanvasGroup settingsGroup;
    [SerializeField] private Button backButton;

    [SerializeField] private bool openWithButton = true;

    [Header("Sliders")]
    [SerializeField] private Slider lookSense;
    [SerializeField] private Slider monitorSense, masterVol, musicVol, sfxVol;
    [SerializeField] private AudioMixer masterMixer;

    private InputActionMap movementMap;
    private InputActionMap monitorMap;
    private InputActionMap uiMap;

    private bool opened = false;

    private void Awake()
    {
        InitBackButton();

        movementMap = _ipa.FindActionMap("Movement");
        uiMap = _ipa.FindActionMap("UI");
        monitorMap = _ipa.FindActionMap("Monitor");
    }

    private void Start()
    {
        InitSettingsValues();
        backButton.GetComponent<ButtonMouseSizer>().SetParentCanvas(transform.parent.GetComponent<Canvas>());
    }

    private void Update()
    {
        if(uiMap.FindAction("Settings").WasPressedThisFrame() && !opened && openWithButton)
        {
            GUIManager.FadeCanvasGroupUnscaledTime(settingsGroup, 1f, 0.3f);
            opened = true;
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            movementMap.Disable();
        }
        else if (uiMap.FindAction("Settings").WasPressedThisFrame() && opened && openWithButton)
        {
            GUIManager.FadeCanvasGroupUnscaledTime(settingsGroup, 0f, 0.3f);
            opened = false;
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            movementMap.Enable();
        }
    }

    private void InitBackButton()
    {
        backButton.onClick.AddListener(() =>
        {
            GUIManager.FadeCanvasGroupUnscaledTime(settingsGroup, 0f, 0.3f);
            opened = false;
            Time.timeScale = 1f;

            if (openWithButton)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                movementMap.Enable();
            }
        });
    }

    public void OpenSettings()
    {
        GUIManager.FadeCanvasGroupUnscaledTime(settingsGroup, 1f, 0.3f);
        opened = true;
        Time.timeScale = 0f;
    }

    private void InitSettingsValues()
    {
        lookSense.value = PlayerPrefs.GetFloat("look_sense");
        monitorSense.value = PlayerPrefs.GetFloat("monitor_sense");
        masterVol.value = PlayerPrefs.GetFloat("master_vol");
        musicVol.value = PlayerPrefs.GetFloat("music_vol");
        sfxVol.value = PlayerPrefs.GetFloat("sfx_vol");

        lookSense.onValueChanged.AddListener((float val) => { SaveManager.UpdateSavedFloat("look_sense", val); });
        monitorSense.onValueChanged.AddListener((float val) => { SaveManager.UpdateSavedFloat("monitor_sense", val); });
        masterVol.onValueChanged.AddListener((float val) => { SaveManager.UpdateSavedFloat("master_vol", val); UpdateMixerVolumes("master_vol", val); });
        musicVol.onValueChanged.AddListener((float val) => { SaveManager.UpdateSavedFloat("music_vol", val); UpdateMixerVolumes("music_vol", val); });
        sfxVol.onValueChanged.AddListener((float val) => { SaveManager.UpdateSavedFloat("sfx_vol", val); UpdateMixerVolumes("sfx_vol", val); });

        UpdateMixerVolumes("master_vol");
        UpdateMixerVolumes("music_vol");
        UpdateMixerVolumes("sfx_vol");
    }

    public void UpdateMixerVolumes(string groupStr, float? val = null)
    {
        float toSet = 0;

        if(val != null)
        {
            toSet = (float)val;
        }
        else
        {
            toSet = PlayerPrefs.GetFloat(groupStr);
        }

        float dB = Mathf.Log10(toSet) * 20f;

        masterMixer.SetFloat(groupStr.Split('_')[0], dB);
    }
}
