using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasScript : MonoBehaviour
{
    Canvas _canvas;
    public Canvas Canvas
    {
        get
        {
            if (!_canvas) _canvas = GetComponent<Canvas>();
            return _canvas;
        }
    }
}