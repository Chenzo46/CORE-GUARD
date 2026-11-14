using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class CoolantSynthesizer : MonoBehaviour
{
    // Water, Fire, Ice
    [SerializeField] private RectTransform coolantResultant; // Maybe make color average of material input color
    [SerializeField] private float resultSizeMultipler = 1.2f;
    [SerializeField] private Button[] materialButtons = new Button[3];
    [SerializeField] private GameObject[] materialIconPrefabs = new GameObject[3];
    [SerializeField] private TMP_Text[] recipeLabels = new TMP_Text[3];

    private Dictionary<CoolantMaterialType, int> materialInputs = new Dictionary<CoolantMaterialType, int>();
    private Dictionary<CoolantMaterialType, int> coolantRecipe = new Dictionary<CoolantMaterialType, int>();

    [SerializeField] private List<CoolantMaterialType> recipeAsList = new List<CoolantMaterialType>();

    private enum CoolantMaterialType
    {
        Water,
        Fire,
        Ice
    }

    private void Awake()
    {
        InitButtons();
        ConvertListToRecipe();
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
                return;
            }
        }

        Debug.Log("Coolant Finalized");
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
}
