using TMPro;
using UnityEngine;

public class CoolantInjector : MonoBehaviour
{
    [SerializeField] private Rotatable _handValve;
    [SerializeField] private TMP_Text _pressureIndicator;
    [SerializeField] private int idealPressureIndex = 3;

    [SerializeField] private CanvasGroup[] pressureIndicator;

    private void Awake()
    {
        
    }

    private void Update()
    {
        Debug.Log(ConvertValveProgToIndex());

        UpdateLevels();
    }

    private void UpdateLevels()
    {
        int currentIdx = ConvertValveProgToIndex();
        for(int idx = 0; idx < pressureIndicator.Length; idx++)
        {
            pressureIndicator[idx].alpha = idx <= currentIdx ? 1 : 0;
        }


        if (currentIdx < idealPressureIndex)
        {
            _pressureIndicator.text = "PRESSURE\nLOW";
            _pressureIndicator.color = Color.red;
        }
        else if (currentIdx > idealPressureIndex)
        {
            _pressureIndicator.text = "PRESSURE\nHIGH";
            _pressureIndicator.color = Color.red;
        }
        else
        {
            _pressureIndicator.text = "PRESSURE\nIDEAL";
            _pressureIndicator.color = Color.green;
        }

    }

    private int ConvertValveProgToIndex()
    {
        float pos = _handValve.GetCurrentThetaDispProg();
        return (int)(pos*(pressureIndicator.Length-1));
    }
}
