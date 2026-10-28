using NUnit.Framework;
using UnityEngine;

public class EMFPuzzlePiece : MonoBehaviour
{
    private RectTransform _rt;
    private RectTransform CursorObject = null;
    private EMFPuzzle puzzleParent;
    [SerializeField] private NodeConnectionType[] connections = new NodeConnectionType[3]; // Will be read clockwise from the rightmost side
    private int connectionStepDirection = 1; // used to reverse the connections if puzzle piece is rotated 180 degrees

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        puzzleParent = transform.parent.parent.GetComponent<EMFPuzzle>();
    }

    private void Update()
    {
        if (CursorObject == null)
            return;

        _rt.anchoredPosition = CursorObject.anchoredPosition;
    }

    public void HandleOnClick()
    {
        if(CursorObject == null)
        {
            CursorObject = puzzleParent.GetCursorObject();
        }
        else
        {
            CursorObject = null;
            // Player is releasing puzzle piece
            // if piece is over a slot and slot can take piece, place onto slot
            // if piece is not over slot or slot is invalid, tween back to original anchored position
        }
    }

    public NodeConnectionType[] GetNodeConnections()
    {
        return connections;
    }
    public int GetConnectionStep()
    {
        return connectionStepDirection;
    }

    public enum NodeConnectionType
    {
        ONE,
        TWO,
        THREE,
        FOUR,
    }
}
