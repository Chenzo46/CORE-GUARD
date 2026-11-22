using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "ScriptableAudioListSettings", menuName = "Scriptable Objects/ScriptableAudioListSettings")]
public class ScriptableAudioListSettings : ScriptableObject
{
    public List<AudioElement> audioList = new List<AudioElement>();
    public List<MixerGroupElement> mixerGroupList = new List<MixerGroupElement>();

    public AudioClip GetAudioClip(string audioName)
    {
        foreach (AudioElement ae in audioList)
        {
            if (ae.audioName.Equals(audioName))
            {
                return ae.audioClip;
            }
        }

        return null;
    }

    public AudioMixerGroup GetMixerGroup(string mixerGroupName)
    {
        foreach (MixerGroupElement mge in mixerGroupList)
        {
            if (mge.mixerGroupName.Equals(mixerGroupName))
            {
                return mge.mixerGroup;
            }
        }

        return null;
    }
}

[System.Serializable]
public struct MixerGroupElement 
{
    public string mixerGroupName;
    public AudioMixerGroup mixerGroup;
}

