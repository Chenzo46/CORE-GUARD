using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoolantInjector : MonoBehaviour
{
    [SerializeField] private CanvasGroup CoolantWarning;
    [SerializeField] private Rotatable _handValve;
    [SerializeField] private TMP_Text _pressureIndicator;
    [SerializeField] private int idealPressureIndex = 3;

    [SerializeField] private CanvasGroup[] pressureIndicator;
    [SerializeField] private Button CoolantInjectButton;

    public Action OnCoolantInjected;
    private bool coolantInjected = false;

    private int pIdx = 0;

    private void Awake()
    {
        CoolantInjectButton.onClick.AddListener(InjectCoolant);
    }

    private void Update()
    {
        //Debug.Log(ConvertValveProgToIndex());

        UpdateLevels();
    }

    private void UpdateLevels()
    {
        int currentIdx = ConvertValveProgToIndex();
        for(int idx = 0; idx < pressureIndicator.Length; idx++)
        {
            pressureIndicator[idx].alpha = idx <= currentIdx ? 1 : 0;
        }

        if (pIdx != currentIdx)
        {
            AudioManager.Singleton.PlaySoundWorldPitchRamp("inject-status", transform.position, 1 + currentIdx * 0.15f);
        }


        if (currentIdx < idealPressureIndex)
        {
            _pressureIndicator.text = "PRESSURE\nLOW";
            _pressureIndicator.color = Color.red;
            CoolantInjectButton.interactable = false;
        }
        else if (currentIdx > idealPressureIndex)
        {
            _pressureIndicator.text = "PRESSURE\nHIGH";
            _pressureIndicator.color = Color.red;
            CoolantInjectButton.interactable = false;
        }
        else
        {
            _pressureIndicator.text = "PRESSURE\nIDEAL";
            _pressureIndicator.color = Color.green;
            CoolantInjectButton.interactable = true;
        }

        pIdx = currentIdx;

    }

    private int ConvertValveProgToIndex()
    {
        float pos = _handValve.GetCurrentThetaDispProg();
        return (int)(pos*(pressureIndicator.Length-1));
    }

    public void HideCoolantWarning()
    {
        GUIManager.ShowCanvasGroupOverlap(CoolantWarning, false);
        _handValve.UpdateIsInteractable(true);
    }

    public void InjectCoolant()
    {
        if (coolantInjected)
            return;

        AudioManager.Singleton.PlaySound("confirm-button");

        coolantInjected = true;
        OnCoolantInjected?.Invoke();
    }
}
