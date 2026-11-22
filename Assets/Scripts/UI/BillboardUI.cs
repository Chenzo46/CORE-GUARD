using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform.position);
    }
}
