using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private List<SaveDataFloat> savedFloats = new List<SaveDataFloat>();
    private void Awake()
    {
        InitalizeSaveData();
    }

    private void InitalizeSaveData()
    {
        for(int idx = 0; idx < savedFloats.Count; idx++)
        {
            int tIdx = idx;
            SaveDataFloat sdf = savedFloats[tIdx];

            if (!PlayerPrefs.HasKey(sdf.key))
            {
                PlayerPrefs.SetFloat(sdf.key, sdf.value);
            }
            else
            {
                sdf.value = PlayerPrefs.GetFloat(sdf.key);
            }

            savedFloats[tIdx] = sdf;
        }

        PlayerPrefs.Save();
    }

    public static void UpdateSavedFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);

        PlayerPrefs.Save();
    }
}

[System.Serializable]
public struct SaveDataFloat 
{
    public string key;
    public float value;
}

