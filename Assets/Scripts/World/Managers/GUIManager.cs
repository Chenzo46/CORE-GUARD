using TMPro;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text interactCtxLabel;
    public static GUIManager Singleton;

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            // for DontDestroyOnLoad
        }
    }

    public void SetInteractableContext(string context)
    {
        interactCtxLabel.text = context;
    }
}
