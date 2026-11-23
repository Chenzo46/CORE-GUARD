using UnityEngine;

public class Cable : MonoBehaviour
{
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private Transform plugReceiver;
    [SerializeField] private Rigidbody plugRb;

    private bool isPluggedIn = false;

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

    public void PlugIn()
    {
        if (isPluggedIn)
            return;

        AudioManager.Singleton.PlaySound("plug-in");

        isPluggedIn = true;

        Item plugItem = plugRb.GetComponent<Item>();
        plugItem.Drop();
        plugItem.SetInteractionEnabled(false);

        plugRb.isKinematic = true;
        plugRb.transform.position = plugReceiver.position;
        plugRb.transform.rotation = plugReceiver.rotation;
    }

    public bool IsPluggedIn()
    {
        return isPluggedIn;
    }
}
