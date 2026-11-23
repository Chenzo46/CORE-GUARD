using UnityEngine;

public class GearRotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 5f;

    void Update()
    {
        transform.Rotate(rotateSpeed * Time.deltaTime, 0f, 0f);    
    }
}
