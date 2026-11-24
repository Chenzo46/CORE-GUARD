using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableTutorial", menuName = "Scriptable Objects/ScriptableTutorial")]
public class ScriptableTutorial : ScriptableObject
{
    public List<TutorialNode> tutorials = new List<TutorialNode>();

    public TutorialNode? GetTutorialNode(string tKey)
    {
        foreach(TutorialNode tn in tutorials)
        {
            if (tn.tutorialKey.Equals(tKey))
            {
                return tn;
            }
        }

        return null;
    }

    public int GetTutorialNodeIndex(TutorialNode tn)
    {
        int idx = 0;
        foreach(TutorialNode tnc in tutorials)
        {
            if (tn.isComplete)
                continue;

            int tIdx = idx;

            if(tn.Equals(tnc))
            {
                return tIdx;
            }

            idx++;
        }

        return -1;
    }
}

[System.Serializable]
public struct TutorialNode 
{
    public string tutorialInput; // e.g. W A S D
    public string tutorialDescription; // e.g. to walk
    public string tutorialKey; // simple word for quick lookup
    public string tutorialInputMap; // e.g. Movement, Monitor, UI....
    public bool isComplete;
}

public enum TutorialActionType 
{
    Button,
    Value
}

