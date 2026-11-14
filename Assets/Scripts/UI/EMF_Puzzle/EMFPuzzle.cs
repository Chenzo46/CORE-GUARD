using System.Collections.Generic;
using UnityEngine;

public class EMFPuzzle : MonoBehaviour
{
    [SerializeField] private RectTransform CursorObject;

    // Maybe we can generate a puzzle based off of a root prefab??
    [SerializeField] private GameObject EMFPuzzleRootPrefab;
    [SerializeField] private GameObject EMFPuzzleSlotPrefab;
    [SerializeField] private int puzzleDepth = 1;
    [SerializeField] private float slotDistance = 1;
    private EMFPuzzlePiece EMFPuzzleRoot;

    //private List<EMFPuzzleSlot> puzzleSlots = new List<EMFPuzzleSlot>();
    private EMFPuzzleSlot rootPuzzleSlot = new EMFPuzzleSlot();

    private List<Vector2> traveledPositions = new List<Vector2>();

    private void Awake()
    {
        InitializeEMFPuzzle();
    }

    public RectTransform GetCursorObject()
    {
        return CursorObject;
    }

    private readonly HashSet<Vector2Int> traveledCells = new HashSet<Vector2Int>();

    private struct PendingSlot
    {
        public EMFPuzzleSlot slot;
        public Vector2Int cell;
        public int depth;
    }

    private void InitializeEMFPuzzle()
    {
        EMFPuzzleRoot = Instantiate(EMFPuzzleRootPrefab, Vector2.zero, Quaternion.identity, transform).GetComponent<EMFPuzzlePiece>();

        rootPuzzleSlot.slot = EMFPuzzleRoot.GetComponent<RectTransform>();
        rootPuzzleSlot.piece = EMFPuzzleRoot;
        rootPuzzleSlot.orientation = 1;
        rootPuzzleSlot.connections = new EMFPuzzleSlot[3];

        Vector2 rootPos = EMFPuzzleRoot.transform.position;
        GenerateSlots(rootPos);
    }

    private void GenerateSlots(Vector2 rootPos)
    {
        traveledCells.Clear();

        var queue = new Queue<PendingSlot>();
        var rootCell = Vector2Int.zero;
        traveledCells.Add(rootCell);
        queue.Enqueue(new PendingSlot { slot = rootPuzzleSlot, cell = rootCell, depth = 0 });

        while (queue.Count > 0)
        {
            PendingSlot current = queue.Dequeue();

            // Slots at this depth exist, but don't expand past puzzleDepth
            if (current.depth >= puzzleDepth)
                continue;

            int newOrientation = current.slot.orientation * -1;

            for (int direction = 0; direction < 3; direction++)
            {
                Vector2Int nCell = current.cell + GetDirectionByOrientation(newOrientation, direction);

                // Integer grid coords make this check exact
                if (!traveledCells.Add(nCell))
                    continue;

                Vector2 nPos = rootPos + (Vector2)nCell * slotDistance;

                EMFPuzzleSlot puzzleSlot = new EMFPuzzleSlot
                {
                    slot = Instantiate(EMFPuzzleSlotPrefab, nPos, Quaternion.identity, transform).GetComponent<RectTransform>(),
                    piece = null,
                    orientation = newOrientation,
                    connections = new EMFPuzzleSlot[3]
                };

                current.slot.connections[CorrectConnectionIndex(current.slot.orientation, direction)] = puzzleSlot;

                queue.Enqueue(new PendingSlot { slot = puzzleSlot, cell = nCell, depth = current.depth + 1 });
            }
        }
    }

    private Vector2Int GetDirectionByOrientation(int orientation, int direction)
    {
        switch (direction)
        {
            case 0: return orientation == 1 ? Vector2Int.right : Vector2Int.left;
            case 1: return orientation == 1 ? Vector2Int.down : Vector2Int.up;
            case 2: return orientation == 1 ? Vector2Int.left : Vector2Int.right;
            default: return Vector2Int.zero;
        }
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

    private class EMFPuzzleSlot
    {
        public RectTransform slot;
        public EMFPuzzlePiece piece;
        public int orientation;
        public EMFPuzzleSlot[] connections;
    }

}
