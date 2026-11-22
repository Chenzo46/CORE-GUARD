using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private ScriptableAudioListSettings audioListSettings;
    [SerializeField] private AudioSource _staticAudioSource;
    [SerializeField] private GameObject audioSourcePrefab;

    public static AudioManager Singleton;

    private Dictionary<AudioClip, AudioSource> loopedElements = new Dictionary<AudioClip, AudioSource>();

    private void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Sound Player Methods

    public void PlaySound(string audioName, float audioVolumeAdjust = 1, float pitchVar = 0f)
    {
        AudioClip ac = audioListSettings.GetAudioClip(audioName);
        
        _staticAudioSource.volume = 1f;
        _staticAudioSource.volume = _staticAudioSource.volume * Mathf.Clamp(audioVolumeAdjust, 0f, 1f);

        _staticAudioSource.pitch = 0f;
        _staticAudioSource.pitch = 1f + Random.Range(-pitchVar, pitchVar);

        if (ac)
            _staticAudioSource.PlayOneShot(ac);
    }

    public void PlaySoundPitchRamp(string audioName, float audioVolumeAdjust = 1, float pitchRamp = 1f)
    {
        AudioClip ac = audioListSettings.GetAudioClip(audioName);

        _staticAudioSource.volume = 1f;
        _staticAudioSource.volume = _staticAudioSource.volume * Mathf.Clamp(audioVolumeAdjust, 0f, 1f);

        _staticAudioSource.pitch = pitchRamp;

        if (ac)
            _staticAudioSource.PlayOneShot(ac);
    }

    public void PlaySoundWorld(string audioName, Vector3 audioPosition, float pitchVar = 0, float audioVolumeAdjust = 1)
    {
        AudioClip ac = audioListSettings.GetAudioClip(audioName);

        if (ac)
        {
            AudioSource newAudioSource = Instantiate(audioSourcePrefab, audioPosition, Quaternion.identity).GetComponent<AudioSource>();
            newAudioSource.clip = ac;
            newAudioSource.outputAudioMixerGroup = audioListSettings.GetMixerGroup("SFX");
            newAudioSource.pitch = 1f + Random.Range(-pitchVar, pitchVar);
            newAudioSource.volume = newAudioSource.volume * Mathf.Clamp(audioVolumeAdjust, 0, 1);
            newAudioSource.Play();
            Destroy(newAudioSource.gameObject, ac.length + 0.1f);
        }
    }

    public void PlaySoundWorld(string audioName, string mixerName, Vector3 audioPosition, float pitchVar = 0, float audioVolumeAdjust = 1)
    {
        AudioClip ac = audioListSettings.GetAudioClip(audioName);

        if (ac)
        {
            AudioSource newAudioSource = Instantiate(audioSourcePrefab, audioPosition, Quaternion.identity).GetComponent<AudioSource>();
            newAudioSource.clip = ac;
            newAudioSource.outputAudioMixerGroup = audioListSettings.GetMixerGroup(mixerName);
            newAudioSource.pitch = 1f + Random.Range(-pitchVar, pitchVar);
            newAudioSource.volume = newAudioSource.volume * Mathf.Clamp(audioVolumeAdjust, 0, 1);
            newAudioSource.Play();
            Destroy(newAudioSource.gameObject, ac.length + 0.1f);
        }
    }

    public void PlaySoundWorldPitchRamp(string audioName, Vector3 audioPosition, float pitchRamp = 1f, float audioVolumeAdjust = 1)
    {
        AudioClip ac = audioListSettings.GetAudioClip(audioName);

        if (ac)
        {
            AudioSource newAudioSource = Instantiate(audioSourcePrefab, audioPosition, Quaternion.identity).GetComponent<AudioSource>();
            newAudioSource.clip = ac;
            newAudioSource.outputAudioMixerGroup = audioListSettings.GetMixerGroup("SFX");
            newAudioSource.pitch = pitchRamp;
            newAudioSource.volume = newAudioSource.volume * Mathf.Clamp(audioVolumeAdjust, 0, 1);
            newAudioSource.Play();
            Destroy(newAudioSource.gameObject, ac.length + 0.1f);
        }
    }

    // Loop Player Methods

    public void StartLoop(string audioName, float audioVolumeAdjust = 1)
    {

        AudioClip ac = audioListSettings.GetAudioClip(audioName);

        if (loopedElements.ContainsKey(ac))
            return;
            

        AudioSource ad = Instantiate(audioSourcePrefab).GetComponent<AudioSource>();
        ad.spatialBlend = 0;
        ad.loop = true;
        ad.clip = ac;
        ad.volume = ad.volume * Mathf.Clamp(audioVolumeAdjust, 0, 1);

        loopedElements.Add(ac, ad);
        ad.Play();
    }

    public void StartLoop(string audioName, string mixerName, float audioVolumeAdjust = 1)
    {
        StartLoop(audioName);

        AudioClip ac = audioListSettings.GetAudioClip(audioName);
        AudioSource ad = loopedElements[ac];

        ad.volume = 1f;
        ad.volume = ad.volume * Mathf.Clamp(audioVolumeAdjust, 0, 1);

        ad.outputAudioMixerGroup = audioListSettings.GetMixerGroup(mixerName);
    }

    public void StopLoop(string audioName)
    {
        AudioClip ac = audioListSettings.GetAudioClip(audioName);

        if (!loopedElements.ContainsKey(ac))
            return;

        AudioSource ad = loopedElements[ac];

        ad.DOFade(0f, 0.3f).OnComplete(() =>
        {
            ad.Stop();

            Destroy(ad.gameObject, 0.1f);
        });

        loopedElements.Remove(ac);
    }

    public AudioSource GetFreeAudioSource(string audioName, float audioVolumeAdjust = 1f)
    {
        AudioClip ac = audioListSettings.GetAudioClip(audioName);


        AudioSource ad = Instantiate(audioSourcePrefab).GetComponent<AudioSource>();
        ad.spatialBlend = 0;
        ad.loop = true;
        ad.clip = ac;
        ad.volume = ad.volume * Mathf.Clamp(audioVolumeAdjust, 0, 1);

        ad.Pause();

        return ad;
    }
}

[System.Serializable]

public struct AudioElement
{
    public string audioName;
    public AudioClip audioClip;
}
