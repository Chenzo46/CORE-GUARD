using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceTimeSync : MonoBehaviour
{
    private AudioSource ad;

    private bool paused = false;

    private void Awake()
    {
        ad = GetComponent<AudioSource>();
    }


    private void Update()
    {
        if (!ad.isPlaying && !paused)
            return;


        if(Time.timeScale <= 0 && !paused)
        {
            paused = true;
            ad.Pause();
        }
        else if(Time.timeScale != 0 && paused)
        {
            paused = false;
            ad.Play();
        }
    }
}
