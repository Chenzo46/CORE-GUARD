using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonMouseSizer : MonoBehaviour
{
    [SerializeField] private Canvas parentCanvas;

    [SerializeField] private float maxDist = 10f;
    [SerializeField] private float maxScale = 1.5f;

    private RectTransform rt;


    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void Update()
    {
        ScaleButtonSize();
    }

    private void ScaleButtonSize()
    {
        rt.localScale = new Vector3(GetLerpScale(), GetLerpScale(), 1f);
    }

    private float GetLerpScale()
    {
        return Mathf.Lerp(1f, maxScale,  1 - Vector2.Distance(GetMousePosition(), rt.anchoredPosition) / maxDist);
    }

    private Vector2 GetMousePosition()
    {

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
           parentCanvas.transform as RectTransform,
           Mouse.current.position.ReadValue(),
           null,
           out Vector2 canvasPos
       );

        return canvasPos;
    }

    public void SetParentCanvas(Canvas pc)
    {
        parentCanvas = pc;
    }
}

