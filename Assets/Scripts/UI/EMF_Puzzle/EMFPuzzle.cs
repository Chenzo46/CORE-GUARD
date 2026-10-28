using System.Collections.Generic;
using UnityEngine;

public class EMFPuzzle : MonoBehaviour
{
    [SerializeField] private RectTransform CursorObject;
    [SerializeField] private MonitorScreen _monitor;

    // Maybe we can generate a puzzle based off of a root prefab??
    [SerializeField] private GameObject EMFPuzzleRootPrefab;
    [SerializeField] private GameObject EMFPuzzleSlotPrefab;
    [SerializeField] private int puzzleDepth = 1;
    [SerializeField] private float slotDistance = 1;
    private EMFPuzzlePiece EMFPuzzleRoot;

    public static EMFPuzzle Singleton;

    //private List<EMFPuzzleSlot> puzzleSlots = new List<EMFPuzzleSlot>();
    private EMFPuzzleSlot rootPuzzleSlot = new EMFPuzzleSlot();

    private List<Vector2> traveledPositions = new List<Vector2>();

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        //InitializeEMFPuzzle();
    }

    public RectTransform GetCursorObject()
    {
        return CursorObject;
    }

    private void InitializeEMFPuzzle()
    {
        EMFPuzzleRoot = Instantiate(EMFPuzzleRootPrefab, Vector2.zero, Quaternion.identity, transform).GetComponent<EMFPuzzlePiece>();

        rootPuzzleSlot.slot = EMFPuzzleRoot.GetComponent<RectTransform>();
        rootPuzzleSlot.piece = EMFPuzzleRoot;
        rootPuzzleSlot.orientation = 1;
        rootPuzzleSlot.connections = new EMFPuzzleSlot[3];

        Vector2 rootPos = EMFPuzzleRoot.transform.position;
        traveledPositions.Add(rootPos);

        //GenerateSlots(rootPuzzleSlot, 0, rootPos, 1);
        //GenerateSlots(rootPuzzleSlot, 1, rootPos, 1);
        //GenerateSlots(rootPuzzleSlot, 2, rootPos, 1);
    }

    private void GenerateSlots(EMFPuzzleSlot previousPuzzleSlot, int direction, Vector2 pos, int localDepth)
    {
        if (localDepth > puzzleDepth)
            return;

        int newOrientation = previousPuzzleSlot.orientation * -1;
        Vector2 nPos = pos + slotDistance * GetDirectionByOrientation(newOrientation, direction);

        if (traveledPositions.Contains(nPos))
            return;

        traveledPositions.Add(nPos);

        EMFPuzzleSlot puzzleSlot = new EMFPuzzleSlot
        {
            slot = Instantiate(EMFPuzzleSlotPrefab, nPos, Quaternion.identity, transform).GetComponent<RectTransform>(),
            piece = null,
            orientation = newOrientation,
            connections = new EMFPuzzleSlot[3]
        };

        previousPuzzleSlot.connections[CorrectConnectionIndex(previousPuzzleSlot.orientation, direction)] = puzzleSlot;

        GenerateSlots(puzzleSlot, 0, nPos, localDepth+1);
        GenerateSlots(puzzleSlot, 1, nPos, localDepth+1);
        GenerateSlots(puzzleSlot, 2, nPos, localDepth+1);
    }

    private Vector2 GetDirectionByOrientation(int orientation, int direction)
    {
        Vector2 dir = new Vector2();
        
        switch (direction) 
        {
            case 0:
                dir = orientation == 1 ? Vector2.right : Vector2.left;
                break;
            case 1:
                dir = orientation == 1 ? Vector2.down : Vector2.up;
                break;
            case 2:
                dir = orientation == 1 ? Vector2.left : Vector2.right;
                break;
        }

        return dir;
    }

    private int CorrectConnectionIndex(int orientation, int direction)
    {
        if (direction == 1)
            return 1;

        if (direction == 0 && orientation == -1)
            return 2;

        if (direction == 2 && orientation == -1)
            return 0;

        return direction;
    }

    private void HandleSlotBehavior()
    {

    }

    public bool IsOverPuzzleSlot(out RectTransform puzzleSlot)
    {
        puzzleSlot = null;
        return false;
    }

    public MonitorScreen GetMonitor()
    {
        return _monitor;
    }

    [System.Serializable]
    private struct EMFPuzzleSlot 
    {
        public RectTransform slot;
        public EMFPuzzlePiece piece;
        public int orientation;
        public EMFPuzzleSlot[] connections;
    }

}
