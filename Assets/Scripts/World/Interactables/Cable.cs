using UnityEngine;

public class Cable : MonoBehaviour
{
    [SerializeField] private LineRenderer _lr;

    private void Update()
    {
        SetCablePoints();
    }

    private void SetCablePoints()
    {
        _lr.positionCount = transform.childCount;

        for (int idx = 0; idx < transform.childCount; idx++)
        {
            _lr.SetPosition(idx, transform.GetChild(idx).position);
        }
    }
}
