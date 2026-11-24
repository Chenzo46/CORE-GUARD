using UnityEngine;

public class SkyManager : MonoBehaviour
{
    public float spinSpeed;

    // Update is called once per frame
    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * spinSpeed);
    }
}
