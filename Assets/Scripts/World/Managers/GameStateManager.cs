using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Singleton;

    private void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else 
        {
            // for DontDestroyOnLoad
        }
    }
}
